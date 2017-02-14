﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using DS4Lib.Hid;

namespace DS4Lib.DS4
{
    public struct DS4Colour
    {
        public byte Red;
        public byte Green;
        public byte Blue;

        public DS4Colour(Color c)
        {
            Red = c.R;
            Green = c.G;
            Blue = c.B;
        }

        public DS4Colour(byte r, byte g, byte b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }

        public override bool Equals(object obj)
        {
            if (obj is DS4Colour dsc)
                return Red == dsc.Red && Green == dsc.Green && Blue == dsc.Blue;

            return false;
        }

        public Color ToColor => Color.FromArgb(Red, Green, Blue);
        public Color ToColorA
        {
            get
            {
                var alphacolor = Math.Max(Red, Math.Max(Green, Blue));
                var reg = Color.FromArgb(Red, Green, Blue);
                var full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), reg);
                return Color.FromArgb(alphacolor > 205 ? 255 : alphacolor + 50, full);
            }
        }

        private Color HuetoRGB(float hue, float light, Color rgb)
        {
            var L = (float)Math.Max(.5, light);
            var C = 1 - Math.Abs(2 * L - 1);
            var X = C * (1 - Math.Abs(hue / 60 % 2 - 1));
            var m = L - C / 2;
            float R = 0, G = 0, B = 0;
            if (light == 1) return Color.White;
            else if (rgb.R == rgb.G && rgb.G == rgb.B) return Color.White;
            else if (0 <= hue && hue < 60) { R = C; G = X; }
            else if (60 <= hue && hue < 120) { R = X; G = C; }
            else if (120 <= hue && hue < 180) { G = C; B = X; }
            else if (180 <= hue && hue < 240) { G = X; B = C; }
            else if (240 <= hue && hue < 300) { R = X; B = C; }
            else if (300 <= hue && hue < 360) { R = C; B = X; }
            return Color.FromArgb((int)((R + m) * 255), (int)((G + m) * 255), (int)((B + m) * 255));
        }

        public static bool TryParse(string value, ref DS4Colour ds4Colour)
        {
            try
            {
                var ss = value.Split(',');
                return byte.TryParse(ss[0], out ds4Colour.Red) && byte.TryParse(ss[1], out ds4Colour.Green) && byte.TryParse(ss[2], out ds4Colour.Blue);
            }
            catch { return false; }
        }
        public override string ToString() => $"Red: {Red} Green: {Green} Blue: {Blue}";
    }

    public enum ConnectionType : byte { BT, USB }; // Prioritize Bluetooth when both are connected.

    /**
     * The haptics engine uses a stack of these states representing the light bar and rumble motor settings.
     * It (will) handle composing them and the details of output report management.
     */
    public struct HapticState
    {
        public DS4Colour LightBarColour;
        public bool LightBarExplicitlyOff;
        public byte LightBarFlashDurationOn, LightBarFlashDurationOff;
        public byte RumbleMotorStrengthLeftHeavySlow, RumbleMotorStrengthRightLightFast;
        public bool RumbleMotorsExplicitlyOff;
        public bool IsLightBarSet()
        {
            return LightBarExplicitlyOff || LightBarColour.Red != 0 || LightBarColour.Green != 0 || LightBarColour.Blue != 0;
        }
        public bool IsRumbleSet()
        {
            return RumbleMotorsExplicitlyOff || RumbleMotorStrengthLeftHeavySlow != 0 || RumbleMotorStrengthRightLightFast != 0;
        }
    }

    public class Device
    {
        private const int BT_OUTPUT_REPORT_LENGTH = 78;
        private const int BT_INPUT_REPORT_LENGTH = 547;
        private HidDevice hDevice;
        private string Mac;
        private State cState = new State();
        private State pState = new State();
        private ConnectionType conType;
        private byte[] accel = new byte[6];
        private byte[] gyro = new byte[6];
        private byte[] inputReport;
        private byte[] btInputReport;
        private byte[] outputReportBuffer, outputReport;
        private readonly Touchpad touchpad;
        private readonly SixAxis sixAxis;
        private byte rightLightFastRumble;
        private byte leftHeavySlowRumble;
        private DS4Colour _ligtBarColour;
        private byte ledFlashOn, ledFlashOff;
        private Thread ds4Input, ds4Output;
        private int battery;
        public DateTime lastActive = DateTime.UtcNow;
        public DateTime firstActive = DateTime.UtcNow;
        private bool charging;
        public event EventHandler<EventArgs> Report;
        public event EventHandler<EventArgs> Removal;

        public HidDevice HidDevice => hDevice;
        public bool IsExclusive => HidDevice.IsExclusive;
        public bool IsDisconnecting { get; private set; }

        public string MacAddress => Mac;

        public ConnectionType ConnectionType => conType;
        public int IdleTimeout { get; set; } // behavior only active when > 0

        public int Battery => battery;
        public bool Charging => charging;

        public byte RightLightFastRumble
        {
            get { return rightLightFastRumble; }
            set
            {
                if (value == rightLightFastRumble) return;
                rightLightFastRumble = value;
            }
        }

        public byte LeftHeavySlowRumble
        {
            get { return leftHeavySlowRumble; }
            set
            {
                if (value == leftHeavySlowRumble) return;
                leftHeavySlowRumble = value;
            }
        }

        public DS4Colour LightBarColour
        {
            get { return _ligtBarColour; }
            set
            {
                if (_ligtBarColour.Red != value.Red || _ligtBarColour.Green != value.Green || _ligtBarColour.Blue != value.Blue)
                {
                    _ligtBarColour = value;
                }
            }
        }

        public byte LightBarOnDuration
        {
            get { return ledFlashOn; }
            set
            {
                if (ledFlashOn != value)
                {
                    ledFlashOn = value;
                }
            }
        }

        public byte LightBarOffDuration
        {
            get { return ledFlashOff; }
            set
            {
                if (ledFlashOff != value)
                {
                    ledFlashOff = value;
                }
            }
        }

        public Touchpad Touchpad { get { return touchpad; } }
        public SixAxis SixAxis { get { return sixAxis; } }

        public static ConnectionType HidConnectionType(HidDevice hidDevice)
        {
            return hidDevice.Capabilities.InputReportByteLength == 64 ? ConnectionType.USB : ConnectionType.BT;
        }

        public Device(HidDevice hidDevice)
        {
            hDevice = hidDevice;
            conType = HidConnectionType(hDevice);
            Mac = hDevice.readSerial();
            if (conType == ConnectionType.USB)
            {
                inputReport = new byte[64];
                outputReport = new byte[hDevice.Capabilities.OutputReportByteLength];
                outputReportBuffer = new byte[hDevice.Capabilities.OutputReportByteLength];
            }
            else
            {
                btInputReport = new byte[BT_INPUT_REPORT_LENGTH];
                inputReport = new byte[btInputReport.Length - 2];
                outputReport = new byte[BT_OUTPUT_REPORT_LENGTH];
                outputReportBuffer = new byte[BT_OUTPUT_REPORT_LENGTH];
            }
            touchpad = new Touchpad();
            sixAxis = new SixAxis();
        }

        public void StartUpdate()
        {
            if (ds4Input == null)
            {
                Console.WriteLine(MacAddress.ToString() + " " + DateTime.UtcNow.ToString("o") + "> start");
                sendOutputReport(true); // initialize the output report
                ds4Output = new Thread(performDs4Output);
                ds4Output.Name = "DS4 Output thread: " + Mac;
                ds4Output.Start();
                ds4Input = new Thread(performDs4Input);
                ds4Input.Name = "DS4 Input thread: " + Mac;
                ds4Input.Start();
            }
            else
                Console.WriteLine("Thread already running for DS4: " + Mac);
        }

        public void StopUpdate()
        {
            if (ds4Input.ThreadState != System.Threading.ThreadState.Stopped || ds4Input.ThreadState != System.Threading.ThreadState.Aborted)
            {
                try
                {
                    ds4Input.Abort();
                    ds4Input.Join();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            StopOutputUpdate();
        }

        private void StopOutputUpdate()
        {
            if (ds4Output.ThreadState != System.Threading.ThreadState.Stopped || ds4Output.ThreadState != System.Threading.ThreadState.Aborted)
            {
                try
                {
                    ds4Output.Abort();
                    ds4Output.Join();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private bool writeOutput()
        {
            return conType == ConnectionType.BT ? 
                hDevice.WriteOutputReportViaControl(outputReport) : 
                hDevice.WriteOutputReportViaInterrupt(outputReport, 8);
        }

        private void performDs4Output()
        {
            lock (outputReport)
            {
                var lastError = 0;
                while (true)
                {
                    if (writeOutput())
                    {
                        lastError = 0;
                        if (testRumble.IsRumbleSet()) // repeat test rumbles periodically; rumble has auto-shut-off in the DS4 firmware
                            Monitor.Wait(outputReport, 10000); // DS4 firmware stops it after 5 seconds, so let the motors rest for that long, too.
                        else
                            Monitor.Wait(outputReport);
                    }
                    else
                    {
                        var thisError = Marshal.GetLastWin32Error();
                        if (lastError != thisError)
                        {
                            Console.WriteLine(MacAddress.ToString() + " " + DateTime.UtcNow.ToString("o") + "> encountered write failure: " + thisError);
                            lastError = thisError;
                        }
                    }
                }
            }
        }

        /** Is the device alive and receiving valid sensor input reports? */
        public bool IsAlive()
        {
            return priorInputReport30 != 0xff;
        }
        private byte priorInputReport30 = 0xff;
        public double Latency;
        bool warn;
        public string error;
        private void performDs4Input()
        {
            firstActive = DateTime.UtcNow;
            var readTimeout = new System.Timers.Timer(); // Await 30 seconds for the initial packet, then 3 seconds thereafter.
            readTimeout.Elapsed += delegate { HidDevice.CancelIO(); };
            var Latency = new List<long>();
            long oldtime = 0;
            var sw = new Stopwatch();
            sw.Start();
            while (true)
            {
                var currerror = string.Empty;
                Latency.Add(sw.ElapsedMilliseconds - oldtime);
                oldtime = sw.ElapsedMilliseconds;

                if (Latency.Count > 100)
                    Latency.RemoveAt(0);

                this.Latency = Latency.Average();

                if (this.Latency > 10 && !warn && sw.ElapsedMilliseconds > 4000)
                {
                    warn = true;
                    //System.Diagnostics.Trace.WriteLine(System.DateTime.UtcNow.ToString("o") + "> " + "Controller " + /*this.DeviceNum*/ + 1 + " (" + this.MacAddress + ") is experiencing latency issues. Currently at " + Math.Round(this.Latency, 2).ToString() + "ms of recomended maximum 10ms");
                }
                else if (this.Latency <= 10 && warn) warn = false;

                if (readTimeout.Interval != 3000.0)
                {
                    if (readTimeout.Interval != 30000.0)
                        readTimeout.Interval = 30000.0;
                    else
                        readTimeout.Interval = 3000.0;
                }
                readTimeout.Enabled = true;
                if (conType != ConnectionType.USB)
                {
                    var res = hDevice.ReadFile(btInputReport);
                    readTimeout.Enabled = false;
                    if (res == HidDevice.ReadStatus.Success)
                    {
                        Array.Copy(btInputReport, 2, inputReport, 0, inputReport.Length);
                    }
                    else
                    {
                        Console.WriteLine(MacAddress.ToString() + " " + DateTime.UtcNow.ToString("o") + "> disconnect due to read failure: " + Marshal.GetLastWin32Error());
                        sendOutputReport(true); // Kick Windows into noticing the disconnection.
                        StopOutputUpdate();
                        IsDisconnecting = true;
                        if (Removal != null)
                            Removal(this, EventArgs.Empty);
                        return;

                    }
                }
                else
                {
                    var res = hDevice.ReadFile(inputReport);
                    readTimeout.Enabled = false;
                    if (res != HidDevice.ReadStatus.Success)
                    {
                        Console.WriteLine(MacAddress.ToString() + " " + DateTime.UtcNow.ToString("o") + "> disconnect due to read failure: " + Marshal.GetLastWin32Error());
                        StopOutputUpdate();
                        IsDisconnecting = true;
                        if (Removal != null)
                            Removal(this, EventArgs.Empty);
                        return;
                    }
                }
                if (ConnectionType == ConnectionType.BT && btInputReport[0] != 0x11)
                {
                    //Received incorrect report, skip it
                    continue;
                }
                var utcNow = DateTime.UtcNow; // timestamp with UTC in case system time zone changes
                resetHapticState();
                cState.ReportTimeStamp = utcNow;
                cState.LX = inputReport[1];
                cState.LY = inputReport[2];
                cState.RX = inputReport[3];
                cState.RY = inputReport[4];
                cState.L2 = inputReport[8];
                cState.R2 = inputReport[9];

                cState.Triangle = ((byte)inputReport[5] & (1 << 7)) != 0;
                cState.Circle = ((byte)inputReport[5] & (1 << 6)) != 0;
                cState.Cross = ((byte)inputReport[5] & (1 << 5)) != 0;
                cState.Square = ((byte)inputReport[5] & (1 << 4)) != 0;
                cState.DpadUp = ((byte)inputReport[5] & (1 << 3)) != 0;
                cState.DpadDown = ((byte)inputReport[5] & (1 << 2)) != 0;
                cState.DpadLeft = ((byte)inputReport[5] & (1 << 1)) != 0;
                cState.DpadRight = ((byte)inputReport[5] & (1 << 0)) != 0;

                //Convert dpad into individual On/Off bits instead of a clock representation
                byte dpad_state = 0;

                dpad_state = (byte)(
                ((cState.DpadRight ? 1 : 0) << 0) |
                ((cState.DpadLeft ? 1 : 0) << 1) |
                ((cState.DpadDown ? 1 : 0) << 2) |
                ((cState.DpadUp ? 1 : 0) << 3));

                switch (dpad_state)
                {
                    case 0: cState.DpadUp = true; cState.DpadDown = false; cState.DpadLeft = false; cState.DpadRight = false; break;
                    case 1: cState.DpadUp = true; cState.DpadDown = false; cState.DpadLeft = false; cState.DpadRight = true; break;
                    case 2: cState.DpadUp = false; cState.DpadDown = false; cState.DpadLeft = false; cState.DpadRight = true; break;
                    case 3: cState.DpadUp = false; cState.DpadDown = true; cState.DpadLeft = false; cState.DpadRight = true; break;
                    case 4: cState.DpadUp = false; cState.DpadDown = true; cState.DpadLeft = false; cState.DpadRight = false; break;
                    case 5: cState.DpadUp = false; cState.DpadDown = true; cState.DpadLeft = true; cState.DpadRight = false; break;
                    case 6: cState.DpadUp = false; cState.DpadDown = false; cState.DpadLeft = true; cState.DpadRight = false; break;
                    case 7: cState.DpadUp = true; cState.DpadDown = false; cState.DpadLeft = true; cState.DpadRight = false; break;
                    case 8: cState.DpadUp = false; cState.DpadDown = false; cState.DpadLeft = false; cState.DpadRight = false; break;
                }

                cState.R3 = ((byte)inputReport[6] & (1 << 7)) != 0;
                cState.L3 = ((byte)inputReport[6] & (1 << 6)) != 0;
                cState.Options = ((byte)inputReport[6] & (1 << 5)) != 0;
                cState.Share = ((byte)inputReport[6] & (1 << 4)) != 0;
                cState.R1 = ((byte)inputReport[6] & (1 << 1)) != 0;
                cState.L1 = ((byte)inputReport[6] & (1 << 0)) != 0;

                cState.PS = ((byte)inputReport[7] & (1 << 0)) != 0;
                cState.TouchButton = (inputReport[7] & (1 << 2 - 1)) != 0;
                cState.FrameCounter = (byte)(inputReport[7] >> 2);

                // Store Gyro and Accel values
                Array.Copy(inputReport, 14, accel, 0, 6);
                Array.Copy(inputReport, 20, gyro, 0, 6);
                sixAxis.handleSixaxis(gyro, accel, cState);

                try
                {
                    charging = (inputReport[30] & 0x10) != 0;
                    battery = (inputReport[30] & 0x0f) * 10;
                    cState.Battery = (byte)battery;
                    if (inputReport[30] != priorInputReport30)
                    {
                        priorInputReport30 = inputReport[30];
                        Console.WriteLine(MacAddress.ToString() + " " + DateTime.UtcNow.ToString("o") + "> power subsystem octet: 0x" + inputReport[30].ToString("x02"));
                    }
                }
                catch { currerror = "Index out of bounds: battery"; }
                // XXX DS4State mapping needs fixup, turn touches into an array[4] of structs.  And include the touchpad details there instead.
                try
                {
                    for (int touches = inputReport[-1 + Touchpad.TOUCHPAD_DATA_OFFSET - 1], touchOffset = 0; touches > 0; touches--, touchOffset += 9)
                    {
                        cState.TouchPacketCounter = inputReport[-1 + Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset];
                        cState.Touch1 = inputReport[0 + Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] >> 7 == 0; // >= 1 touch detected
                        cState.Touch1Identifier = (byte)(inputReport[0 + Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] & 0x7f);
                        cState.Touch2 = inputReport[4 + Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] >> 7 == 0; // 2 touches detected
                        cState.Touch2Identifier = (byte)(inputReport[4 + Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] & 0x7f);
                        cState.TouchLeft = inputReport[1 + Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] + (inputReport[2 + Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] & 0xF) * 255 < 1920 * 2 / 5;
                        cState.TouchRight = inputReport[1 + Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] + (inputReport[2 + Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] & 0xF) * 255 >= 1920 * 2 / 5;
                        // Even when idling there is still a touch packet indicating no touch 1 or 2
                        touchpad.handleTouchpad(inputReport, cState, touchOffset);
                    }
                }
                catch { currerror = "Index out of bounds: touchpad"; }

                /* Debug output of incoming HID data:
                if (cState.L2 == 0xff && cState.R2 == 0xff)
                {
                    Console.Write(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + ">");
                    for (int i = 0; i < inputReport.Length; i++)
                        Console.Write(" " + inputReport[i].ToString("x2"));
                    Console.WriteLine();
                } */
                if (!isDS4Idle())
                    lastActive = utcNow;
                if (conType == ConnectionType.BT)
                {
                    var shouldDisconnect = false;
                    if (IdleTimeout > 0)
                    {
                        if (isDS4Idle())
                        {
                            var timeout = lastActive + TimeSpan.FromSeconds(IdleTimeout);
                            if (!Charging)
                                shouldDisconnect = utcNow >= timeout;
                        }
                    }
                    if (shouldDisconnect && DisconnectBT())
                        return; // all done
                }
                // XXX fix initialization ordering so the null checks all go away
                if (Report != null)
                    Report(this, EventArgs.Empty);
                sendOutputReport(false);
                if (!string.IsNullOrEmpty(error))
                    error = string.Empty;
                if (!string.IsNullOrEmpty(currerror))
                    error = currerror;
                cState.CopyTo(pState);
            }
        }

        public void FlushHID()
        {
            hDevice.flush_Queue();
        }
        private void sendOutputReport(bool synchronous)
        {
            setTestRumble();
            setHapticState();
            if (conType == ConnectionType.BT)
            {
                outputReportBuffer[0] = 0x11;
                outputReportBuffer[1] = 0x80;
                outputReportBuffer[3] = 0xff;
                outputReportBuffer[6] = rightLightFastRumble; //fast motor
                outputReportBuffer[7] = leftHeavySlowRumble; //slow motor
                outputReportBuffer[8] = LightBarColour.Red; //red
                outputReportBuffer[9] = LightBarColour.Green; //green
                outputReportBuffer[10] = LightBarColour.Blue; //blue
                outputReportBuffer[11] = ledFlashOn; //flash on duration
                outputReportBuffer[12] = ledFlashOff; //flash off duration
            }
            else
            {
                outputReportBuffer[0] = 0x05;
                outputReportBuffer[1] = 0xff;
                outputReportBuffer[4] = rightLightFastRumble; //fast motor
                outputReportBuffer[5] = leftHeavySlowRumble; //slow  motor
                outputReportBuffer[6] = LightBarColour.Red; //red
                outputReportBuffer[7] = LightBarColour.Green; //green
                outputReportBuffer[8] = LightBarColour.Blue; //blue
                outputReportBuffer[9] = ledFlashOn; //flash on duration
                outputReportBuffer[10] = ledFlashOff; //flash off duration
            }
            lock (outputReport)
            {
                if (synchronous)
                {
                    outputReportBuffer.CopyTo(outputReport, 0);
                    try
                    {
                        if (!writeOutput())
                        {
                            Console.WriteLine(MacAddress.ToString() + " " + DateTime.UtcNow.ToString("o") + "> encountered synchronous write failure: " + Marshal.GetLastWin32Error());
                            ds4Output.Abort();
                            ds4Output.Join();
                        }
                    }
                    catch
                    {
                        // If it's dead already, don't worry about it.
                    }
                }
                else
                {
                    var output = false;
                    for (var i = 0; !output && i < outputReport.Length; i++)
                        output = outputReport[i] != outputReportBuffer[i];
                    if (output)
                    {
                        outputReportBuffer.CopyTo(outputReport, 0);
                        Monitor.Pulse(outputReport);
                    }
                }
            }
        }

        public bool DisconnectBT()
        {
            if (Mac != null)
            {
                Console.WriteLine("Trying to disconnect BT device " + Mac);
                var btHandle = IntPtr.Zero;
                var IOCTL_BTH_DISCONNECT_DEVICE = 0x41000c;

                var btAddr = new byte[8];
                var sbytes = Mac.Split(':');
                for (var i = 0; i < 6; i++)
                {
                    //parse hex byte in reverse order
                    btAddr[5 - i] = Convert.ToByte(sbytes[i], 16);
                }
                var lbtAddr = BitConverter.ToInt64(btAddr, 0);

                var p = new NativeMethods.BLUETOOTH_FIND_RADIO_PARAMS();
                p.dwSize = Marshal.SizeOf(typeof(NativeMethods.BLUETOOTH_FIND_RADIO_PARAMS));
                var searchHandle = NativeMethods.BluetoothFindFirstRadio(ref p, ref btHandle);
                var bytesReturned = 0;
                var success = false;
                while (!success && btHandle != IntPtr.Zero)
                {
                    success = NativeMethods.DeviceIoControl(btHandle, IOCTL_BTH_DISCONNECT_DEVICE, ref lbtAddr, 8, IntPtr.Zero, 0, ref bytesReturned, IntPtr.Zero);
                    NativeMethods.CloseHandle(btHandle);
                    if (!success)
                        if (!NativeMethods.BluetoothFindNextRadio(searchHandle, ref btHandle))
                            btHandle = IntPtr.Zero;

                }
                NativeMethods.BluetoothFindRadioClose(searchHandle);
                Console.WriteLine("Disconnect successful: " + success);
                success = true; // XXX return value indicates failure, but it still works?
                if (success)
                {
                    IsDisconnecting = true;
                    StopOutputUpdate();
                    if (Removal != null)
                        Removal(this, EventArgs.Empty);
                }
                return success;
            }
            return false;
        }

        private HapticState testRumble;
        public void setRumble(byte rightLightFastMotor, byte leftHeavySlowMotor)
        {
            testRumble.RumbleMotorStrengthRightLightFast = rightLightFastMotor;
            testRumble.RumbleMotorStrengthLeftHeavySlow = leftHeavySlowMotor;
            testRumble.RumbleMotorsExplicitlyOff = rightLightFastMotor == 0 && leftHeavySlowMotor == 0;
        }

        private void setTestRumble()
        {
            if (testRumble.IsRumbleSet())
            {
                pushHapticState(testRumble);
                if (testRumble.RumbleMotorsExplicitlyOff)
                    testRumble.RumbleMotorsExplicitlyOff = false;
            }
        }

        public State getCurrentState()
        {
            return cState.Clone();
        }

        public State getPreviousState()
        {
            return pState.Clone();
        }

        public void getExposedState(StateExposed expState, State state)
        {
            cState.CopyTo(state);
            expState.Accel = accel;
            expState.Gyro = gyro;
        }

        public void getCurrentState(State state)
        {
            cState.CopyTo(state);
        }

        public void getPreviousState(State state)
        {
            pState.CopyTo(state);
        }

        private bool isDS4Idle()
        {
            if (cState.Square || cState.Cross || cState.Circle || cState.Triangle)
                return false;
            if (cState.DpadUp || cState.DpadLeft || cState.DpadDown || cState.DpadRight)
                return false;
            if (cState.L3 || cState.R3 || cState.L1 || cState.R1 || cState.Share || cState.Options)
                return false;
            if (cState.L2 != 0 || cState.R2 != 0)
                return false;
            // TODO calibrate to get an accurate jitter and center-play range and centered position
            const int slop = 64;
            if (cState.LX <= 127 - slop || cState.LX >= 128 + slop || cState.LY <= 127 - slop || cState.LY >= 128 + slop)
                return false;
            if (cState.RX <= 127 - slop || cState.RX >= 128 + slop || cState.RY <= 127 - slop || cState.RY >= 128 + slop)
                return false;
            if (cState.Touch1 || cState.Touch2 || cState.TouchButton)
                return false;
            return true;
        }

        private HapticState[] hapticState = new HapticState[1];
        private int hapticStackIndex;
        private void resetHapticState()
        {
            hapticStackIndex = 0;
        }

        // Use the "most recently set" haptic state for each of light bar/motor.
        private void setHapticState()
        {
            var i = 0;
            var lightBarColor = LightBarColour;
            byte lightBarFlashDurationOn = LightBarOnDuration, lightBarFlashDurationOff = LightBarOffDuration;
            byte rumbleMotorStrengthLeftHeavySlow = LeftHeavySlowRumble, rumbleMotorStrengthRightLightFast = rightLightFastRumble;
            foreach (var haptic in hapticState)
            {
                if (i++ == hapticStackIndex)
                    break; // rest haven't been used this time
                if (haptic.IsLightBarSet())
                {
                    lightBarColor = haptic.LightBarColour;
                    lightBarFlashDurationOn = haptic.LightBarFlashDurationOn;
                    lightBarFlashDurationOff = haptic.LightBarFlashDurationOff;
                }
                if (haptic.IsRumbleSet())
                {
                    rumbleMotorStrengthLeftHeavySlow = haptic.RumbleMotorStrengthLeftHeavySlow;
                    rumbleMotorStrengthRightLightFast = haptic.RumbleMotorStrengthRightLightFast;
                }
            }
            LightBarColour = lightBarColor;
            LightBarOnDuration = lightBarFlashDurationOn;
            LightBarOffDuration = lightBarFlashDurationOff;
            LeftHeavySlowRumble = rumbleMotorStrengthLeftHeavySlow;
            RightLightFastRumble = rumbleMotorStrengthRightLightFast;
        }

        public void pushHapticState(HapticState hs)
        {
            if (hapticStackIndex == hapticState.Length)
            {
                var newHaptics = new HapticState[hapticState.Length + 1];
                Array.Copy(hapticState, newHaptics, hapticState.Length);
                hapticState = newHaptics;
            }
            hapticState[hapticStackIndex++] = hs;
        }

        override
        public string ToString()
        {
            return Mac;
        }
    }
}
