using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using DS4Lib.Hid;

namespace DS4Lib.DS4
{
    public enum ConnectionType : byte
    {
        BT,
        USB
    }; // Prioritize Bluetooth when both are connected.

    /**
     * The haptics engine uses a stack of these states representing the light bar and rumble motor settings.
     * It (will) handle composing them and the details of output report management.
     */

    public struct HapticState
    {
        public LightBarColour LightBarColour;
        public bool LightBarExplicitlyOff;
        public byte LightBarFlashDurationOn, LightBarFlashDurationOff;
        public byte RumbleMotorStrengthLeftHeavySlow, RumbleMotorStrengthRightLightFast;
        public bool RumbleMotorsExplicitlyOff;

        public bool IsLightBarSet() 
            => LightBarExplicitlyOff || LightBarColour.Red != 0 || LightBarColour.Green != 0 || LightBarColour.Blue != 0;

        public bool IsRumbleSet() 
            => RumbleMotorsExplicitlyOff || RumbleMotorStrengthLeftHeavySlow != 0 || RumbleMotorStrengthRightLightFast != 0;
    }

    public class Device
    {
        private const int BtOutputReportLength = 78;
        private const int BtInputReportLength = 547;

        public HidDevice HidDevice { get; }
        public bool IsExclusive => HidDevice.IsExclusive;
        public string MacAddress { get; }

        public ConnectionType ConnectionType { get; }

        private readonly State _cState = new State();
        private readonly State _pState = new State();
        private readonly byte[] _accel = new byte[6];
        private readonly byte[] _gyro = new byte[6];
        private readonly byte[] _inputReport;
        private readonly byte[] _btInputReport;
        private readonly byte[] _outputReportBuffer, _outputReport;
        private Thread _ds4Input, _ds4Output;
        public DateTime LastActive = DateTime.UtcNow;
        public DateTime FirstActive = DateTime.UtcNow;

        public event EventHandler<EventArgs> Report;
        public event EventHandler<EventArgs> Removal;

        public bool IsDisconnecting { get; private set; }

        public int IdleTimeout { get; set; } // behavior only active when > 0

        public int Battery { get; private set; }
        public bool Charging { get; private set; }

        public byte RightLightFastRumble { get; set; }
        public byte LeftHeavySlowRumble { get; set; }

        public LightBarColour LightBarColour { get; set; }
        public byte LightBarOnDuration { get; set; }
        public byte LightBarOffDuration { get; set; }

        public Touchpad Touchpad { get; }

        public SixAxis SixAxis { get; }

        public static ConnectionType HidConnectionType(HidDevice hidDevice)
            => hidDevice.Capabilities.InputReportByteLength == 64 ? ConnectionType.USB : ConnectionType.BT;

        public Device(HidDevice hidDevice)
        {
            HidDevice = hidDevice;
            ConnectionType = HidConnectionType(HidDevice);
            MacAddress = HidDevice.readSerial();

            if (ConnectionType == ConnectionType.USB)
            {
                _inputReport = new byte[64];
                _outputReport = new byte[HidDevice.Capabilities.OutputReportByteLength];
                _outputReportBuffer = new byte[HidDevice.Capabilities.OutputReportByteLength];
            }
            else
            {
                _btInputReport = new byte[BtInputReportLength];
                _inputReport = new byte[_btInputReport.Length - 2];
                _outputReport = new byte[BtOutputReportLength];
                _outputReportBuffer = new byte[BtOutputReportLength];
            }

            Touchpad = new Touchpad();
            SixAxis = new SixAxis();
        }

        public void StartUpdate()
        {
            if (_ds4Input == null)
            {
                Console.WriteLine(MacAddress + " " + DateTime.UtcNow.ToString("o") + "> start");
                SendOutputReport(true); // initialize the output report

                _ds4Output = new Thread(PerformDs4Output) {Name = "DS4 Output thread: " + MacAddress};
                _ds4Output.Start();

                _ds4Input = new Thread(PerformDs4Input) {Name = "DS4 Input thread: " + MacAddress};
                _ds4Input.Start();
            }
            else
                Console.WriteLine("Thread already running for DS4: " + MacAddress);
        }

        public void StopUpdate()
        {
            if (_ds4Input.ThreadState != System.Threading.ThreadState.Stopped || _ds4Input.ThreadState != System.Threading.ThreadState.Aborted)
            {
                try
                {
                    _ds4Input.Abort();
                    _ds4Input.Join();
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
            if (_ds4Output.ThreadState != System.Threading.ThreadState.Stopped || _ds4Output.ThreadState != System.Threading.ThreadState.Aborted)
            {
                try
                {
                    _ds4Output.Abort();
                    _ds4Output.Join();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private bool WriteOutput()
        {
            return ConnectionType == ConnectionType.BT
                ? HidDevice.WriteOutputReportViaControl(_outputReport)
                : HidDevice.WriteOutputReportViaInterrupt(_outputReport, 8);
        }

        private void PerformDs4Output()
        {
            lock (_outputReport)
            {
                var lastError = 0;
                while (true)
                {
                    if (WriteOutput())
                    {
                        lastError = 0;
                        if (testRumble.IsRumbleSet()) // repeat test rumbles periodically; rumble has auto-shut-off in the DS4 firmware
                            Monitor.Wait(_outputReport, 10000); // DS4 firmware stops it after 5 seconds, so let the motors rest for that long, too.
                        else
                            Monitor.Wait(_outputReport);
                    }
                    else
                    {
                        var thisError = Marshal.GetLastWin32Error();
                        if (lastError != thisError)
                        {
                            Console.WriteLine($"{MacAddress} {DateTime.UtcNow:o}> encountered write failure: {thisError}");
                            lastError = thisError;
                        }
                    }
                }
            }
        }

        /** Is the device alive and receiving valid sensor input reports? */

        public bool IsAlive => priorInputReport30 != 0xff;

        private byte priorInputReport30 = 0xff;
        public double Latency;
        bool warn;
        public string error;

        private void PerformDs4Input()
        {
            FirstActive = DateTime.UtcNow;

            var readTimeout = new System.Timers.Timer(); // Await 30 seconds for the initial packet, then 3 seconds thereafter.
            readTimeout.Elapsed += (sender, args) => HidDevice.CancelIO();

            var latencies = new List<long>();
            long oldtime = 0;

            var sw = Stopwatch.StartNew();

            while (true)
            {
                var currentError = string.Empty;

                latencies.Add(sw.ElapsedMilliseconds - oldtime);
                oldtime = sw.ElapsedMilliseconds;

                if (latencies.Count > 100)
                    latencies.RemoveAt(0);

                Latency = latencies.Average();

                if (Latency > 10 && !warn && sw.ElapsedMilliseconds > 4000)
                    warn = true;
                else if (Latency <= 10 && warn)
                    warn = false;

                if (readTimeout.Interval != 3000.0)
                {
                    if (readTimeout.Interval != 30000.0)
                        readTimeout.Interval = 30000.0;
                    else
                        readTimeout.Interval = 3000.0;
                }
                readTimeout.Enabled = true;
                if (ConnectionType != ConnectionType.USB)
                {
                    var res = HidDevice.ReadFile(_btInputReport);
                    readTimeout.Enabled = false;
                    if (res == HidDevice.ReadStatus.Success)
                    {
                        Array.Copy(_btInputReport, 2, _inputReport, 0, _inputReport.Length);
                    }
                    else
                    {
                        Console.WriteLine(MacAddress + " " + DateTime.UtcNow.ToString("o") + "> disconnect due to read failure: " +
                                          Marshal.GetLastWin32Error());
                        SendOutputReport(true); // Kick Windows into noticing the disconnection.
                        StopOutputUpdate();
                        IsDisconnecting = true;
                        Removal?.Invoke(this, EventArgs.Empty);
                        return;
                    }
                }
                else
                {
                    var res = HidDevice.ReadFile(_inputReport);
                    readTimeout.Enabled = false;
                    if (res != HidDevice.ReadStatus.Success)
                    {
                        Console.WriteLine(MacAddress + " " + DateTime.UtcNow.ToString("o") + "> disconnect due to read failure: " +
                                          Marshal.GetLastWin32Error());
                        StopOutputUpdate();
                        IsDisconnecting = true;
                        Removal?.Invoke(this, EventArgs.Empty);
                        return;
                    }
                }
                if (ConnectionType == ConnectionType.BT && _btInputReport[0] != 0x11)
                {
                    //Received incorrect report, skip it
                    continue;
                }
                var utcNow = DateTime.UtcNow; // timestamp with UTC in case system time zone changes
                resetHapticState();
                _cState.ReportTimeStamp = utcNow;
                _cState.LX = _inputReport[1];
                _cState.LY = _inputReport[2];
                _cState.RX = _inputReport[3];
                _cState.RY = _inputReport[4];
                _cState.L2 = _inputReport[8];
                _cState.R2 = _inputReport[9];

                _cState.Triangle = (_inputReport[5] & (1 << 7)) != 0;
                _cState.Circle = (_inputReport[5] & (1 << 6)) != 0;
                _cState.Cross = (_inputReport[5] & (1 << 5)) != 0;
                _cState.Square = (_inputReport[5] & (1 << 4)) != 0;
                _cState.DpadUp = (_inputReport[5] & (1 << 3)) != 0;
                _cState.DpadDown = (_inputReport[5] & (1 << 2)) != 0;
                _cState.DpadLeft = (_inputReport[5] & (1 << 1)) != 0;
                _cState.DpadRight = (_inputReport[5] & (1 << 0)) != 0;

                //Convert dpad into individual On/Off bits instead of a clock representation
                var dpadState = (byte)(
                    ((_cState.DpadRight ? 1 : 0) << 0) |
                    ((_cState.DpadLeft ? 1 : 0) << 1) |
                    ((_cState.DpadDown ? 1 : 0) << 2) |
                    ((_cState.DpadUp ? 1 : 0) << 3));

                switch (dpadState)
                {
                    case 0:
                        _cState.DpadUp = true;
                        _cState.DpadDown = false;
                        _cState.DpadLeft = false;
                        _cState.DpadRight = false;
                        break;
                    case 1:
                        _cState.DpadUp = true;
                        _cState.DpadDown = false;
                        _cState.DpadLeft = false;
                        _cState.DpadRight = true;
                        break;
                    case 2:
                        _cState.DpadUp = false;
                        _cState.DpadDown = false;
                        _cState.DpadLeft = false;
                        _cState.DpadRight = true;
                        break;
                    case 3:
                        _cState.DpadUp = false;
                        _cState.DpadDown = true;
                        _cState.DpadLeft = false;
                        _cState.DpadRight = true;
                        break;
                    case 4:
                        _cState.DpadUp = false;
                        _cState.DpadDown = true;
                        _cState.DpadLeft = false;
                        _cState.DpadRight = false;
                        break;
                    case 5:
                        _cState.DpadUp = false;
                        _cState.DpadDown = true;
                        _cState.DpadLeft = true;
                        _cState.DpadRight = false;
                        break;
                    case 6:
                        _cState.DpadUp = false;
                        _cState.DpadDown = false;
                        _cState.DpadLeft = true;
                        _cState.DpadRight = false;
                        break;
                    case 7:
                        _cState.DpadUp = true;
                        _cState.DpadDown = false;
                        _cState.DpadLeft = true;
                        _cState.DpadRight = false;
                        break;
                    case 8:
                        _cState.DpadUp = false;
                        _cState.DpadDown = false;
                        _cState.DpadLeft = false;
                        _cState.DpadRight = false;
                        break;
                }

                _cState.R3 = (_inputReport[6] & (1 << 7)) != 0;
                _cState.L3 = (_inputReport[6] & (1 << 6)) != 0;
                _cState.Options = (_inputReport[6] & (1 << 5)) != 0;
                _cState.Share = (_inputReport[6] & (1 << 4)) != 0;
                _cState.R1 = (_inputReport[6] & (1 << 1)) != 0;
                _cState.L1 = (_inputReport[6] & (1 << 0)) != 0;

                _cState.PS = (_inputReport[7] & (1 << 0)) != 0;
                _cState.TouchButton = (_inputReport[7] & (1 << 2 - 1)) != 0;
                _cState.FrameCounter = (byte)(_inputReport[7] >> 2);

                // Store Gyro and Accel values
                Array.Copy(_inputReport, 14, _accel, 0, 6);
                Array.Copy(_inputReport, 20, _gyro, 0, 6);
                SixAxis.handleSixaxis(_gyro, _accel, _cState);

                try
                {
                    Charging = (_inputReport[30] & 0x10) != 0;
                    Battery = (_inputReport[30] & 0x0f) * 10;
                    _cState.Battery = (byte)Battery;
                    if (_inputReport[30] != priorInputReport30)
                    {
                        priorInputReport30 = _inputReport[30];
                        Console.WriteLine(MacAddress + " " + DateTime.UtcNow.ToString("o") + "> power subsystem octet: 0x" +
                                          _inputReport[30].ToString("x02"));
                    }
                }
                catch
                {
                    currentError = "Index out of bounds: battery";
                }
                // XXX DS4State mapping needs fixup, turn touches into an array[4] of structs.  And include the touchpad details there instead.
                try
                {
                    for (int touches = _inputReport[-1 + Touchpad.TOUCHPAD_DATA_OFFSET - 1], touchOffset = 0; touches > 0; touches--, touchOffset += 9)
                    {
                        _cState.TouchPacketCounter = _inputReport[-1 + Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset];
                        _cState.Touch1 = _inputReport[0 + Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] >> 7 == 0; // >= 1 touch detected
                        _cState.Touch1Identifier = (byte)(_inputReport[0 + Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] & 0x7f);
                        _cState.Touch2 = _inputReport[4 + Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] >> 7 == 0; // 2 touches detected
                        _cState.Touch2Identifier = (byte)(_inputReport[4 + Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] & 0x7f);
                        _cState.TouchLeft = _inputReport[1 + Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] +
                                           (_inputReport[2 + Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] & 0xF) * 255 < 1920 * 2 / 5;
                        _cState.TouchRight = _inputReport[1 + Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] +
                                            (_inputReport[2 + Touchpad.TOUCHPAD_DATA_OFFSET + touchOffset] & 0xF) * 255 >= 1920 * 2 / 5;
                        // Even when idling there is still a touch packet indicating no touch 1 or 2
                        Touchpad.handleTouchpad(_inputReport, _cState, touchOffset);
                    }
                }
                catch
                {
                    currentError = "Index out of bounds: touchpad";
                }

                /* Debug output of incoming HID data:
                if (cState.L2 == 0xff && cState.R2 == 0xff)
                {
                    Console.Write(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + ">");
                    for (int i = 0; i < inputReport.Length; i++)
                        Console.Write(" " + inputReport[i].ToString("x2"));
                    Console.WriteLine();
                } */
                if (!IsIdle())
                    LastActive = utcNow;
                if (ConnectionType == ConnectionType.BT)
                {
                    var shouldDisconnect = false;
                    if (IdleTimeout > 0)
                    {
                        if (IsIdle())
                        {
                            var timeout = LastActive + TimeSpan.FromSeconds(IdleTimeout);
                            if (!Charging)
                                shouldDisconnect = utcNow >= timeout;
                        }
                    }
                    if (shouldDisconnect && DisconnectBT())
                        return; // all done
                }

                // XXX fix initialization ordering so the null checks all go away
                Report?.Invoke(this, EventArgs.Empty);

                SendOutputReport(false);
                if (!string.IsNullOrEmpty(error))
                    error = string.Empty;
                if (!string.IsNullOrEmpty(currentError))
                    error = currentError;
                _cState.CopyTo(_pState);
            }
        }

        public void FlushHid()
        {
            HidDevice.flush_Queue();
        }

        private void SendOutputReport(bool synchronous)
        {
            SetTestRumble();
            setHapticState();
            if (ConnectionType == ConnectionType.BT)
            {
                _outputReportBuffer[0] = 0x11;
                _outputReportBuffer[1] = 0x80;
                _outputReportBuffer[3] = 0xff;
                _outputReportBuffer[6] = RightLightFastRumble; //fast motor
                _outputReportBuffer[7] = LeftHeavySlowRumble; //slow motor
                _outputReportBuffer[8] = LightBarColour.Red; //red
                _outputReportBuffer[9] = LightBarColour.Green; //green
                _outputReportBuffer[10] = LightBarColour.Blue; //blue
                _outputReportBuffer[11] = LightBarOnDuration; //flash on duration
                _outputReportBuffer[12] = LightBarOffDuration; //flash off duration
            }
            else
            {
                _outputReportBuffer[0] = 0x05;
                _outputReportBuffer[1] = 0xff;
                _outputReportBuffer[4] = RightLightFastRumble; //fast motor
                _outputReportBuffer[5] = LeftHeavySlowRumble; //slow  motor
                _outputReportBuffer[6] = LightBarColour.Red; //red
                _outputReportBuffer[7] = LightBarColour.Green; //green
                _outputReportBuffer[8] = LightBarColour.Blue; //blue
                _outputReportBuffer[9] = LightBarOnDuration; //flash on duration
                _outputReportBuffer[10] = LightBarOffDuration; //flash off duration
            }
            lock (_outputReport)
            {
                if (synchronous)
                {
                    _outputReportBuffer.CopyTo(_outputReport, 0);
                    try
                    {
                        if (!WriteOutput())
                        {
                            Console.WriteLine(MacAddress + " " + DateTime.UtcNow.ToString("o") +
                                              "> encountered synchronous write failure: " + Marshal.GetLastWin32Error());
                            _ds4Output.Abort();
                            _ds4Output.Join();
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
                    for (var i = 0; !output && i < _outputReport.Length; i++)
                        output = _outputReport[i] != _outputReportBuffer[i];
                    if (output)
                    {
                        _outputReportBuffer.CopyTo(_outputReport, 0);
                        Monitor.Pulse(_outputReport);
                    }
                }
            }
        }

        public bool DisconnectBT()
        {
            if (MacAddress != null)
            {
                Console.WriteLine("Trying to disconnect BT device " + MacAddress);
                var btHandle = IntPtr.Zero;
                var IOCTL_BTH_DISCONNECT_DEVICE = 0x41000c;

                var btAddr = new byte[8];
                var sbytes = MacAddress.Split(':');
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
                    success = NativeMethods.DeviceIoControl(btHandle, IOCTL_BTH_DISCONNECT_DEVICE, ref lbtAddr, 8, IntPtr.Zero, 0, ref bytesReturned,
                        IntPtr.Zero);
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

        public void SetRumble(byte rightLightFastMotor, byte leftHeavySlowMotor)
        {
            testRumble.RumbleMotorStrengthRightLightFast = rightLightFastMotor;
            testRumble.RumbleMotorStrengthLeftHeavySlow = leftHeavySlowMotor;
            testRumble.RumbleMotorsExplicitlyOff = rightLightFastMotor == 0 && leftHeavySlowMotor == 0;
        }

        private void SetTestRumble()
        {
            if (!testRumble.IsRumbleSet())
                return;

            pushHapticState(testRumble);

            if (testRumble.RumbleMotorsExplicitlyOff)
                testRumble.RumbleMotorsExplicitlyOff = false;
        }

        public State GetCurrentState()
        {
            return _cState.Clone();
        }

        public State GetPreviousState()
        {
            return _pState.Clone();
        }

        public void GetExposedState(StateExposed expState, State state)
        {
            _cState.CopyTo(state);
            expState.Accel = _accel;
            expState.Gyro = _gyro;
        }

        public void GetCurrentState(State state)
        {
            _cState.CopyTo(state);
        }

        public void GetPreviousState(State state)
        {
            _pState.CopyTo(state);
        }

        private bool IsIdle()
        {
            if (_cState.Square || _cState.Cross || _cState.Circle || _cState.Triangle)
                return false;

            if (_cState.DpadUp || _cState.DpadLeft || _cState.DpadDown || _cState.DpadRight)
                return false;

            if (_cState.L3 || _cState.R3 || _cState.L1 || _cState.R1 || _cState.Share || _cState.Options)
                return false;

            if (_cState.L2 != 0 || _cState.R2 != 0)
                return false;

            // TODO calibrate to get an accurate jitter and center-play range and centered position
            const int slop = 64;
            if (_cState.LX <= 127 - slop || _cState.LX >= 128 + slop || _cState.LY <= 127 - slop || _cState.LY >= 128 + slop)
                return false;
            if (_cState.RX <= 127 - slop || _cState.RX >= 128 + slop || _cState.RY <= 127 - slop || _cState.RY >= 128 + slop)
                return false;

            if (_cState.Touch1 || _cState.Touch2 || _cState.TouchButton)
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
            byte rumbleMotorStrengthLeftHeavySlow = LeftHeavySlowRumble, rumbleMotorStrengthRightLightFast = RightLightFastRumble;
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
            return MacAddress;
        }
    }
}