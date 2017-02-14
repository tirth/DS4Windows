using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using DS4Lib.Hid;

namespace DS4Lib.DS4
{
    public static class Devices
    {
        private static readonly Dictionary<string, Device> DS4s = new Dictionary<string, Device>();
        private static readonly HashSet<string> DevicePaths = new HashSet<string>();
        public static bool IsExclusiveMode = false;

        private static string DevicePathToInstanceId(string devicePath)
        {
            var deviceInstanceId = devicePath;
            deviceInstanceId = deviceInstanceId.Remove(0, deviceInstanceId.LastIndexOf('\\') + 1);
            deviceInstanceId = deviceInstanceId.Remove(deviceInstanceId.LastIndexOf('{'));
            deviceInstanceId = deviceInstanceId.Replace('#', '\\');
            if (deviceInstanceId.EndsWith("\\"))
                deviceInstanceId = deviceInstanceId.Remove(deviceInstanceId.Length - 1);

            return deviceInstanceId;
        }

        // enumerates DS4 controllers in the system
        public static void FindControllers()
        {
            Trace.WriteLine($"Finding controllers");

            lock (DS4s)
            {
                int[] pid = {0xBA0, 0x5C4, 0x09CC};

                // Sort Bluetooth first in case USB is also connected on the same controller.
                var hDevices = HidDevices.Enumerate(0x054C, pid).OrderBy(Device.HidConnectionType);

                foreach (var hDevice in hDevices)
                {
                    if (DevicePaths.Contains(hDevice.DevicePath))
                        continue; // BT/USB endpoint already open once

                    if (!hDevice.IsOpen)
                    {
                        hDevice.OpenDevice(IsExclusiveMode);
                        if (!hDevice.IsOpen && IsExclusiveMode)
                        {
                            try
                            {
                                var identity = WindowsIdentity.GetCurrent();
                                var principal = new WindowsPrincipal(identity);
                                var elevated = principal.IsInRole(WindowsBuiltInRole.Administrator);

                                Trace.WriteLine($"We elevated? {elevated}");

                                if (!elevated)
                                {
                                    // Launches an elevated child process to re-enable device
                                    var exeName = Process.GetCurrentProcess().MainModule.FileName;
                                    var startInfo = new ProcessStartInfo(exeName);
                                    startInfo.Verb = "runas";
                                    startInfo.Arguments = "re-enabledevice " + DevicePathToInstanceId(hDevice.DevicePath);
                                    var child = Process.Start(startInfo);
                                    if (!child.WaitForExit(5000))
                                    {
                                        child.Kill();
                                    }
                                    else if (child.ExitCode == 0)
                                    {
                                        hDevice.OpenDevice(IsExclusiveMode);
                                    }
                                }
                                else
                                {
                                    ReEnableDevice(DevicePathToInstanceId(hDevice.DevicePath));
                                    hDevice.OpenDevice(IsExclusiveMode);
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }

                        // TODO in exclusive mode, try to hold both open when both are connected
                        if (IsExclusiveMode && !hDevice.IsOpen)
                            hDevice.OpenDevice(false);
                    }
                    if (hDevice.IsOpen)
                    {
                        if (DS4s.ContainsKey(hDevice.readSerial()))
                            continue; // happens when the BT endpoint already is open and the USB is plugged into the same host
                        else
                        {
                            var ds4Device = new Device(hDevice);
                            ds4Device.Removal += On_Removal;
                            DS4s.Add(ds4Device.MacAddress, ds4Device);
                            DevicePaths.Add(hDevice.DevicePath);
                            ds4Device.StartUpdate();
                        }
                    }
                }
            }
        }

        //allows to get DS4Device by specifying unique MAC address
        //format for MAC address is XX:XX:XX:XX:XX:XX
        public static Device GetController(string mac)
        {
            lock (DS4s)
            {
                Device device = null;
                try
                {
                    DS4s.TryGetValue(mac, out device);
                }
                catch (ArgumentNullException)
                {
                }
                return device;
            }
        }

        //returns DS4 controllers that were found and are running
        public static IEnumerable<Device> GetControllers()
        {
            lock (DS4s)
            {
                var controllers = new Device[DS4s.Count];
                DS4s.Values.CopyTo(controllers, 0);
                return controllers;
            }
        }

        public static void StopControllers()
        {
            lock (DS4s)
            {
                var devices = GetControllers();
                foreach (var device in devices)
                {
                    device.StopUpdate();
                    device.HidDevice.CloseDevice();
                }
                DS4s.Clear();
                DevicePaths.Clear();
            }
        }

        //called when devices is diconnected, timed out or has input reading failure
        public static void On_Removal(object sender, EventArgs e)
        {
            lock (DS4s)
            {
                var device = (Device)sender;
                device.HidDevice.CloseDevice();
                DS4s.Remove(device.MacAddress);
                DevicePaths.Remove(device.HidDevice.DevicePath);
            }
        }

        // TODO: NativeMethods stuff, HidLib extension
        public static void ReEnableDevice(string deviceInstanceId)
        {
            bool success;

            var hidGuid = new Guid();
            NativeMethods.HidD_GetHidGuid(ref hidGuid);

            var deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref hidGuid, deviceInstanceId, 0,
                NativeMethods.DIGCF_PRESENT | NativeMethods.DIGCF_DEVICEINTERFACE);

            var deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize = Marshal.SizeOf(deviceInfoData);

            success = NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, 0, ref deviceInfoData);
            if (!success)
                throw new Exception("Error getting device info data, error code = " + Marshal.GetLastWin32Error());

            success = NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, 1, ref deviceInfoData); // Checks that we have a unique device
            if (success)
                throw new Exception("Can't find unique device");

            var propChangeParams = new NativeMethods.SP_PROPCHANGE_PARAMS();
            propChangeParams.classInstallHeader.cbSize = Marshal.SizeOf(propChangeParams.classInstallHeader);
            propChangeParams.classInstallHeader.installFunction = NativeMethods.DIF_PROPERTYCHANGE;
            propChangeParams.stateChange = NativeMethods.DICS_DISABLE;
            propChangeParams.scope = NativeMethods.DICS_FLAG_GLOBAL;
            propChangeParams.hwProfile = 0;

            success = NativeMethods.SetupDiSetClassInstallParams(deviceInfoSet, ref deviceInfoData, ref propChangeParams,
                Marshal.SizeOf(propChangeParams));
            if (!success)
                throw new Exception("Error setting class install params, error code = " + Marshal.GetLastWin32Error());

            success = NativeMethods.SetupDiCallClassInstaller(NativeMethods.DIF_PROPERTYCHANGE, deviceInfoSet, ref deviceInfoData);
            if (!success)
                throw new Exception("Error disabling device, error code = " + Marshal.GetLastWin32Error());

            propChangeParams.stateChange = NativeMethods.DICS_ENABLE;
            success = NativeMethods.SetupDiSetClassInstallParams(deviceInfoSet, ref deviceInfoData, ref propChangeParams,
                Marshal.SizeOf(propChangeParams));
            if (!success)
                throw new Exception("Error setting class install params, error code = " + Marshal.GetLastWin32Error());

            success = NativeMethods.SetupDiCallClassInstaller(NativeMethods.DIF_PROPERTYCHANGE, deviceInfoSet, ref deviceInfoData);
            if (!success)
                throw new Exception("Error enabling device, error code = " + Marshal.GetLastWin32Error());

            NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
        }
    }
}