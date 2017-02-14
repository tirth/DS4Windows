using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using DS4Lib.DS4;

namespace DS4Lib.Control
{
    public static class Global
    {
        private static readonly BackingStore Config = new BackingStore();

        static int m_IdleTimeout = 600000;
        static string exepath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
        public static string appdatapath;
        public static string[] tempprofilename = new string[] {string.Empty, string.Empty, string.Empty, string.Empty, string.Empty};

        public static void SaveWhere(string path)
        {
            appdatapath = path;
            Config.m_Profile = appdatapath + "\\Profiles.xml";
            Config.m_Actions = appdatapath + "\\Actions.xml";
        }

        /// <summary>
        /// Check if Admin Rights are needed to write in Appliplation Directory
        /// </summary>
        /// <returns></returns>
        public static bool AdminNeeded()
        {
            try
            {
                File.WriteAllText(exepath + "\\test.txt", "test");
                File.Delete(exepath + "\\test.txt");
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
        }

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static event EventHandler<EventArgs> ControllerStatusChange;
        // called when a controller is added/removed/battery or touchpad mode changes/etc.

        public static void ControllerStatusChanged(object sender)
        {
            if (ControllerStatusChange != null)
                ControllerStatusChange(sender, EventArgs.Empty);
        }

        //general values
        public static bool UseExclusiveMode
        {
            set { Config.useExclusiveMode = value; }
            get { return Config.useExclusiveMode; }
        }

        public static DateTime LastChecked
        {
            set { Config.lastChecked = value; }
            get { return Config.lastChecked; }
        }

        public static int CheckWhen
        {
            set { Config.CheckWhen = value; }
            get { return Config.CheckWhen; }
        }

        public static int Notifications
        {
            set { Config.notifications = value; }
            get { return Config.notifications; }
        }

        public static bool DCBTatStop
        {
            set { Config.disconnectBTAtStop = value; }
            get { return Config.disconnectBTAtStop; }
        }

        public static bool SwipeProfiles
        {
            set { Config.swipeProfiles = value; }
            get { return Config.swipeProfiles; }
        }

        public static bool DS4Mapping
        {
            set { Config.ds4Mapping = value; }
            get { return Config.ds4Mapping; }
        }

        public static bool QuickCharge
        {
            set { Config.quickCharge = value; }
            get { return Config.quickCharge; }
        }

        public static int FirstXinputPort
        {
            set { Config.firstXinputPort = value; }
            get { return Config.firstXinputPort; }
        }

        public static bool CloseMini
        {
            set { Config.closeMini = value; }
            get { return Config.closeMini; }
        }

        public static bool StartMinimized
        {
            set { Config.startMinimized = value; }
            get { return Config.startMinimized; }
        }

        public static int FormWidth
        {
            set { Config.formWidth = value; }
            get { return Config.formWidth; }
        }

        public static int FormHeight
        {
            set { Config.formHeight = value; }
            get { return Config.formHeight; }
        }

        public static bool DownloadLang
        {
            set { Config.downloadLang = value; }
            get { return Config.downloadLang; }
        }

        public static bool FlashWhenLate
        {
            set { Config.flashWhenLate = value; }
            get { return Config.flashWhenLate; }
        }

        public static int FlashWhenLateAt
        {
            set { Config.flashWhenLateAt = value; }
            get { return Config.flashWhenLateAt; }
        }

        public static bool UseWhiteIcon
        {
            set { Config.useWhiteIcon = value; }
            get { return Config.useWhiteIcon; }
        }

        //controller/profile specfic values
        public static int[] ButtonMouseSensitivity => Config.buttonMouseSensitivity;
        public static byte[] RumbleBoost => Config.rumble;
        public static double[] Rainbow => Config.rainbow;
        public static bool[] FlushHIDQueue => Config.flushHIDQueue;
        public static int[] IdleDisconnectTimeout => Config.idleDisconnectTimeout;
        public static byte[] TouchSensitivity => Config.touchSensitivity;
        public static byte[] FlashType => Config.flashType;
        public static int[] FlashAt => Config.flashAt;
        public static bool[] LedAsBatteryIndicator => Config.ledAsBattery;
        public static int[] ChargingType => Config.chargingType;
        public static bool[] DinputOnly => Config.dinputOnly;
        public static bool[] StartTouchpadOff => Config.startTouchpadOff;
        public static bool[] UseTPforControls => Config.useTPforControls;
        public static bool[] UseSAforMouse => Config.useSAforMouse;
        public static string[] SATriggers => Config.sATriggers;
        public static int[] GyroSensitivity => Config.gyroSensitivity;
        public static int[] GyroInvert => Config.gyroInvert;
        public static LightBarColour[] MainColour => Config.m_Leds;
        public static LightBarColour[] LowColour => Config.m_LowLeds;
        public static LightBarColour[] ChargingColour => Config.m_ChargingLeds;
        public static LightBarColour[] CustomColour => Config.m_CustomLeds;
        public static bool[] UseCustomLed => Config.useCustomLeds;

        public static LightBarColour[] FlashColour => Config.m_FlashLeds;
        public static byte[] TapSensitivity => Config.tapSensitivity;
        public static bool[] DoubleTap => Config.doubleTap;
        public static int[] ScrollSensitivity => Config.scrollSensitivity;
        public static bool[] LowerRCOn => Config.lowerRCOn;
        public static bool[] TouchpadJitterCompensation => Config.touchpadJitterCompensation;

        public static byte[] L2Deadzone => Config.l2Deadzone;
        public static byte[] R2Deadzone => Config.r2Deadzone;
        public static double[] SXDeadzone => Config.SXDeadzone;
        public static double[] SZDeadzone => Config.SZDeadzone;
        public static int[] LSDeadzone => Config.LSDeadzone;
        public static int[] RSDeadzone => Config.RSDeadzone;
        public static int[] LSCurve => Config.lsCurve;
        public static int[] RSCurve => Config.rsCurve;
        public static double[] L2Sens => Config.l2Sens;
        public static double[] R2Sens => Config.r2Sens;
        public static double[] SXSens => Config.SXSens;
        public static double[] SZSens => Config.SZSens;
        public static double[] LSSens => Config.LSSens;
        public static double[] RSSens => Config.RSSens;
        public static bool[] MouseAccel => Config.mouseAccel;
        public static string[] LaunchProgram => Config.launchProgram;
        public static string[] ProfilePath => Config.profilePath;
        public static List<string>[] ProfileActions => Config.profileActions;

        public static void UpdateDS4CSetting(int deviceNum, string buttonName, bool shift, object action, string exts, DS4KeyType kt, int trigger = 0)
        {
            Config.UpdateDS4CSetting(deviceNum, buttonName, shift, action, exts, kt, trigger);
        }

        public static void UpdateDS4Extra(int deviceNum, string buttonName, bool shift, string exts)
        {
            Config.UpdateDS4CExtra(deviceNum, buttonName, shift, exts);
        }

        public static object GetDS4Action(int deviceNum, string buttonName, bool shift) => Config.GetDS4Action(deviceNum, buttonName, shift);
        public static DS4KeyType GetDS4KeyType(int deviceNum, string buttonName, bool shift) => Config.GetDS4KeyType(deviceNum, buttonName, shift);
        public static string GetDS4Extra(int deviceNum, string buttonName, bool shift) => Config.GetDS4Extra(deviceNum, buttonName, shift);
        public static int GetDS4STrigger(int deviceNum, string buttonName) => Config.GetDS4STrigger(deviceNum, buttonName);
        public static List<ControlSettings> getDS4CSettings(int device) => Config.Ds4Settings[device];
        public static ControlSettings getDS4CSetting(int deviceNum, string control) => Config.getDS4CSetting(deviceNum, control);
        public static bool HasCustomAction(int deviceNum) => Config.HasCustomActions(deviceNum);
        public static bool HasCustomExtras(int deviceNum) => Config.HasCustomExtras(deviceNum);

        public static void SaveAction(string name, string controls, int mode, string details, bool edit, string extras = "")
        {
            Config.SaveAction(name, controls, mode, details, edit, extras);
            Mapping.actionDone.Add(new Mapping.ActionState());
        }

        public static void RemoveAction(string name)
        {
            Config.RemoveAction(name);
        }

        public static bool LoadActions() => Config.LoadActions();

        public static List<SpecialAction> GetActions() => Config.actions;

        public static int GetActionIndexOf(string name)
        {
            for (var i = 0; i < Config.actions.Count; i++)
                if (Config.actions[i].name == name)
                    return i;
            return -1;
        }

        public static SpecialAction GetAction(string name)
        {
            foreach (var sA in Config.actions)
                if (sA.name == name)
                    return sA;
            return new SpecialAction("null", "null", "null", "null");
        }


        /*public static X360Controls getCustomButton(int device, DS4Controls controlName) => m_Config.GetCustomButton(device, controlName);
        
        public static ushort getCustomKey(int device, DS4Controls controlName) => m_Config.GetCustomKey(device, controlName);
        
        public static string getCustomMacro(int device, DS4Controls controlName) => m_Config.GetCustomMacro(device, controlName);
        
        public static string getCustomExtras(int device, DS4Controls controlName) => m_Config.GetCustomExtras(device, controlName);
        
        public static DS4KeyType getCustomKeyType(int device, DS4Controls controlName) => m_Config.GetCustomKeyType(device, controlName);
        
        public static bool getHasCustomKeysorButtons(int device) => m_Config.customMapButtons[device].Count > 0
                || m_Config.customMapKeys[device].Count > 0;
        
        public static bool getHasCustomExtras(int device) => m_Config.customMapExtras[device].Count > 0;        
        public static Dictionary<DS4Controls, X360Controls> getCustomButtons(int device) => m_Config.customMapButtons[device];        
        public static Dictionary<DS4Controls, ushort> getCustomKeys(int device) => m_Config.customMapKeys[device];        
        public static Dictionary<DS4Controls, string> getCustomMacros(int device) => m_Config.customMapMacros[device];        
        public static Dictionary<DS4Controls, string> getCustomExtras(int device) => m_Config.customMapExtras[device];
        public static Dictionary<DS4Controls, DS4KeyType> getCustomKeyTypes(int device) => m_Config.customMapKeyTypes[device];        

        public static X360Controls getShiftCustomButton(int device, DS4Controls controlName) => m_Config.GetShiftCustomButton(device, controlName);        
        public static ushort getShiftCustomKey(int device, DS4Controls controlName) => m_Config.GetShiftCustomKey(device, controlName);        
        public static string getShiftCustomMacro(int device, DS4Controls controlName) => m_Config.GetShiftCustomMacro(device, controlName);        
        public static string getShiftCustomExtras(int device, DS4Controls controlName) => m_Config.GetShiftCustomExtras(device, controlName);        
        public static DS4KeyType getShiftCustomKeyType(int device, DS4Controls controlName) => m_Config.GetShiftCustomKeyType(device, controlName);        
        public static bool getHasShiftCustomKeysorButtons(int device) => m_Config.shiftCustomMapButtons[device].Count > 0
                || m_Config.shiftCustomMapKeys[device].Count > 0;        
        public static bool getHasShiftCustomExtras(int device) => m_Config.shiftCustomMapExtras[device].Count > 0;        
        public static Dictionary<DS4Controls, X360Controls> getShiftCustomButtons(int device) => m_Config.shiftCustomMapButtons[device];        
        public static Dictionary<DS4Controls, ushort> getShiftCustomKeys(int device) => m_Config.shiftCustomMapKeys[device];        
        public static Dictionary<DS4Controls, string> getShiftCustomMacros(int device) => m_Config.shiftCustomMapMacros[device];        
        public static Dictionary<DS4Controls, string> getShiftCustomExtras(int device) => m_Config.shiftCustomMapExtras[device];        
        public static Dictionary<DS4Controls, DS4KeyType> getShiftCustomKeyTypes(int device) => m_Config.shiftCustomMapKeyTypes[device]; */
        public static bool Load() => Config.Load();

        public static void LoadProfile(int device, bool launchprogram, ControlService control)
        {
            Config.LoadProfile(device, launchprogram, control);
            tempprofilename[device] = string.Empty;
        }

        public static void LoadTempProfile(int device, string name, bool launchprogram, ControlService control)
        {
            Config.LoadProfile(device, launchprogram, control, appdatapath + @"\Profiles\" + name + ".xml");
            tempprofilename[device] = name;
        }

        public static bool Save()
        {
            return Config.Save();
        }

        public static void SaveProfile(int device, string propath)
        {
            Config.SaveProfile(device, propath);
        }

        private static byte ApplyRatio(byte b1, byte b2, double r)
        {
            if (r > 100)
                r = 100;
            else if (r < 0)
                r = 0;
            r /= 100f;
            return (byte)Math.Round(b1 * (1 - r) + b2 * r, 0);
        }

        public static LightBarColour getTransitionedColor(LightBarColour c1, LightBarColour c2, double ratio)
        {
//;
            //Color cs = Color.FromArgb(c1.red, c1.green, c1.blue);
            c1.Red = ApplyRatio(c1.Red, c2.Red, ratio);
            c1.Green = ApplyRatio(c1.Green, c2.Green, ratio);
            c1.Blue = ApplyRatio(c1.Blue, c2.Blue, ratio);
            return c1;
        }

        private static Color ApplyRatio(Color c1, Color c2, uint r)
        {
            var ratio = r / 100f;
            var hue1 = c1.GetHue();
            var hue2 = c2.GetHue();
            var bri1 = c1.GetBrightness();
            var bri2 = c2.GetBrightness();
            var sat1 = c1.GetSaturation();
            var sat2 = c2.GetSaturation();
            var hr = hue2 - hue1;
            var br = bri2 - bri1;
            var sr = sat2 - sat1;

            var csR = bri1 == 0
                ? HuetoRGB(hue2, sat2, bri2 - br * ratio)
                : HuetoRGB(hue2 - hr * ratio, sat2 - sr * ratio, bri2 - br * ratio);

            return csR;
        }

        public static Color HuetoRGB(float hue, float sat, float bri)
        {
            var C = (1 - Math.Abs(2 * bri) - 1) * sat;
            var X = C * (1 - Math.Abs(hue / 60 % 2 - 1));
            var m = bri - C / 2;
            float R, G, B;
            if (0 <= hue && hue < 60)
            {
                R = C;
                G = X;
                B = 0;
            }
            else if (60 <= hue && hue < 120)
            {
                R = X;
                G = C;
                B = 0;
            }
            else if (120 <= hue && hue < 180)
            {
                R = 0;
                G = C;
                B = X;
            }
            else if (180 <= hue && hue < 240)
            {
                R = 0;
                G = X;
                B = C;
            }
            else if (240 <= hue && hue < 300)
            {
                R = X;
                G = 0;
                B = C;
            }
            else if (300 <= hue && hue < 360)
            {
                R = C;
                G = 0;
                B = X;
            }
            else
            {
                R = 255;
                G = 0;
                B = 0;
            }
            R += m;
            G += m;
            B += m;
            R *= 255;
            G *= 255;
            B *= 255;
            return Color.FromArgb((int)R, (int)G, (int)B);
        }
    }
}