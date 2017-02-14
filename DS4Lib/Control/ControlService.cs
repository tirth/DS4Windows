using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DS4Lib.DS4;
using static DS4Lib.Control.Global;

namespace DS4Lib.Control
{
    public class ControlService
    {
        public readonly X360Device X360Bus;
        public Device[] Controllers = new Device[4];
        public Mouse[] touchPad = new Mouse[4];
        private bool running;
        private State[] MappedState = new State[4];
        private State[] CurrentState = new State[4];
        private State[] PreviousState = new State[4];
        public StateExposed[] ExposedState = new StateExposed[4];
        public bool recordingMacro = false;
        public event EventHandler<DebugEventArgs> Debug;
        public bool eastertime = false;
        private int eCode;
        bool[] buttonsdown = {false, false, false, false};
        List<DS4Controls> dcs = new List<DS4Controls>();
        bool[] held = new bool[4];
        int[] oldmouse = new int[4] {-1, -1, -1, -1};
        
        private class X360Data
        {
            public readonly byte[] Report = new byte[28];
            public readonly byte[] Rumble = new byte[8];
        }

        private readonly X360Data[] _processingData = new X360Data[4];

        public ControlService()
        {
            X360Bus = new X360Device();
            AddtoDS4List();
            for (var i = 0; i < Controllers.Length; i++)
            {
                _processingData[i] = new X360Data();
                MappedState[i] = new State();
                CurrentState[i] = new State();
                PreviousState[i] = new State();
                ExposedState[i] = new StateExposed(CurrentState[i]);
            }
        }

        void AddtoDS4List()
        {
            dcs.Add(DS4Controls.Cross);
            dcs.Add(DS4Controls.Cross);
            dcs.Add(DS4Controls.Circle);
            dcs.Add(DS4Controls.Square);
            dcs.Add(DS4Controls.Triangle);
            dcs.Add(DS4Controls.Options);
            dcs.Add(DS4Controls.Share);
            dcs.Add(DS4Controls.DpadUp);
            dcs.Add(DS4Controls.DpadDown);
            dcs.Add(DS4Controls.DpadLeft);
            dcs.Add(DS4Controls.DpadRight);
            dcs.Add(DS4Controls.PS);
            dcs.Add(DS4Controls.L1);
            dcs.Add(DS4Controls.R1);
            dcs.Add(DS4Controls.L2);
            dcs.Add(DS4Controls.R2);
            dcs.Add(DS4Controls.L3);
            dcs.Add(DS4Controls.R3);
            dcs.Add(DS4Controls.LXPos);
            dcs.Add(DS4Controls.LXNeg);
            dcs.Add(DS4Controls.LYPos);
            dcs.Add(DS4Controls.LYNeg);
            dcs.Add(DS4Controls.RXPos);
            dcs.Add(DS4Controls.RXNeg);
            dcs.Add(DS4Controls.RYPos);
            dcs.Add(DS4Controls.RYNeg);
            dcs.Add(DS4Controls.SwipeUp);
            dcs.Add(DS4Controls.SwipeDown);
            dcs.Add(DS4Controls.SwipeLeft);
            dcs.Add(DS4Controls.SwipeRight);
        }

        private async void WarnExclusiveModeFailure(Device device)
        {
            if (Devices.IsExclusiveMode && !device.IsExclusive)
            {
                await Task.Delay(5);

                var message = $"Couldn't open DS4 at {device.MacAddress}, check other programs";

                LogDebug(message, true);
                Log.LogToTray(message, true);
            }
        }

        public bool Start(bool showLog = true)
        {
            if (X360Bus.Open() && X360Bus.Start())
            {
                if (showLog)
                    LogDebug("Starting");

                Devices.IsExclusiveMode = UseExclusiveMode;
                if (showLog)
                {
                    LogDebug("Searching for controllers");
                    LogDebug(Devices.IsExclusiveMode ? "Using Exclusive" : "Using Shared");
                }
                try
                {
                    Devices.FindControllers();
                    var devices = Devices.GetControllers();
                    var ind = 0;
                    DS4LightBar.defualtLight = false;
                    foreach (var device in devices)
                    {
                        if (showLog)
                            LogDebug($"Found controller: {device.MacAddress} ({device.ConnectionType})");
                        WarnExclusiveModeFailure(device);
                        Controllers[ind] = device;
                        device.Removal -= Devices.On_Removal;
                        device.Removal += On_DS4Removal;
                        device.Removal += Devices.On_Removal;
                        touchPad[ind] = new Mouse(ind, device);
                        device.LightBarColour = MainColour[ind];
                        if (!DinputOnly[ind])
                            X360Bus.Plugin(ind);
                        device.Report += On_Report;
                        TouchPadOn(ind, device);
                        //string filename = ProfilePath[ind];
                        ind++;
                        if (showLog)
                            if (File.Exists(appdatapath + "\\Profiles\\" + ProfilePath[ind - 1] + ".xml"))
                            {
                                var profileLog = $"Controller {ind} is using profile {ProfilePath[ind - 1]}";
                                LogDebug(profileLog);
                                Log.LogToTray(profileLog);
                            }
                            else
                            {
                                var profileLog = $"Controller {ind} is not using a profile";
                                LogDebug(profileLog);
                                Log.LogToTray(profileLog);
                            }
                        if (ind >= 4) // out of Xinput devices!
                            break;
                    }
                }
                catch (Exception e)
                {
                    LogDebug(e.Message);
                    Log.LogToTray(e.Message);
                }
                running = true;
            }
            return true;
        }

        public bool Stop(bool showLog = true)
        {
            if (running)
            {
                running = false;

                if (showLog)
                    LogDebug("Stopping X360 controllers");

                var anyUnplugged = false;
                for (var i = 0; i < Controllers.Length; i++)
                {
                    if (Controllers[i] != null)
                    {
                        if (DCBTatStop && !Controllers[i].Charging && showLog)
                            Controllers[i].DisconnectBT();
                        else
                        {
                            DS4LightBar.forcelight[i] = false;
                            DS4LightBar.forcedFlash[i] = 0;
                            DS4LightBar.defualtLight = true;
                            DS4LightBar.updateLightBar(Controllers[i], i, CurrentState[i], ExposedState[i], touchPad[i]);
                            System.Threading.Thread.Sleep(50);
                        }
                        CurrentState[i].Battery = PreviousState[i].Battery = 0; // Reset for the next connection's initial status change.
                        X360Bus.Unplug(i);
                        anyUnplugged = true;
                        Controllers[i] = null;
                        touchPad[i] = null;
                    }
                }

                if (anyUnplugged)
                    System.Threading.Thread.Sleep(XINPUT_UNPLUG_SETTLE_TIME);

                X360Bus.UnplugAll();
                X360Bus.Stop();

                if (showLog)
                    LogDebug("Stopping DS4s");

                Devices.StopControllers();

                if (showLog)
                    LogDebug("Stopped DS4Windows");

                ControllerStatusChanged(this);
            }
            return true;
        }

        public void HotPlug()
        {
            Trace.WriteLine($"HOTPLUG!");

            if (!running)
                return;

            Devices.FindControllers();
            foreach (var device in Devices.GetControllers())
            {
                if (device.IsDisconnecting)
                    continue;

                if (Controllers.Any(c => c.MacAddress == device.MacAddress))
                    continue;

                for (var Index = 0; Index < Controllers.Length; Index++)
                    if (Controllers[Index] == null)
                    {
                        LogDebug($"Found controller: {device.MacAddress} ({device.ConnectionType})");
                        WarnExclusiveModeFailure(device);
                        Controllers[Index] = device;
                        device.Removal -= Devices.On_Removal;
                        device.Removal += On_DS4Removal;
                        device.Removal += Devices.On_Removal;
                        touchPad[Index] = new Mouse(Index, device);
                        device.LightBarColour = MainColour[Index];
                        device.Report += On_Report;
                        if (!DinputOnly[Index])
                            X360Bus.Plugin(Index);
                        TouchPadOn(Index, device);
                        //string filename = Path.GetFileName(ProfilePath[Index]);
                        if (File.Exists(appdatapath + "\\Profiles\\" + ProfilePath[Index] + ".xml"))
                        {
                            var profileLog = $"Controller {Index + 1} is using profile {ProfilePath[Index]}";
                            LogDebug(profileLog);
                            Log.LogToTray(profileLog);
                        }
                        else
                        {
                            var profileLog = $"Controller {Index + 1} is not using a profile";
                            LogDebug(profileLog);
                            Log.LogToTray(profileLog);
                        }

                        break;
                    }
            }
        }

        public void TouchPadOn(int ind, Device device)
        {
            ITouchpadBehaviour tPad = touchPad[ind];
            device.Touchpad.TouchButtonDown += tPad.touchButtonDown;
            device.Touchpad.TouchButtonUp += tPad.touchButtonUp;
            device.Touchpad.TouchesBegan += tPad.touchesBegan;
            device.Touchpad.TouchesMoved += tPad.touchesMoved;
            device.Touchpad.TouchesEnded += tPad.touchesEnded;
            device.Touchpad.TouchUnchanged += tPad.touchUnchanged;
            device.SixAxis.SixAccelMoved += tPad.sixaxisMoved;
            //LogDebug("Touchpad mode for " + device.MacAddress + " is now " + tmode.ToString());
            //Log.LogToTray("Touchpad mode for " + device.MacAddress + " is now " + tmode.ToString());
            ControllerStatusChanged(this);
        }

        public void TimeoutConnection(Device d)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                
                while (!d.IsAlive)
                {
                    if (sw.ElapsedMilliseconds < 1000)
                        System.Threading.Thread.SpinWait(500);
                    //If weve been waiting less than 1 second let the thread keep its processing chunk
                    else
                        System.Threading.Thread.Sleep(500);
                    //If weve been waiting more than 1 second give up some resources

                    if (sw.ElapsedMilliseconds > 5000) throw new TimeoutException(); //Weve waited long enough
                }
                sw.Reset();
            }
            catch (TimeoutException)
            {
                Stop(false);
                Start(false);
            }
        }

        public string getDS4ControllerInfo(int index)
        {
            if (Controllers[index] == null)
                return string.Empty;

            var d = Controllers[index];
            if (!d.IsAlive)
                //return "Connecting..."; // awaiting the first battery charge indication
            {
                var timeoutThread = new System.Threading.Thread(() => TimeoutConnection(d))
                {
                    IsBackground = true,
                    Name = "TimeoutFor" + d.MacAddress
                };
                timeoutThread.Start();
                return "Connecting...";
            }

            string battery;
            if (d.Charging)
                battery = d.Battery >= 100 ? "Charged" : $"Charging: {d.Battery}%";
            else
                battery = $"Battery: {d.Battery}%";

            return $"{d.MacAddress} ({d.ConnectionType}), {battery}";
            //return d.MacAddress + " (" + d.ConnectionType + "), Battery is " + battery + ", Touchpad in " + modeSwitcher[index].ToString();
        }

        public string GetDS4MacAddress(int index)
        {
            if (Controllers[index] == null)
                return string.Empty;

            var d = Controllers[index];
            if (!d.IsAlive)
                //return "Connecting..."; // awaiting the first battery charge indication
            {
                var TimeoutThread = new System.Threading.Thread(() => TimeoutConnection(d));
                TimeoutThread.IsBackground = true;
                TimeoutThread.Name = "TimeoutFor" + d.MacAddress.ToString();
                TimeoutThread.Start();

                return "Connecting...";
            }
            return d.MacAddress;
        }

        public string GetShortDS4ControllerInfo(int index)
        {
            if (Controllers[index] != null)
            {
                var d = Controllers[index];

                string battery;
                if (!d.IsAlive)
                    battery = "...";

                if (d.Charging)
                    if (d.Battery >= 100)
                        battery = "Full";
                    else
                        battery = d.Battery + "%+";
                else
                    battery = d.Battery + "%";

                return d.ConnectionType + " " + battery;
            }

            return "None";
        }

        public string GetDS4Battery(int index)
        {
            if (Controllers[index] != null)
            {
                var d = Controllers[index];

                string battery;
                if (!d.IsAlive)
                    battery = "...";

                if (d.Charging)
                    if (d.Battery >= 100)
                        battery = "Full";
                    else
                        battery = d.Battery + "%+";
                else
                    battery = d.Battery + "%";

                return battery;
            }

            return "N/A";
        }

        public string GetDS4Status(int index)
        {
            if (Controllers[index] == null)
                return "none";

            var d = Controllers[index];
            return d.ConnectionType + "";
        }


        private const int XINPUT_UNPLUG_SETTLE_TIME = 250; // Inhibit races that occur with the asynchronous teardown of ScpVBus -> X360 driver instance.
        //Called when DS4 is disconnected or timed out
        protected virtual void On_DS4Removal(object sender, EventArgs e)
        {
            var device = (Device)sender;

            var ind = -1;
            for (var i = 0; i < Controllers.Length; i++)
                if (Controllers[i] != null && device.MacAddress == Controllers[i].MacAddress)
                    ind = i;

            if (ind != -1)
            {
                CurrentState[ind].Battery = PreviousState[ind].Battery = 0; // Reset for the next connection's initial status change.

                X360Bus.Unplug(ind);

                var removed = $"Controller {ind + 1} was removed or lost connection";
                if (Controllers[ind].Battery <= 20 && Controllers[ind].ConnectionType == ConnectionType.BT && !Controllers[ind].Charging)
                    removed += ". Charge it up";

                LogDebug(removed);
                Log.LogToTray(removed);

                System.Threading.Thread.Sleep(XINPUT_UNPLUG_SETTLE_TIME);

                Controllers[ind] = null;
                touchPad[ind] = null;
                ControllerStatusChanged(this);
            }
        }

        public bool[] lag = {false, false, false, false};
        //Called every time the new input report has arrived
        protected virtual void On_Report(object sender, EventArgs e)
        {
            var device = (Device)sender;

            var ind = -1;
            for (var i = 0; i < Controllers.Length; i++)
                if (device == Controllers[i])
                    ind = i;

            if (ind != -1)
            {
                if (FlushHIDQueue[ind])
                    device.FlushHid();
                if (!string.IsNullOrEmpty(device.error))
                {
                    LogDebug(device.error);
                }
                if (DateTime.UtcNow - device.FirstActive > TimeSpan.FromSeconds(5))
                {
                    if (device.Latency >= FlashWhenLateAt && !lag[ind])
                        LagFlashWarning(ind, true);
                    else if (device.Latency < FlashWhenLateAt && lag[ind])
                        LagFlashWarning(ind, false);
                }
                device.GetExposedState(ExposedState[ind], CurrentState[ind]);
                var cState = CurrentState[ind];
                device.GetPreviousState(PreviousState[ind]);
                var pState = PreviousState[ind];
                if (pState.Battery != cState.Battery)
                    ControllerStatusChanged(this);
                CheckForHotkeys(ind, cState, pState);
                if (eastertime)
                    EasterTime(ind);
                GetInputkeys(ind);
                if (LSCurve[ind] != 0 || RSCurve[ind] != 0 || LSDeadzone[ind] != 0 || RSDeadzone[ind] != 0 ||
                    L2Deadzone[ind] != 0 || R2Deadzone[ind] != 0 || LSSens[ind] != 0 || RSSens[ind] != 0 ||
                    L2Sens[ind] != 0 || R2Sens[ind] != 0) //if a curve or deadzone is in place
                    cState = Mapping.SetCurveAndDeadzone(ind, cState);
                if (!recordingMacro && (!string.IsNullOrEmpty(tempprofilename[ind]) ||
                                        HasCustomAction(ind) || HasCustomExtras(ind) || ProfileActions[ind].Count > 0))
                {
                    Mapping.MapCustom(ind, cState, MappedState[ind], ExposedState[ind], touchPad[ind], this);
                    cState = MappedState[ind];
                }
                //if (HasCustomExtras(ind))
                //  DoExtras(ind);

                // Update the GUI/whatever.
                DS4LightBar.updateLightBar(device, ind, cState, ExposedState[ind], touchPad[ind]);

                X360Bus.Parse(cState, _processingData[ind].Report, ind);
                // We push the translated Xinput state, and simultaneously we
                // pull back any possible rumble data coming from Xinput consumers.
                if (X360Bus.Report(_processingData[ind].Report, _processingData[ind].Rumble))
                {
                    var Big = (byte)_processingData[ind].Rumble[3];
                    var Small = (byte)_processingData[ind].Rumble[4];

                    if (_processingData[ind].Rumble[1] == 0x08)
                    {
                        setRumble(Big, Small, ind);
                    }
                }

                // Output any synthetic events.
                Mapping.Commit(ind);
                // Pull settings updates.
                device.IdleTimeout = IdleDisconnectTimeout[ind];
            }
        }

        public void LagFlashWarning(int ind, bool on)
        {
            if (on)
            {
                lag[ind] = true;
                LogDebug($"Controller {ind + 1} is super latent", true);
                if (FlashWhenLate)
                {
                    var color = new LightBarColour {Red = 50, Green = 0, Blue = 0};
                    DS4LightBar.forcedColor[ind] = color;
                    DS4LightBar.forcedFlash[ind] = 2;
                    DS4LightBar.forcelight[ind] = true;
                }
            }
            else
            {
                lag[ind] = false;
                LogDebug($"Controller {ind + 1}");
                DS4LightBar.forcelight[ind] = false;
                DS4LightBar.forcedFlash[ind] = 0;
            }
        }

        /* private void DoExtras(int ind)
         {
             DS4State cState = CurrentState[ind];
             DS4StateExposed eState = ExposedState[ind];
             Mouse tp = touchPad[ind];
             DS4Controls helddown = DS4Controls.None;
             foreach (KeyValuePair<DS4Controls, string> p in getCustomExtras(ind))
             {
                 if (Mapping.getBoolMapping(ind, p.Key, cState, eState, tp))
                 {
                     helddown = p.Key;
                     break;
                 }
             }
             if (helddown != DS4Controls.None)
             {
                 string p = getCustomExtras(ind)[helddown];
                 string[] extraS = p.Split(',');
                 int[] extras = new int[extraS.Length];
                 for (int i = 0; i < extraS.Length; i++)
                 {
                     int b;
                     if (int.TryParse(extraS[i], out b))
                         extras[i] = b;
                 }
                 held[ind] = true;
                 try
                 {
                     if (!(extras[0] == extras[1] && extras[1] == 0))
                         setRumble((byte)extras[0], (byte)extras[1], ind);
                     if (extras[2] == 1)
                     {
                         DS4Color color = new DS4Color { red = (byte)extras[3], green = (byte)extras[4], blue = (byte)extras[5] };
                         DS4LightBar.forcedColor[ind] = color;
                         DS4LightBar.forcedFlash[ind] = (byte)extras[6];
                         DS4LightBar.forcelight[ind] = true;
                     }
                     if (extras[7] == 1)
                     {
                         if (oldmouse[ind] == -1)
                             oldmouse[ind] = ButtonMouseSensitivity[ind];
                         ButtonMouseSensitivity[ind] = extras[8];
                     }
                 }
                 catch { }
             }
             else if (held[ind])
             {
                 DS4LightBar.forcelight[ind] = false;
                 DS4LightBar.forcedFlash[ind] = 0;                
                 ButtonMouseSensitivity[ind] = oldmouse[ind];
                 oldmouse[ind] = -1;
                 setRumble(0, 0, ind);
                 held[ind] = false;
             }
         }*/


        public void EasterTime(int ind)
        {
            var cState = CurrentState[ind];
            var eState = ExposedState[ind];
            var tp = touchPad[ind];

            var pb = false;
            foreach (var dc in dcs)
            {
                if (Mapping.getBoolMapping(ind, dc, cState, eState, tp))
                {
                    pb = true;
                    break;
                }
            }
            var temp = eCode;
            //Looks like you found the easter egg code, since you're already cheating,
            //I scrambled the code for you :)
            if (pb && !buttonsdown[ind])
            {
                if (cState.Cross && eCode == 9)
                    eCode++;
                else if (!cState.Cross && eCode == 9)
                    eCode = 0;
                else if (cState.DpadLeft && eCode == 6)
                    eCode++;
                else if (!cState.DpadLeft && eCode == 6)
                    eCode = 0;
                else if (cState.DpadRight && eCode == 7)
                    eCode++;
                else if (!cState.DpadRight && eCode == 7)
                    eCode = 0;
                else if (cState.DpadLeft && eCode == 4)
                    eCode++;
                else if (!cState.DpadLeft && eCode == 4)
                    eCode = 0;
                else if (cState.DpadDown && eCode == 2)
                    eCode++;
                else if (!cState.DpadDown && eCode == 2)
                    eCode = 0;
                else if (cState.DpadRight && eCode == 5)
                    eCode++;
                else if (!cState.DpadRight && eCode == 5)
                    eCode = 0;
                else if (cState.DpadUp && eCode == 1)
                    eCode++;
                else if (!cState.DpadUp && eCode == 1)
                    eCode = 0;
                else if (cState.DpadDown && eCode == 3)
                    eCode++;
                else if (!cState.DpadDown && eCode == 3)
                    eCode = 0;
                else if (cState.Circle && eCode == 8)
                    eCode++;
                else if (!cState.Circle && eCode == 8)
                    eCode = 0;

                if (cState.DpadUp && eCode == 0)
                    eCode++;

                if (eCode == 10)
                {
                    var message = "(!)";
                    LogDebug(message, true);
                    eCode = 0;
                }

                if (temp != eCode)
                    Console.WriteLine(eCode);
                buttonsdown[ind] = true;
            }
            else if (!pb)
                buttonsdown[ind] = false;
        }

        public string GetInputkeys(int ind)
        {
            var cState = CurrentState[ind];
            var eState = ExposedState[ind];
            var tp = touchPad[ind];
            if (Controllers[ind] != null)
                if (Mapping.getBoolMapping(ind, DS4Controls.Cross, cState, eState, tp)) return "Cross";
                else if (Mapping.getBoolMapping(ind, DS4Controls.Circle, cState, eState, tp)) return "Circle";
                else if (Mapping.getBoolMapping(ind, DS4Controls.Triangle, cState, eState, tp)) return "Triangle";
                else if (Mapping.getBoolMapping(ind, DS4Controls.Square, cState, eState, tp)) return "Square";
                else if (Mapping.getBoolMapping(ind, DS4Controls.L1, cState, eState, tp)) return "L1";
                else if (Mapping.getBoolMapping(ind, DS4Controls.R1, cState, eState, tp)) return "R1";
                else if (Mapping.getBoolMapping(ind, DS4Controls.L2, cState, eState, tp)) return "L2";
                else if (Mapping.getBoolMapping(ind, DS4Controls.R2, cState, eState, tp)) return "R2";
                else if (Mapping.getBoolMapping(ind, DS4Controls.L3, cState, eState, tp)) return "L3";
                else if (Mapping.getBoolMapping(ind, DS4Controls.R3, cState, eState, tp)) return "R3";
                else if (Mapping.getBoolMapping(ind, DS4Controls.DpadUp, cState, eState, tp)) return "Up";
                else if (Mapping.getBoolMapping(ind, DS4Controls.DpadDown, cState, eState, tp)) return "Down";
                else if (Mapping.getBoolMapping(ind, DS4Controls.DpadLeft, cState, eState, tp)) return "Left";
                else if (Mapping.getBoolMapping(ind, DS4Controls.DpadRight, cState, eState, tp)) return "Right";
                else if (Mapping.getBoolMapping(ind, DS4Controls.Share, cState, eState, tp)) return "Share";
                else if (Mapping.getBoolMapping(ind, DS4Controls.Options, cState, eState, tp)) return "Options";
                else if (Mapping.getBoolMapping(ind, DS4Controls.PS, cState, eState, tp)) return "PS";
                else if (Mapping.getBoolMapping(ind, DS4Controls.LXPos, cState, eState, tp)) return "LS Right";
                else if (Mapping.getBoolMapping(ind, DS4Controls.LXNeg, cState, eState, tp)) return "LS Left";
                else if (Mapping.getBoolMapping(ind, DS4Controls.LYPos, cState, eState, tp)) return "LS Down";
                else if (Mapping.getBoolMapping(ind, DS4Controls.LYNeg, cState, eState, tp)) return "LS Up";
                else if (Mapping.getBoolMapping(ind, DS4Controls.RXPos, cState, eState, tp)) return "RS Right";
                else if (Mapping.getBoolMapping(ind, DS4Controls.RXNeg, cState, eState, tp)) return "RS Left";
                else if (Mapping.getBoolMapping(ind, DS4Controls.RYPos, cState, eState, tp)) return "RS Down";
                else if (Mapping.getBoolMapping(ind, DS4Controls.RYNeg, cState, eState, tp)) return "RS Up";
                else if (Mapping.getBoolMapping(ind, DS4Controls.TouchLeft, cState, eState, tp)) return "Touch Left";
                else if (Mapping.getBoolMapping(ind, DS4Controls.TouchRight, cState, eState, tp)) return "Touch Right";
                else if (Mapping.getBoolMapping(ind, DS4Controls.TouchMulti, cState, eState, tp)) return "Touch Multi";
                else if (Mapping.getBoolMapping(ind, DS4Controls.TouchUpper, cState, eState, tp)) return "Touch Upper";
            return "nothing";
        }

        public DS4Controls GetInputkeysDS4(int ind)
        {
            var cState = CurrentState[ind];
            var eState = ExposedState[ind];
            var tp = touchPad[ind];
            if (Controllers[ind] != null)
                if (Mapping.getBoolMapping(ind, DS4Controls.Cross, cState, eState, tp)) return DS4Controls.Cross;
                else if (Mapping.getBoolMapping(ind, DS4Controls.Circle, cState, eState, tp)) return DS4Controls.Circle;
                else if (Mapping.getBoolMapping(ind, DS4Controls.Triangle, cState, eState, tp)) return DS4Controls.Triangle;
                else if (Mapping.getBoolMapping(ind, DS4Controls.Square, cState, eState, tp)) return DS4Controls.Square;
                else if (Mapping.getBoolMapping(ind, DS4Controls.L1, cState, eState, tp)) return DS4Controls.L1;
                else if (Mapping.getBoolMapping(ind, DS4Controls.R1, cState, eState, tp)) return DS4Controls.R1;
                else if (Mapping.getBoolMapping(ind, DS4Controls.L2, cState, eState, tp)) return DS4Controls.L2;
                else if (Mapping.getBoolMapping(ind, DS4Controls.R2, cState, eState, tp)) return DS4Controls.R2;
                else if (Mapping.getBoolMapping(ind, DS4Controls.L3, cState, eState, tp)) return DS4Controls.L3;
                else if (Mapping.getBoolMapping(ind, DS4Controls.R3, cState, eState, tp)) return DS4Controls.R3;
                else if (Mapping.getBoolMapping(ind, DS4Controls.DpadUp, cState, eState, tp)) return DS4Controls.DpadUp;
                else if (Mapping.getBoolMapping(ind, DS4Controls.DpadDown, cState, eState, tp)) return DS4Controls.DpadDown;
                else if (Mapping.getBoolMapping(ind, DS4Controls.DpadLeft, cState, eState, tp)) return DS4Controls.DpadLeft;
                else if (Mapping.getBoolMapping(ind, DS4Controls.DpadRight, cState, eState, tp)) return DS4Controls.DpadRight;
                else if (Mapping.getBoolMapping(ind, DS4Controls.Share, cState, eState, tp)) return DS4Controls.Share;
                else if (Mapping.getBoolMapping(ind, DS4Controls.Options, cState, eState, tp)) return DS4Controls.Options;
                else if (Mapping.getBoolMapping(ind, DS4Controls.PS, cState, eState, tp)) return DS4Controls.PS;
                else if (Mapping.getBoolMapping(ind, DS4Controls.LXPos, cState, eState, tp)) return DS4Controls.LXPos;
                else if (Mapping.getBoolMapping(ind, DS4Controls.LXNeg, cState, eState, tp)) return DS4Controls.LXNeg;
                else if (Mapping.getBoolMapping(ind, DS4Controls.LYPos, cState, eState, tp)) return DS4Controls.LYPos;
                else if (Mapping.getBoolMapping(ind, DS4Controls.LYNeg, cState, eState, tp)) return DS4Controls.LYNeg;
                else if (Mapping.getBoolMapping(ind, DS4Controls.RXPos, cState, eState, tp)) return DS4Controls.RXPos;
                else if (Mapping.getBoolMapping(ind, DS4Controls.RXNeg, cState, eState, tp)) return DS4Controls.RXNeg;
                else if (Mapping.getBoolMapping(ind, DS4Controls.RYPos, cState, eState, tp)) return DS4Controls.RYPos;
                else if (Mapping.getBoolMapping(ind, DS4Controls.RYNeg, cState, eState, tp)) return DS4Controls.RYNeg;
                else if (Mapping.getBoolMapping(ind, DS4Controls.TouchLeft, cState, eState, tp)) return DS4Controls.TouchLeft;
                else if (Mapping.getBoolMapping(ind, DS4Controls.TouchRight, cState, eState, tp)) return DS4Controls.TouchRight;
                else if (Mapping.getBoolMapping(ind, DS4Controls.TouchMulti, cState, eState, tp)) return DS4Controls.TouchMulti;
                else if (Mapping.getBoolMapping(ind, DS4Controls.TouchUpper, cState, eState, tp)) return DS4Controls.TouchUpper;
            return DS4Controls.None;
        }

        public bool[] touchreleased = {true, true, true, true}, touchslid = {false, false, false, false};
        public byte[] oldtouchvalue = {0, 0, 0, 0};
        public int[] oldscrollvalue = {0, 0, 0, 0};

        protected virtual void CheckForHotkeys(int deviceID, State cState, State pState)
        {
            if (!UseTPforControls[deviceID] && cState.Touch1 && pState.PS)
            {
                if (TouchSensitivity[deviceID] > 0 && touchreleased[deviceID])
                {
                    oldtouchvalue[deviceID] = TouchSensitivity[deviceID];
                    oldscrollvalue[deviceID] = ScrollSensitivity[deviceID];
                    TouchSensitivity[deviceID] = 0;
                    ScrollSensitivity[deviceID] = 0;
                    LogDebug(TouchSensitivity[deviceID] > 0 ? "Touchpad movement on" : "Touchpad movement off");
                    Log.LogToTray(TouchSensitivity[deviceID] > 0 ? "Touchpad movement on" : "Touchpad movement off");
                    touchreleased[deviceID] = false;
                }
                else if (touchreleased[deviceID])
                {
                    TouchSensitivity[deviceID] = oldtouchvalue[deviceID];
                    ScrollSensitivity[deviceID] = oldscrollvalue[deviceID];
                    LogDebug(TouchSensitivity[deviceID] > 0 ? "Touchpad movement on" : "Touchpad movement off");
                    Log.LogToTray(TouchSensitivity[deviceID] > 0 ? "Touchpad movement on" : "Touchpad movement off");
                    touchreleased[deviceID] = false;
                }
            }
            else
                touchreleased[deviceID] = true;
        }

        public virtual void StartTPOff(int deviceID)
        {
            if (deviceID < 4)
            {
                oldtouchvalue[deviceID] = TouchSensitivity[deviceID];
                oldscrollvalue[deviceID] = ScrollSensitivity[deviceID];
                TouchSensitivity[deviceID] = 0;
                ScrollSensitivity[deviceID] = 0;
            }
        }

        public virtual string TouchpadSlide(int ind)
        {
            var cState = CurrentState[ind];
            var slidedir = "none";
            if (Controllers[ind] != null && cState.Touch2 && !(touchPad[ind].dragging || touchPad[ind].dragging2))
                if (touchPad[ind].slideright && !touchslid[ind])
                {
                    slidedir = "right";
                    touchslid[ind] = true;
                }
                else if (touchPad[ind].slideleft && !touchslid[ind])
                {
                    slidedir = "left";
                    touchslid[ind] = true;
                }
                else if (!touchPad[ind].slideleft && !touchPad[ind].slideright)
                {
                    slidedir = "";
                    touchslid[ind] = false;
                }
            return slidedir;
        }

        public virtual void LogDebug(string Data, bool warning = false)
        {
            Console.WriteLine(DateTime.Now.ToString("G") + "> " + Data);
            if (Debug != null)
            {
                var args = new DebugEventArgs(Data, warning);
                OnDebug(this, args);
            }
        }

        public virtual void OnDebug(object sender, DebugEventArgs args)
        {
            Debug?.Invoke(this, args);
        }

        //sets the rumble adjusted with rumble boost
        public virtual void setRumble(byte heavyMotor, byte lightMotor, int deviceNum)
        {
            var boost = RumbleBoost[deviceNum];
            var lightBoosted = (uint)lightMotor * (uint)boost / 100;
            if (lightBoosted > 255)
                lightBoosted = 255;
            var heavyBoosted = (uint)heavyMotor * (uint)boost / 100;
            if (heavyBoosted > 255)
                heavyBoosted = 255;
            if (deviceNum < 4)
                if (Controllers[deviceNum] != null)
                    Controllers[deviceNum].SetRumble((byte)lightBoosted, (byte)heavyBoosted);
        }

        public State getDS4State(int ind)
        {
            return CurrentState[ind];
        }

        public State getDS4StateMapped(int ind)
        {
            return MappedState[ind];
        }
    }
}