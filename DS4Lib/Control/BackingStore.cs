using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Xml;
using DS4Lib.DS4;

namespace DS4Lib.Control
{
    public class BackingStore
    {
        //public String m_Profile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DS4Tool" + "\\Profiles.xml";
        public string m_Profile = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + "\\Profiles.xml";
        public string m_Actions = Global.appdatapath + "\\Actions.xml";

        protected XmlDocument m_Xdoc = new XmlDocument();
        //fifth value used to for options, not fifth controller
        public int[] buttonMouseSensitivity = {25, 25, 25, 25, 25};

        public bool[] flushHIDQueue = {true, true, true, true, true};
        public int[] idleDisconnectTimeout = {0, 0, 0, 0, 0};
        public bool[] touchpadJitterCompensation = {true, true, true, true, true};
        public bool[] lowerRCOn = {false, false, false, false, false};
        public bool[] ledAsBattery = {false, false, false, false, false};
        public byte[] flashType = {0, 0, 0, 0, 0};
        public string[] profilePath = {string.Empty, string.Empty, string.Empty, string.Empty, string.Empty};
        public byte[] rumble = {100, 100, 100, 100, 100};
        public byte[] touchSensitivity = {100, 100, 100, 100, 100};
        public byte[] l2Deadzone = {0, 0, 0, 0, 0}, r2Deadzone = {0, 0, 0, 0, 0};
        public int[] LSDeadzone = {0, 0, 0, 0, 0}, RSDeadzone = {0, 0, 0, 0, 0};
        public double[] SXDeadzone = {0.25, 0.25, 0.25, 0.25, 0.25}, SZDeadzone = {0.25, 0.25, 0.25, 0.25, 0.25};
        public double[] l2Sens = {1, 1, 1, 1, 1}, r2Sens = {1, 1, 1, 1, 1};
        public double[] LSSens = {1, 1, 1, 1, 1}, RSSens = {1, 1, 1, 1, 1};
        public double[] SXSens = {1, 1, 1, 1, 1}, SZSens = {1, 1, 1, 1, 1};
        public byte[] tapSensitivity = {0, 0, 0, 0, 0};
        public bool[] doubleTap = {false, false, false, false, false};
        public int[] scrollSensitivity = {0, 0, 0, 0, 0};
        public double[] rainbow = {0, 0, 0, 0, 0};
        public int[] flashAt = {0, 0, 0, 0, 0};
        public bool[] mouseAccel = {true, true, true, true, true};

        public LightBarColour[] m_LowLeds = new LightBarColour[]
        {
            new LightBarColour(Color.Black),
            new LightBarColour(Color.Black),
            new LightBarColour(Color.Black),
            new LightBarColour(Color.Black),
            new LightBarColour(Color.Black)
        };

        public LightBarColour[] m_Leds = new LightBarColour[]
        {
            new LightBarColour(Color.Blue),
            new LightBarColour(Color.Red),
            new LightBarColour(Color.Green),
            new LightBarColour(Color.Pink),
            new LightBarColour(Color.White)
        };

        public LightBarColour[] m_ChargingLeds = new LightBarColour[]
        {
            new LightBarColour(Color.Black),
            new LightBarColour(Color.Black),
            new LightBarColour(Color.Black),
            new LightBarColour(Color.Black),
            new LightBarColour(Color.Black)
        };

        public LightBarColour[] m_FlashLeds = new LightBarColour[]
        {
            new LightBarColour(Color.Black),
            new LightBarColour(Color.Black),
            new LightBarColour(Color.Black),
            new LightBarColour(Color.Black),
            new LightBarColour(Color.Black)
        };

        public bool[] useCustomLeds = new bool[] {false, false, false, false};

        public LightBarColour[] m_CustomLeds = new LightBarColour[]
        {
            new LightBarColour(Color.Black),
            new LightBarColour(Color.Black),
            new LightBarColour(Color.Black),
            new LightBarColour(Color.Black)
        };

        public int[] chargingType = {0, 0, 0, 0, 0};
        public string[] launchProgram = {string.Empty, string.Empty, string.Empty, string.Empty, string.Empty};
        public bool[] dinputOnly = {false, false, false, false, false};
        public bool[] startTouchpadOff = {false, false, false, false, false};
        public bool[] useTPforControls = {false, false, false, false, false};
        public bool[] useSAforMouse = {false, false, false, false, false};
        public string[] sATriggers = {"", "", "", "", ""};
        public int[] lsCurve = {0, 0, 0, 0, 0};
        public int[] rsCurve = {0, 0, 0, 0, 0};
        public bool useExclusiveMode;
        public int formWidth = 782;
        public int formHeight = 550;
        public bool startMinimized;
        public DateTime lastChecked;
        public int CheckWhen = 1;
        public int notifications = 2;
        public bool disconnectBTAtStop;
        public bool swipeProfiles = true;
        public bool ds4Mapping = true;
        public bool quickCharge;
        public int firstXinputPort = 1;
        public bool closeMini;
        public List<SpecialAction> actions = new List<SpecialAction>();

        public readonly List<ControlSettings>[] Ds4Settings =
        {
            new List<ControlSettings>(), new List<ControlSettings>(),
            new List<ControlSettings>(), new List<ControlSettings>(), new List<ControlSettings>()
        };

        /*public Dictionary<DS4Controls, DS4KeyType>[] customMapKeyTypes = { null, null, null, null, null };
                public Dictionary<DS4Controls, UInt16>[] customMapKeys = { null, null, null, null, null };
                public Dictionary<DS4Controls, String>[] customMapMacros = { null, null, null, null, null };
                public Dictionary<DS4Controls, X360Controls>[] customMapButtons = { null, null, null, null, null };
                public Dictionary<DS4Controls, String>[] customMapExtras = { null, null, null, null, null };
        
                public Dictionary<DS4Controls, DS4KeyType>[] shiftCustomMapKeyTypes = { null, null, null, null, null };
                public Dictionary<DS4Controls, UInt16>[] shiftCustomMapKeys = { null, null, null, null, null };
                public Dictionary<DS4Controls, String>[] shiftCustomMapMacros = { null, null, null, null, null };
                public Dictionary<DS4Controls, X360Controls>[] shiftCustomMapButtons = { null, null, null, null, null };
                public Dictionary<DS4Controls, String>[] shiftCustomMapExtras = { null, null, null, null, null };*/
        public List<string>[] profileActions = {null, null, null, null, null};
        public bool downloadLang = true;
        public bool useWhiteIcon;
        public bool flashWhenLate = true;
        public int flashWhenLateAt = 10;
        public int[] gyroSensitivity = {100, 100, 100, 100, 100};
        public int[] gyroInvert = {0, 0, 0, 0, 0};

        public BackingStore()
        {
            for (var i = 0; i < 5; i++)
            {
                foreach (DS4Controls dc in Enum.GetValues(typeof(DS4Controls)))
                    if (dc != DS4Controls.None)
                        Ds4Settings[i].Add(new ControlSettings(dc));
                /*customMapKeyTypes[i] = new Dictionary<DS4Controls, DS4KeyType>();
                customMapKeys[i] = new Dictionary<DS4Controls, UInt16>();
                customMapMacros[i] = new Dictionary<DS4Controls, String>();
                customMapButtons[i] = new Dictionary<DS4Controls, X360Controls>();
                customMapExtras[i] = new Dictionary<DS4Controls, string>();

                shiftCustomMapKeyTypes[i] = new Dictionary<DS4Controls, DS4KeyType>();
                shiftCustomMapKeys[i] = new Dictionary<DS4Controls, UInt16>();
                shiftCustomMapMacros[i] = new Dictionary<DS4Controls, String>();
                shiftCustomMapButtons[i] = new Dictionary<DS4Controls, X360Controls>();
                shiftCustomMapExtras[i] = new Dictionary<DS4Controls, string>();*/
                profileActions[i] = new List<string>();
                profileActions[i].Add("Disconnect Controller");
            }
        }

        /*public X360Controls GetCustomButton(int device, DS4Controls controlName)
        {
            if (customMapButtons[device].ContainsKey(controlName))
                return customMapButtons[device][controlName];
            else return X360Controls.None;
        }
        public UInt16 GetCustomKey(int device, DS4Controls controlName)
        {
            if (customMapKeys[device].ContainsKey(controlName))
                return customMapKeys[device][controlName];
            else return 0;
        }
        public string GetCustomMacro(int device, DS4Controls controlName)
        {
            if (customMapMacros[device].ContainsKey(controlName))
                return customMapMacros[device][controlName];
            else return "0";
        }
        public string GetCustomExtras(int device, DS4Controls controlName)
        {
            if (customMapExtras[device].ContainsKey(controlName))
                return customMapExtras[device][controlName];
            else return "0";
        }
        public DS4KeyType GetCustomKeyType(int device, DS4Controls controlName)
        {
            try
            {
                if (customMapKeyTypes[device].ContainsKey(controlName))
                    return customMapKeyTypes[device][controlName];
                else return 0;
            }
            catch { return 0; }
        }

        public X360Controls GetShiftCustomButton(int device, DS4Controls controlName)
        {
            if (shiftCustomMapButtons[device].ContainsKey(controlName))
                return shiftCustomMapButtons[device][controlName];
            else return X360Controls.None;
        }
        public UInt16 GetShiftCustomKey(int device, DS4Controls controlName)
        {
            if (shiftCustomMapKeys[device].ContainsKey(controlName))
                return shiftCustomMapKeys[device][controlName];
            else return 0;
        }
        public string GetShiftCustomMacro(int device, DS4Controls controlName)
        {
            if (shiftCustomMapMacros[device].ContainsKey(controlName))
                return shiftCustomMapMacros[device][controlName];
            else return "0";
        }
        public string GetShiftCustomExtras(int device, DS4Controls controlName)
        {
            if (customMapExtras[device].ContainsKey(controlName))
                return customMapExtras[device][controlName];
            else return "0";
        }
        public DS4KeyType GetShiftCustomKeyType(int device, DS4Controls controlName)
        {
            try
            {
                if (shiftCustomMapKeyTypes[device].ContainsKey(controlName))
                    return shiftCustomMapKeyTypes[device][controlName];
                else return 0;
            }
            catch { return 0; }
        }*/

        public bool SaveProfile(int device, string propath)
        {
            var saved = true;
            var path = Global.appdatapath + @"\Profiles\" + Path.GetFileNameWithoutExtension(propath) + ".xml";
            try
            {
                var xmlControls = m_Xdoc.SelectSingleNode("/DS4Windows/Control");
                var xmlShiftControls = m_Xdoc.SelectSingleNode("/DS4Windows/ShiftControl");
                m_Xdoc.RemoveAll();

                XmlNode Node = m_Xdoc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateComment($" DS4Windows Configuration Data. {DateTime.Now} ");
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateWhitespace("\r\n");
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateNode(XmlNodeType.Element, "DS4Windows", null);

                var xmlFlushHIDQueue = m_Xdoc.CreateNode(XmlNodeType.Element, "flushHIDQueue", null);
                xmlFlushHIDQueue.InnerText = flushHIDQueue[device].ToString();
                Node.AppendChild(xmlFlushHIDQueue);

                var xmlIdleDisconnectTimeout = m_Xdoc.CreateNode(XmlNodeType.Element, "idleDisconnectTimeout", null);
                xmlIdleDisconnectTimeout.InnerText = idleDisconnectTimeout[device].ToString();
                Node.AppendChild(xmlIdleDisconnectTimeout);

                var xmlColor = m_Xdoc.CreateNode(XmlNodeType.Element, "Color", null);
                xmlColor.InnerText = m_Leds[device].Red + "," + m_Leds[device].Green + "," + m_Leds[device].Blue;
                Node.AppendChild(xmlColor);

                var xmlRumbleBoost = m_Xdoc.CreateNode(XmlNodeType.Element, "RumbleBoost", null);
                xmlRumbleBoost.InnerText = rumble[device].ToString();
                Node.AppendChild(xmlRumbleBoost);

                var xmlLedAsBatteryIndicator = m_Xdoc.CreateNode(XmlNodeType.Element, "ledAsBatteryIndicator", null);
                xmlLedAsBatteryIndicator.InnerText = ledAsBattery[device].ToString();
                Node.AppendChild(xmlLedAsBatteryIndicator);

                var xmlLowBatteryFlash = m_Xdoc.CreateNode(XmlNodeType.Element, "FlashType", null);
                xmlLowBatteryFlash.InnerText = flashType[device].ToString();
                Node.AppendChild(xmlLowBatteryFlash);

                var xmlFlashBatterAt = m_Xdoc.CreateNode(XmlNodeType.Element, "flashBatteryAt", null);
                xmlFlashBatterAt.InnerText = flashAt[device].ToString();
                Node.AppendChild(xmlFlashBatterAt);

                var xmlTouchSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "touchSensitivity", null);
                xmlTouchSensitivity.InnerText = touchSensitivity[device].ToString();
                Node.AppendChild(xmlTouchSensitivity);

                var xmlLowColor = m_Xdoc.CreateNode(XmlNodeType.Element, "LowColor", null);
                xmlLowColor.InnerText = m_LowLeds[device].Red + "," + m_LowLeds[device].Green + "," + m_LowLeds[device].Blue;
                Node.AppendChild(xmlLowColor);

                var xmlChargingColor = m_Xdoc.CreateNode(XmlNodeType.Element, "ChargingColor", null);
                xmlChargingColor.InnerText = m_ChargingLeds[device].Red + "," + m_ChargingLeds[device].Green + "," + m_ChargingLeds[device].Blue;
                Node.AppendChild(xmlChargingColor);

                var xmlFlashColor = m_Xdoc.CreateNode(XmlNodeType.Element, "FlashColor", null);
                xmlFlashColor.InnerText = m_FlashLeds[device].Red + "," + m_FlashLeds[device].Green + "," + m_FlashLeds[device].Blue;
                Node.AppendChild(xmlFlashColor);

                var xmlTouchpadJitterCompensation = m_Xdoc.CreateNode(XmlNodeType.Element, "touchpadJitterCompensation", null);
                xmlTouchpadJitterCompensation.InnerText = touchpadJitterCompensation[device].ToString();
                Node.AppendChild(xmlTouchpadJitterCompensation);

                var xmlLowerRCOn = m_Xdoc.CreateNode(XmlNodeType.Element, "lowerRCOn", null);
                xmlLowerRCOn.InnerText = lowerRCOn[device].ToString();
                Node.AppendChild(xmlLowerRCOn);

                var xmlTapSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "tapSensitivity", null);
                xmlTapSensitivity.InnerText = tapSensitivity[device].ToString();
                Node.AppendChild(xmlTapSensitivity);

                var xmlDouble = m_Xdoc.CreateNode(XmlNodeType.Element, "doubleTap", null);
                xmlDouble.InnerText = doubleTap[device].ToString();
                Node.AppendChild(xmlDouble);

                var xmlScrollSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "scrollSensitivity", null);
                xmlScrollSensitivity.InnerText = scrollSensitivity[device].ToString();
                Node.AppendChild(xmlScrollSensitivity);

                var xmlLeftTriggerMiddle = m_Xdoc.CreateNode(XmlNodeType.Element, "LeftTriggerMiddle", null);
                xmlLeftTriggerMiddle.InnerText = l2Deadzone[device].ToString();
                Node.AppendChild(xmlLeftTriggerMiddle);

                var xmlRightTriggerMiddle = m_Xdoc.CreateNode(XmlNodeType.Element, "RightTriggerMiddle", null);
                xmlRightTriggerMiddle.InnerText = r2Deadzone[device].ToString();
                Node.AppendChild(xmlRightTriggerMiddle);

                var xmlButtonMouseSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "ButtonMouseSensitivity", null);
                xmlButtonMouseSensitivity.InnerText = buttonMouseSensitivity[device].ToString();
                Node.AppendChild(xmlButtonMouseSensitivity);

                var xmlRainbow = m_Xdoc.CreateNode(XmlNodeType.Element, "Rainbow", null);
                xmlRainbow.InnerText = rainbow[device].ToString();
                Node.AppendChild(xmlRainbow);

                var xmlLSD = m_Xdoc.CreateNode(XmlNodeType.Element, "LSDeadZone", null);
                xmlLSD.InnerText = LSDeadzone[device].ToString();
                Node.AppendChild(xmlLSD);

                var xmlRSD = m_Xdoc.CreateNode(XmlNodeType.Element, "RSDeadZone", null);
                xmlRSD.InnerText = RSDeadzone[device].ToString();
                Node.AppendChild(xmlRSD);

                var xmlSXD = m_Xdoc.CreateNode(XmlNodeType.Element, "SXDeadZone", null);
                xmlSXD.InnerText = SXDeadzone[device].ToString();
                Node.AppendChild(xmlSXD);

                var xmlSZD = m_Xdoc.CreateNode(XmlNodeType.Element, "SZDeadZone", null);
                xmlSZD.InnerText = SZDeadzone[device].ToString();
                Node.AppendChild(xmlSZD);

                var xmlSens = m_Xdoc.CreateNode(XmlNodeType.Element, "Sensitivity", null);
                xmlSens.InnerText = $"{LSSens[device]}|{RSSens[device]}|{l2Sens[device]}|{r2Sens[device]}|{SXSens[device]}|{SZSens[device]}";
                Node.AppendChild(xmlSens);

                var xmlChargingType = m_Xdoc.CreateNode(XmlNodeType.Element, "ChargingType", null);
                xmlChargingType.InnerText = chargingType[device].ToString();
                Node.AppendChild(xmlChargingType);

                var xmlMouseAccel = m_Xdoc.CreateNode(XmlNodeType.Element, "MouseAcceleration", null);
                xmlMouseAccel.InnerText = mouseAccel[device].ToString();
                Node.AppendChild(xmlMouseAccel);

                //XmlNode xmlShiftMod = m_Xdoc.CreateNode(XmlNodeType.Element, "ShiftModifier", null); xmlShiftMod.InnerText = shiftModifier[device].ToString(); Node.AppendChild(xmlShiftMod);

                var xmlLaunchProgram = m_Xdoc.CreateNode(XmlNodeType.Element, "LaunchProgram", null);
                xmlLaunchProgram.InnerText = launchProgram[device];
                Node.AppendChild(xmlLaunchProgram);

                var xmlDinput = m_Xdoc.CreateNode(XmlNodeType.Element, "DinputOnly", null);
                xmlDinput.InnerText = dinputOnly[device].ToString();
                Node.AppendChild(xmlDinput);

                var xmlStartTouchpadOff = m_Xdoc.CreateNode(XmlNodeType.Element, "StartTouchpadOff", null);
                xmlStartTouchpadOff.InnerText = startTouchpadOff[device].ToString();
                Node.AppendChild(xmlStartTouchpadOff);

                var xmlUseTPforControls = m_Xdoc.CreateNode(XmlNodeType.Element, "UseTPforControls", null);
                xmlUseTPforControls.InnerText = useTPforControls[device].ToString();
                Node.AppendChild(xmlUseTPforControls);

                var xmlUseSAforMouse = m_Xdoc.CreateNode(XmlNodeType.Element, "UseSAforMouse", null);
                xmlUseSAforMouse.InnerText = useSAforMouse[device].ToString();
                Node.AppendChild(xmlUseSAforMouse);

                var xmlSATriggers = m_Xdoc.CreateNode(XmlNodeType.Element, "SATriggers", null);
                xmlSATriggers.InnerText = sATriggers[device];
                Node.AppendChild(xmlSATriggers);

                var xmlGyroSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroSensitivity", null);
                xmlGyroSensitivity.InnerText = gyroSensitivity[device].ToString();
                Node.AppendChild(xmlGyroSensitivity);

                var xmlGyroInvert = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroInvert", null);
                xmlGyroInvert.InnerText = gyroInvert[device].ToString();
                Node.AppendChild(xmlGyroInvert);

                var xmlLSC = m_Xdoc.CreateNode(XmlNodeType.Element, "LSCurve", null);
                xmlLSC.InnerText = lsCurve[device].ToString();
                Node.AppendChild(xmlLSC);

                var xmlRSC = m_Xdoc.CreateNode(XmlNodeType.Element, "RSCurve", null);
                xmlRSC.InnerText = rsCurve[device].ToString();
                Node.AppendChild(xmlRSC);

                var xmlProfileActions = m_Xdoc.CreateNode(XmlNodeType.Element, "ProfileActions", null);
                xmlProfileActions.InnerText = string.Join("/", profileActions[device]);
                Node.AppendChild(xmlProfileActions);

                var nodeControl = m_Xdoc.CreateNode(XmlNodeType.Element, "Control", null);
                var key = m_Xdoc.CreateNode(XmlNodeType.Element, "Key", null);
                var macro = m_Xdoc.CreateNode(XmlNodeType.Element, "Macro", null);
                var KeyType = m_Xdoc.CreateNode(XmlNodeType.Element, "KeyType", null);
                var button = m_Xdoc.CreateNode(XmlNodeType.Element, "Button", null);
                var Extras = m_Xdoc.CreateNode(XmlNodeType.Element, "Extras", null);

                var NodeShiftControl = m_Xdoc.CreateNode(XmlNodeType.Element, "ShiftControl", null);

                var ShiftKey = m_Xdoc.CreateNode(XmlNodeType.Element, "Key", null);
                var ShiftMacro = m_Xdoc.CreateNode(XmlNodeType.Element, "Macro", null);
                var ShiftKeyType = m_Xdoc.CreateNode(XmlNodeType.Element, "KeyType", null);
                var ShiftButton = m_Xdoc.CreateNode(XmlNodeType.Element, "Button", null);
                var ShiftExtras = m_Xdoc.CreateNode(XmlNodeType.Element, "Extras", null);

                foreach (var dcs in Ds4Settings[device])
                {
                    if (dcs.action != null)
                    {
                        XmlNode buttonNode;
                        var keyType = string.Empty;

                        if (dcs.action is string)
                            if (dcs.action.ToString() == "Unbound")
                                keyType += DS4KeyType.Unbound;
                        if (dcs.keyType.HasFlag(DS4KeyType.HoldMacro))
                            keyType += DS4KeyType.HoldMacro;
                        if (dcs.keyType.HasFlag(DS4KeyType.Macro))
                            keyType += DS4KeyType.Macro;
                        if (dcs.keyType.HasFlag(DS4KeyType.Toggle))
                            keyType += DS4KeyType.Toggle;
                        if (dcs.keyType.HasFlag(DS4KeyType.ScanCode))
                            keyType += DS4KeyType.ScanCode;
                        if (keyType != string.Empty)
                        {
                            buttonNode = m_Xdoc.CreateNode(XmlNodeType.Element, dcs.control.ToString(), null);
                            buttonNode.InnerText = keyType;
                            KeyType.AppendChild(buttonNode);
                        }

                        buttonNode = m_Xdoc.CreateNode(XmlNodeType.Element, dcs.control.ToString(), null);
                        if (dcs.action is IEnumerable<int> || dcs.action is int[] || dcs.action is ushort[])
                        {
                            var ii = (int[])dcs.action;
                            buttonNode.InnerText = string.Join("/", ii);
                            macro.AppendChild(buttonNode);
                        }
                        else if (dcs.action is int || dcs.action is ushort || dcs.action is byte)
                        {
                            buttonNode.InnerText = dcs.action.ToString();
                            key.AppendChild(buttonNode);
                        }
                        else if (dcs.action is string || dcs.action is X360Controls)
                        {
                            buttonNode.InnerText = dcs.action.ToString();
                            button.AppendChild(buttonNode);
                        }
                    }

                    var hasvalue = false;
                    if (!string.IsNullOrEmpty(dcs.extras))
                        foreach (var s in dcs.extras.Split(','))
                            if (s != "0")
                            {
                                hasvalue = true;
                                break;
                            }
                    if (hasvalue)
                    {
                        var extraNode = m_Xdoc.CreateNode(XmlNodeType.Element, dcs.control.ToString(), null);
                        extraNode.InnerText = dcs.extras;
                        Extras.AppendChild(extraNode);
                    }

                    if (dcs.shiftAction != null && dcs.shiftTrigger > 0)
                    {
                        XmlElement buttonNode;
                        var keyType = string.Empty;

                        if (dcs.shiftAction is string)
                            if (dcs.shiftAction.ToString() == "Unbound")
                                keyType += DS4KeyType.Unbound;
                        if (dcs.shiftKeyType.HasFlag(DS4KeyType.HoldMacro))
                            keyType += DS4KeyType.HoldMacro;
                        if (dcs.shiftKeyType.HasFlag(DS4KeyType.Macro))
                            keyType += DS4KeyType.Macro;
                        if (dcs.shiftKeyType.HasFlag(DS4KeyType.Toggle))
                            keyType += DS4KeyType.Toggle;
                        if (dcs.shiftKeyType.HasFlag(DS4KeyType.ScanCode))
                            keyType += DS4KeyType.ScanCode;
                        if (keyType != string.Empty)
                        {
                            buttonNode = m_Xdoc.CreateElement(dcs.control.ToString());
                            buttonNode.InnerText = keyType;
                            ShiftKeyType.AppendChild(buttonNode);
                        }

                        buttonNode = m_Xdoc.CreateElement(dcs.control.ToString());
                        buttonNode.SetAttribute("Trigger", dcs.shiftTrigger.ToString());
                        if (dcs.shiftAction is IEnumerable<int> || dcs.shiftAction is int[] || dcs.shiftAction is ushort[])
                        {
                            var ii = (int[])dcs.shiftAction;
                            buttonNode.InnerText = string.Join("/", ii);
                            ShiftMacro.AppendChild(buttonNode);
                        }
                        else if (dcs.shiftAction is int || dcs.shiftAction is ushort || dcs.shiftAction is byte)
                        {
                            buttonNode.InnerText = dcs.shiftAction.ToString();
                            ShiftKey.AppendChild(buttonNode);
                        }
                        else if (dcs.shiftAction is string || dcs.shiftAction is X360Controls)
                        {
                            buttonNode.InnerText = dcs.shiftAction.ToString();
                            ShiftButton.AppendChild(buttonNode);
                        }
                    }
                    hasvalue = false;
                    if (!string.IsNullOrEmpty(dcs.shiftExtras))
                        foreach (var s in dcs.shiftExtras.Split(','))
                            if (s != "0")
                            {
                                hasvalue = true;
                                break;
                            }
                    if (hasvalue)
                    {
                        var extraNode = m_Xdoc.CreateNode(XmlNodeType.Element, dcs.control.ToString(), null);
                        extraNode.InnerText = dcs.shiftExtras;
                        ShiftExtras.AppendChild(extraNode);
                    }
                }
                Node.AppendChild(nodeControl);
                if (button.HasChildNodes)
                    nodeControl.AppendChild(button);
                if (macro.HasChildNodes)
                    nodeControl.AppendChild(macro);
                if (key.HasChildNodes)
                    nodeControl.AppendChild(key);
                if (Extras.HasChildNodes)
                    nodeControl.AppendChild(Extras);
                if (KeyType.HasChildNodes)
                    nodeControl.AppendChild(KeyType);
                if (nodeControl.HasChildNodes)
                    Node.AppendChild(nodeControl);

                Node.AppendChild(NodeShiftControl);
                if (ShiftButton.HasChildNodes)
                    NodeShiftControl.AppendChild(ShiftButton);
                if (ShiftMacro.HasChildNodes)
                    NodeShiftControl.AppendChild(ShiftMacro);
                if (ShiftKey.HasChildNodes)
                    NodeShiftControl.AppendChild(ShiftKey);
                if (ShiftKeyType.HasChildNodes)
                    NodeShiftControl.AppendChild(ShiftKeyType);
                if (ShiftExtras.HasChildNodes)
                    NodeShiftControl.AppendChild(ShiftExtras);
                /*else if (xmlControls != null)
                {
                    Node.AppendChild(xmlControls);
                }*/
                /*if (shiftModifier[device] > 0)
                {
                    XmlNode NodeShiftControl = m_Xdoc.CreateNode(XmlNodeType.Element, "ShiftControl", null);

                    XmlNode ShiftKey = m_Xdoc.CreateNode(XmlNodeType.Element, "Key", null);
                    XmlNode ShiftMacro = m_Xdoc.CreateNode(XmlNodeType.Element, "Macro", null);
                    XmlNode ShiftKeyType = m_Xdoc.CreateNode(XmlNodeType.Element, "KeyType", null);
                    XmlNode ShiftButton = m_Xdoc.CreateNode(XmlNodeType.Element, "Button", null);
                    XmlNode ShiftExtras = m_Xdoc.CreateNode(XmlNodeType.Element, "Extras", null);
                    if (shiftbuttons != null)
                    {
                        foreach (var button in shiftbuttons)
                        {
                            // Save even if string (for xbox controller buttons)
                            if (button.Tag != null)
                            {
                                XmlNode buttonNode;
                                string keyType = String.Empty;
                                if (button.Tag is KeyValuePair<string, string>)
                                    if (((KeyValuePair<string, string>)button.Tag).Key == "Unbound")
                                        keyType += DS4KeyType.Unbound;

                                if (button.Font.Strikeout)
                                    keyType += DS4KeyType.HoldMacro;
                                if (button.Font.Underline)
                                    keyType += DS4KeyType.Macro;
                                if (button.Font.Italic)
                                    keyType += DS4KeyType.Toggle;
                                if (button.Font.Bold)
                                    keyType += DS4KeyType.ScanCode;
                                if (keyType != String.Empty)
                                {
                                    buttonNode = m_Xdoc.CreateNode(XmlNodeType.Element, button.Name, null);
                                    buttonNode.InnerText = keyType;
                                    ShiftKeyType.AppendChild(buttonNode);
                                }

                                string[] extras;
                                buttonNode = m_Xdoc.CreateNode(XmlNodeType.Element, button.Name, null);
                                if (button.Tag is KeyValuePair<IEnumerable<int>, string> || button.Tag is KeyValuePair<Int32[], string> || button.Tag is KeyValuePair<UInt16[], string>)
                                {
                                    KeyValuePair<Int32[], string> tag = (KeyValuePair<Int32[], string>)button.Tag;
                                    int[] ii = tag.Key;
                                    buttonNode.InnerText = string.Join("/", ii);
                                    ShiftMacro.AppendChild(buttonNode);
                                    extras = tag.Value.Split(',');
                                }
                                else if (button.Tag is KeyValuePair<Int32, string> || button.Tag is KeyValuePair<UInt16, string> || button.Tag is KeyValuePair<byte, string>)
                                {
                                    KeyValuePair<int, string> tag = (KeyValuePair<int, string>)button.Tag;
                                    buttonNode.InnerText = tag.Key.ToString();
                                    ShiftKey.AppendChild(buttonNode);
                                    extras = tag.Value.Split(',');
                                }
                                else if (button.Tag is KeyValuePair<string, string>)
                                {
                                    KeyValuePair<string, string> tag = (KeyValuePair<string, string>)button.Tag;
                                    buttonNode.InnerText = tag.Key;
                                    ShiftButton.AppendChild(buttonNode);
                                    extras = tag.Value.Split(',');
                                }
                                else
                                {
                                    KeyValuePair<object, string> tag = (KeyValuePair<object, string>)button.Tag;
                                    extras = tag.Value.Split(',');
                                }
                                bool hasvalue = false;
                                foreach (string s in extras)
                                    if (s != "0")
                                    {
                                        hasvalue = true;
                                        break;
                                    }
                                if (hasvalue && !string.IsNullOrEmpty(String.Join(",", extras)))
                                {
                                    XmlNode extraNode = m_Xdoc.CreateNode(XmlNodeType.Element, button.Name, null);
                                    extraNode.InnerText = String.Join(",", extras);
                                    ShiftExtras.AppendChild(extraNode);
                                }
                            }
                        }
                        Node.AppendChild(NodeShiftControl);
                        if (ShiftButton.HasChildNodes)
                            NodeShiftControl.AppendChild(ShiftButton);
                        if (ShiftMacro.HasChildNodes)
                            NodeShiftControl.AppendChild(ShiftMacro);
                        if (ShiftKey.HasChildNodes)
                            NodeShiftControl.AppendChild(ShiftKey);
                        if (ShiftKeyType.HasChildNodes)
                            NodeShiftControl.AppendChild(ShiftKeyType);
                    }
                    else if (xmlShiftControls != null)
                        Node.AppendChild(xmlShiftControls);
                }*/
                m_Xdoc.AppendChild(Node);
                m_Xdoc.Save(path);
            }
            catch
            {
                saved = false;
            }
            return saved;
        }

        private DS4Controls getDS4ControlsByName(string key)
        {
            if (!key.StartsWith("bn"))
                return (DS4Controls)Enum.Parse(typeof(DS4Controls), key, true);

            switch (key)
            {
                case "bnShare":
                    return DS4Controls.Share;
                case "bnL3":
                    return DS4Controls.L3;
                case "bnR3":
                    return DS4Controls.R3;
                case "bnOptions":
                    return DS4Controls.Options;
                case "bnUp":
                    return DS4Controls.DpadUp;
                case "bnRight":
                    return DS4Controls.DpadRight;
                case "bnDown":
                    return DS4Controls.DpadDown;
                case "bnLeft":
                    return DS4Controls.DpadLeft;

                case "bnL1":
                    return DS4Controls.L1;
                case "bnR1":
                    return DS4Controls.R1;
                case "bnTriangle":
                    return DS4Controls.Triangle;
                case "bnCircle":
                    return DS4Controls.Circle;
                case "bnCross":
                    return DS4Controls.Cross;
                case "bnSquare":
                    return DS4Controls.Square;

                case "bnPS":
                    return DS4Controls.PS;
                case "bnLSLeft":
                    return DS4Controls.LXNeg;
                case "bnLSUp":
                    return DS4Controls.LYNeg;
                case "bnRSLeft":
                    return DS4Controls.RXNeg;
                case "bnRSUp":
                    return DS4Controls.RYNeg;

                case "bnLSRight":
                    return DS4Controls.LXPos;
                case "bnLSDown":
                    return DS4Controls.LYPos;
                case "bnRSRight":
                    return DS4Controls.RXPos;
                case "bnRSDown":
                    return DS4Controls.RYPos;
                case "bnL2":
                    return DS4Controls.L2;
                case "bnR2":
                    return DS4Controls.R2;

                case "bnTouchLeft":
                    return DS4Controls.TouchLeft;
                case "bnTouchMulti":
                    return DS4Controls.TouchMulti;
                case "bnTouchUpper":
                    return DS4Controls.TouchUpper;
                case "bnTouchRight":
                    return DS4Controls.TouchRight;
                case "bnGyroXP":
                    return DS4Controls.GyroXPos;
                case "bnGyroXN":
                    return DS4Controls.GyroXNeg;
                case "bnGyroZP":
                    return DS4Controls.GyroZPos;
                case "bnGyroZN":
                    return DS4Controls.GyroZNeg;

                case "bnSwipeUp":
                    return DS4Controls.SwipeUp;
                case "bnSwipeDown":
                    return DS4Controls.SwipeDown;
                case "bnSwipeLeft":
                    return DS4Controls.SwipeLeft;
                case "bnSwipeRight":
                    return DS4Controls.SwipeRight;

                    #region OldShiftname

                case "sbnShare":
                    return DS4Controls.Share;
                case "sbnL3":
                    return DS4Controls.L3;
                case "sbnR3":
                    return DS4Controls.R3;
                case "sbnOptions":
                    return DS4Controls.Options;
                case "sbnUp":
                    return DS4Controls.DpadUp;
                case "sbnRight":
                    return DS4Controls.DpadRight;
                case "sbnDown":
                    return DS4Controls.DpadDown;
                case "sbnLeft":
                    return DS4Controls.DpadLeft;

                case "sbnL1":
                    return DS4Controls.L1;
                case "sbnR1":
                    return DS4Controls.R1;
                case "sbnTriangle":
                    return DS4Controls.Triangle;
                case "sbnCircle":
                    return DS4Controls.Circle;
                case "sbnCross":
                    return DS4Controls.Cross;
                case "sbnSquare":
                    return DS4Controls.Square;

                case "sbnPS":
                    return DS4Controls.PS;
                case "sbnLSLeft":
                    return DS4Controls.LXNeg;
                case "sbnLSUp":
                    return DS4Controls.LYNeg;
                case "sbnRSLeft":
                    return DS4Controls.RXNeg;
                case "sbnRSUp":
                    return DS4Controls.RYNeg;

                case "sbnLSRight":
                    return DS4Controls.LXPos;
                case "sbnLSDown":
                    return DS4Controls.LYPos;
                case "sbnRSRight":
                    return DS4Controls.RXPos;
                case "sbnRSDown":
                    return DS4Controls.RYPos;
                case "sbnL2":
                    return DS4Controls.L2;
                case "sbnR2":
                    return DS4Controls.R2;

                case "sbnTouchLeft":
                    return DS4Controls.TouchLeft;
                case "sbnTouchMulti":
                    return DS4Controls.TouchMulti;
                case "sbnTouchUpper":
                    return DS4Controls.TouchUpper;
                case "sbnTouchRight":
                    return DS4Controls.TouchRight;
                case "sbnGsyroXP":
                    return DS4Controls.GyroXPos;
                case "sbnGyroXN":
                    return DS4Controls.GyroXNeg;
                case "sbnGyroZP":
                    return DS4Controls.GyroZPos;
                case "sbnGyroZN":
                    return DS4Controls.GyroZNeg;

                    #endregion

                case "bnShiftShare":
                    return DS4Controls.Share;
                case "bnShiftL3":
                    return DS4Controls.L3;
                case "bnShiftR3":
                    return DS4Controls.R3;
                case "bnShiftOptions":
                    return DS4Controls.Options;
                case "bnShiftUp":
                    return DS4Controls.DpadUp;
                case "bnShiftRight":
                    return DS4Controls.DpadRight;
                case "bnShiftDown":
                    return DS4Controls.DpadDown;
                case "bnShiftLeft":
                    return DS4Controls.DpadLeft;

                case "bnShiftL1":
                    return DS4Controls.L1;
                case "bnShiftR1":
                    return DS4Controls.R1;
                case "bnShiftTriangle":
                    return DS4Controls.Triangle;
                case "bnShiftCircle":
                    return DS4Controls.Circle;
                case "bnShiftCross":
                    return DS4Controls.Cross;
                case "bnShiftSquare":
                    return DS4Controls.Square;

                case "bnShiftPS":
                    return DS4Controls.PS;
                case "bnShiftLSLeft":
                    return DS4Controls.LXNeg;
                case "bnShiftLSUp":
                    return DS4Controls.LYNeg;
                case "bnShiftRSLeft":
                    return DS4Controls.RXNeg;
                case "bnShiftRSUp":
                    return DS4Controls.RYNeg;

                case "bnShiftLSRight":
                    return DS4Controls.LXPos;
                case "bnShiftLSDown":
                    return DS4Controls.LYPos;
                case "bnShiftRSRight":
                    return DS4Controls.RXPos;
                case "bnShiftRSDown":
                    return DS4Controls.RYPos;
                case "bnShiftL2":
                    return DS4Controls.L2;
                case "bnShiftR2":
                    return DS4Controls.R2;

                case "bnShiftTouchLeft":
                    return DS4Controls.TouchLeft;
                case "bnShiftTouchMulti":
                    return DS4Controls.TouchMulti;
                case "bnShiftTouchUpper":
                    return DS4Controls.TouchUpper;
                case "bnShiftTouchRight":
                    return DS4Controls.TouchRight;
                case "bnShiftGyroXP":
                    return DS4Controls.GyroXPos;
                case "bnShiftGyroXN":
                    return DS4Controls.GyroXNeg;
                case "bnShiftGyroZP":
                    return DS4Controls.GyroZPos;
                case "bnShiftGyroZN":
                    return DS4Controls.GyroZNeg;

                case "bnShiftSwipeUp":
                    return DS4Controls.SwipeUp;
                case "bnShiftSwipeDown":
                    return DS4Controls.SwipeDown;
                case "bnShiftSwipeLeft":
                    return DS4Controls.SwipeLeft;
                case "bnShiftSwipeRight":
                    return DS4Controls.SwipeRight;
            }
            return 0;
        }

        private X360Controls getX360ControlsByName(string key)
        {
            X360Controls x3c;
            if (Enum.TryParse(key, true, out x3c))
                return x3c;
            switch (key)
            {
                case "Back":
                    return X360Controls.Back;
                case "Left Stick":
                    return X360Controls.LS;
                case "Right Stick":
                    return X360Controls.RS;
                case "Start":
                    return X360Controls.Start;
                case "Up Button":
                    return X360Controls.DpadUp;
                case "Right Button":
                    return X360Controls.DpadRight;
                case "Down Button":
                    return X360Controls.DpadDown;
                case "Left Button":
                    return X360Controls.DpadLeft;

                case "Left Bumper":
                    return X360Controls.LB;
                case "Right Bumper":
                    return X360Controls.RB;
                case "Y Button":
                    return X360Controls.Y;
                case "B Button":
                    return X360Controls.B;
                case "A Button":
                    return X360Controls.A;
                case "X Button":
                    return X360Controls.X;

                case "Guide":
                    return X360Controls.Guide;
                case "Left X-Axis-":
                    return X360Controls.LXNeg;
                case "Left Y-Axis-":
                    return X360Controls.LYNeg;
                case "Right X-Axis-":
                    return X360Controls.RXNeg;
                case "Right Y-Axis-":
                    return X360Controls.RYNeg;

                case "Left X-Axis+":
                    return X360Controls.LXPos;
                case "Left Y-Axis+":
                    return X360Controls.LYPos;
                case "Right X-Axis+":
                    return X360Controls.RXPos;
                case "Right Y-Axis+":
                    return X360Controls.RYPos;
                case "Left Trigger":
                    return X360Controls.LT;
                case "Right Trigger":
                    return X360Controls.RT;

                case "Left Mouse Button":
                    return X360Controls.LeftMouse;
                case "Right Mouse Button":
                    return X360Controls.RightMouse;
                case "Middle Mouse Button":
                    return X360Controls.MiddleMouse;
                case "4th Mouse Button":
                    return X360Controls.FourthMouse;
                case "5th Mouse Button":
                    return X360Controls.FifthMouse;
                case "Mouse Wheel Up":
                    return X360Controls.WUP;
                case "Mouse Wheel Down":
                    return X360Controls.WDOWN;
                case "Mouse Up":
                    return X360Controls.MouseUp;
                case "Mouse Down":
                    return X360Controls.MouseDown;
                case "Mouse Left":
                    return X360Controls.MouseLeft;
                case "Mouse Right":
                    return X360Controls.MouseRight;
                case "Unbound":
                    return X360Controls.Unbound;
            }
            return X360Controls.Unbound;
        }

        public bool LoadProfile(int device, bool launchprogram, ControlService control, string propath = "")
        {
            var Loaded = true;
            var customMapKeyTypes = new Dictionary<DS4Controls, DS4KeyType>();
            var customMapKeys = new Dictionary<DS4Controls, ushort>();
            var customMapButtons = new Dictionary<DS4Controls, X360Controls>();
            var customMapMacros = new Dictionary<DS4Controls, string>();
            var customMapExtras = new Dictionary<DS4Controls, string>();
            var shiftCustomMapKeyTypes = new Dictionary<DS4Controls, DS4KeyType>();
            var shiftCustomMapKeys = new Dictionary<DS4Controls, ushort>();
            var shiftCustomMapButtons = new Dictionary<DS4Controls, X360Controls>();
            var shiftCustomMapMacros = new Dictionary<DS4Controls, string>();
            var shiftCustomMapExtras = new Dictionary<DS4Controls, string>();
            var rootname = "DS4Windows";
            var missingSetting = false;
            string profilepath;
            if (propath == "")
                profilepath = Global.appdatapath + @"\Profiles\" + profilePath[device] + ".xml";
            else
                profilepath = propath;
            if (File.Exists(profilepath))
            {
                XmlNode Item;

                m_Xdoc.Load(profilepath);
                if (m_Xdoc.SelectSingleNode(rootname) == null)
                {
                    rootname = "ScpControl";
                    missingSetting = true;
                }
                if (device < 4)
                {
                    DS4LightBar.forcelight[device] = false;
                    DS4LightBar.forcedFlash[device] = 0;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/flushHIDQueue");
                    bool.TryParse(Item.InnerText, out flushHIDQueue[device]);
                }
                catch
                {
                    missingSetting = true;
                } //rootname = }

                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/idleDisconnectTimeout");
                    int.TryParse(Item.InnerText, out idleDisconnectTimeout[device]);
                }
                catch
                {
                    missingSetting = true;
                }
                //New method for saving color
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/Color");
                    string[] colors;
                    if (!string.IsNullOrEmpty(Item.InnerText))
                        colors = Item.InnerText.Split(',');
                    else
                        colors = new string[0];
                    m_Leds[device].Red = byte.Parse(colors[0]);
                    m_Leds[device].Green = byte.Parse(colors[1]);
                    m_Leds[device].Blue = byte.Parse(colors[2]);
                }
                catch
                {
                    missingSetting = true;
                }
                if (m_Xdoc.SelectSingleNode("/" + rootname + "/Color") == null)
                {
                    //Old method of color saving
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/Red");
                        byte.TryParse(Item.InnerText, out m_Leds[device].Red);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/Green");
                        byte.TryParse(Item.InnerText, out m_Leds[device].Green);
                    }
                    catch
                    {
                        missingSetting = true;
                    }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/Blue");
                        byte.TryParse(Item.InnerText, out m_Leds[device].Blue);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RumbleBoost");
                    byte.TryParse(Item.InnerText, out rumble[device]);
                }
                catch
                {
                    missingSetting = true;
                }

                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ledAsBatteryIndicator");
                    bool.TryParse(Item.InnerText, out ledAsBattery[device]);
                }
                catch
                {
                    missingSetting = true;
                }

                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/FlashType");
                    byte.TryParse(Item.InnerText, out flashType[device]);
                }
                catch
                {
                    missingSetting = true;
                }

                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/flashBatteryAt");
                    int.TryParse(Item.InnerText, out flashAt[device]);
                }
                catch
                {
                    missingSetting = true;
                }

                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/touchSensitivity");
                    byte.TryParse(Item.InnerText, out touchSensitivity[device]);
                }
                catch
                {
                    missingSetting = true;
                }
                //New method for saving color
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LowColor");
                    string[] colors;
                    if (!string.IsNullOrEmpty(Item.InnerText))
                        colors = Item.InnerText.Split(',');
                    else
                        colors = new string[0];
                    m_LowLeds[device].Red = byte.Parse(colors[0]);
                    m_LowLeds[device].Green = byte.Parse(colors[1]);
                    m_LowLeds[device].Blue = byte.Parse(colors[2]);
                }
                catch
                {
                    missingSetting = true;
                }
                if (m_Xdoc.SelectSingleNode("/" + rootname + "/LowColor") == null)
                {
                    //Old method of color saving
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LowRed");
                        byte.TryParse(Item.InnerText, out m_LowLeds[device].Red);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LowGreen");
                        byte.TryParse(Item.InnerText, out m_LowLeds[device].Green);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LowBlue");
                        byte.TryParse(Item.InnerText, out m_LowLeds[device].Blue);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                }
                //New method for saving color
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ChargingColor");
                    string[] colors;
                    if (!string.IsNullOrEmpty(Item.InnerText))
                        colors = Item.InnerText.Split(',');
                    else
                        colors = new string[0];

                    m_ChargingLeds[device].Red = byte.Parse(colors[0]);
                    m_ChargingLeds[device].Green = byte.Parse(colors[1]);
                    m_ChargingLeds[device].Blue = byte.Parse(colors[2]);
                }
                catch
                {
                    missingSetting = true;
                }
                if (m_Xdoc.SelectSingleNode("/" + rootname + "/ChargingColor") == null)
                {
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ChargingRed");
                        byte.TryParse(Item.InnerText, out m_ChargingLeds[device].Red);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ChargingGreen");
                        byte.TryParse(Item.InnerText, out m_ChargingLeds[device].Green);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ChargingBlue");
                        byte.TryParse(Item.InnerText, out m_ChargingLeds[device].Blue);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/FlashColor");
                    string[] colors;
                    if (!string.IsNullOrEmpty(Item.InnerText))
                        colors = Item.InnerText.Split(',');
                    else
                        colors = new string[0];
                    m_FlashLeds[device].Red = byte.Parse(colors[0]);
                    m_FlashLeds[device].Green = byte.Parse(colors[1]);
                    m_FlashLeds[device].Blue = byte.Parse(colors[2]);
                }
                catch
                {
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/touchpadJitterCompensation");
                    bool.TryParse(Item.InnerText, out touchpadJitterCompensation[device]);
                }
                catch
                {
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/lowerRCOn");
                    bool.TryParse(Item.InnerText, out lowerRCOn[device]);
                }
                catch
                {
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/tapSensitivity");
                    byte.TryParse(Item.InnerText, out tapSensitivity[device]);
                }
                catch
                {
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/doubleTap");
                    bool.TryParse(Item.InnerText, out doubleTap[device]);
                }
                catch
                {
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/scrollSensitivity");
                    int.TryParse(Item.InnerText, out scrollSensitivity[device]);
                }
                catch
                {
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LeftTriggerMiddle");
                    byte.TryParse(Item.InnerText, out l2Deadzone[device]);
                }
                catch
                {
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RightTriggerMiddle");
                    byte.TryParse(Item.InnerText, out r2Deadzone[device]);
                }
                catch
                {
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ButtonMouseSensitivity");
                    int.TryParse(Item.InnerText, out buttonMouseSensitivity[device]);
                }
                catch
                {
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/Rainbow");
                    double.TryParse(Item.InnerText, out rainbow[device]);
                }
                catch
                {
                    rainbow[device] = 0;
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSDeadZone");
                    int.TryParse(Item.InnerText, out LSDeadzone[device]);
                }
                catch
                {
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSDeadZone");
                    int.TryParse(Item.InnerText, out RSDeadzone[device]);
                }
                catch
                {
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SXDeadZone");
                    double.TryParse(Item.InnerText, out SXDeadzone[device]);
                }
                catch
                {
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SZDeadZone");
                    double.TryParse(Item.InnerText, out SZDeadzone[device]);
                }
                catch
                {
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/Sensitivity");
                    var s = Item.InnerText.Split('|');
                    if (s.Length == 1)
                        s = Item.InnerText.Split(',');
                    if (!double.TryParse(s[0], out LSSens[device]) || LSSens[device] < .5f)
                        LSSens[device] = 1;
                    if (!double.TryParse(s[1], out RSSens[device]) || RSSens[device] < .5f)
                        RSSens[device] = 1;
                    if (!double.TryParse(s[2], out l2Sens[device]) || l2Sens[device] < .5f)
                        l2Sens[device] = 1;
                    if (!double.TryParse(s[3], out r2Sens[device]) || r2Sens[device] < .5f)
                        r2Sens[device] = 1;
                    if (!double.TryParse(s[4], out SXSens[device]) || SXSens[device] < .5f)
                        SXSens[device] = 1;
                    if (!double.TryParse(s[5], out SZSens[device]) || SZSens[device] < .5f)
                        SZSens[device] = 1;
                }
                catch
                {
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ChargingType");
                    int.TryParse(Item.InnerText, out chargingType[device]);
                }
                catch
                {
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/MouseAcceleration");
                    bool.TryParse(Item.InnerText, out mouseAccel[device]);
                }
                catch
                {
                    missingSetting = true;
                }
                var shiftM = 0;
                if (m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftModifier") != null)
                    int.TryParse(m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftModifier").InnerText, out shiftM);
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LaunchProgram");
                    launchProgram[device] = Item.InnerText;
                    if (launchprogram == true && launchProgram[device] != string.Empty) System.Diagnostics.Process.Start(launchProgram[device]);
                }
                catch
                {
                    launchProgram[device] = string.Empty;
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/DinputOnly");
                    bool.TryParse(Item.InnerText, out dinputOnly[device]);
                    if (device < 4)
                    {
                        if (dinputOnly[device] == true) control.X360Bus.Unplug(device);
                        else if (control.Controllers[device] != null && control.Controllers[device].IsAlive) control.X360Bus.Plugin(device);
                    }
                }
                catch
                {
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/StartTouchpadOff");
                    bool.TryParse(Item.InnerText, out startTouchpadOff[device]);
                    if (startTouchpadOff[device] == true) control.StartTPOff(device);
                }
                catch
                {
                    startTouchpadOff[device] = false;
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/UseTPforControls");
                    bool.TryParse(Item.InnerText, out useTPforControls[device]);
                }
                catch
                {
                    useTPforControls[device] = false;
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/UseSAforMouse");
                    bool.TryParse(Item.InnerText, out useSAforMouse[device]);
                }
                catch
                {
                    useSAforMouse[device] = false;
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SATriggers");
                    sATriggers[device] = Item.InnerText;
                }
                catch
                {
                    sATriggers[device] = "";
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroSensitivity");
                    int.TryParse(Item.InnerText, out gyroSensitivity[device]);
                }
                catch
                {
                    gyroSensitivity[device] = 100;
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroInvert");
                    int.TryParse(Item.InnerText, out gyroInvert[device]);
                }
                catch
                {
                    gyroInvert[device] = 0;
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSCurve");
                    int.TryParse(Item.InnerText, out lsCurve[device]);
                }
                catch
                {
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSCurve");
                    int.TryParse(Item.InnerText, out rsCurve[device]);
                }
                catch
                {
                    missingSetting = true;
                }
                try
                {
                    Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ProfileActions");
                    profileActions[device].Clear();
                    if (!string.IsNullOrEmpty(Item.InnerText))
                        profileActions[device].AddRange(Item.InnerText.Split('/'));
                }
                catch
                {
                    profileActions[device].Clear();
                    missingSetting = true;
                }

                foreach (var dcs in Ds4Settings[device])
                    dcs.Reset();

                DS4KeyType keyType;
                ushort wvk;

                {
                    var ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/Control/Button");
                    if (ParentItem != null)
                        foreach (XmlNode item in ParentItem.ChildNodes)
                        {
                            UpdateDS4CSetting(device, item.Name, false, getX360ControlsByName(item.InnerText), "", DS4KeyType.None, 0);
                            customMapButtons.Add(getDS4ControlsByName(item.Name), getX360ControlsByName(item.InnerText));
                        }
                    ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/Control/Macro");
                    if (ParentItem != null)
                        foreach (XmlNode item in ParentItem.ChildNodes)
                        {
                            customMapMacros.Add(getDS4ControlsByName(item.Name), item.InnerText);
                            string[] skeys;
                            int[] keys;
                            if (!string.IsNullOrEmpty(item.InnerText))
                            {
                                skeys = item.InnerText.Split('/');
                                keys = new int[skeys.Length];
                            }
                            else
                            {
                                skeys = new string[0];
                                keys = new int[0];
                            }
                            for (var i = 0; i < keys.Length; i++)
                                keys[i] = int.Parse(skeys[i]);
                            UpdateDS4CSetting(device, item.Name, false, keys, "", DS4KeyType.None, 0);
                        }
                    ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/Control/Key");
                    if (ParentItem != null)
                        foreach (XmlNode item in ParentItem.ChildNodes)
                            if (ushort.TryParse(item.InnerText, out wvk))
                            {
                                UpdateDS4CSetting(device, item.Name, false, wvk, "", DS4KeyType.None, 0);
                                customMapKeys.Add(getDS4ControlsByName(item.Name), wvk);
                            }
                    ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/Control/Extras");
                    if (ParentItem != null)
                        foreach (XmlNode item in ParentItem.ChildNodes)
                            if (item.InnerText != string.Empty)
                            {
                                UpdateDS4CExtra(device, item.Name, false, item.InnerText);
                                customMapExtras.Add(getDS4ControlsByName(item.Name), item.InnerText);
                            }
                            else
                                ParentItem.RemoveChild(item);
                    ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/Control/KeyType");
                    if (ParentItem != null)
                        foreach (XmlNode item in ParentItem.ChildNodes)
                            if (item != null)
                            {
                                keyType = DS4KeyType.None;
                                if (item.InnerText.Contains(DS4KeyType.ScanCode.ToString()))
                                    keyType |= DS4KeyType.ScanCode;
                                if (item.InnerText.Contains(DS4KeyType.Toggle.ToString()))
                                    keyType |= DS4KeyType.Toggle;
                                if (item.InnerText.Contains(DS4KeyType.Macro.ToString()))
                                    keyType |= DS4KeyType.Macro;
                                if (item.InnerText.Contains(DS4KeyType.HoldMacro.ToString()))
                                    keyType |= DS4KeyType.HoldMacro;
                                if (item.InnerText.Contains(DS4KeyType.Unbound.ToString()))
                                    keyType |= DS4KeyType.Unbound;
                                if (keyType != DS4KeyType.None)
                                {
                                    UpdateDS4CKeyType(device, item.Name, false, keyType);
                                    customMapKeyTypes.Add(getDS4ControlsByName(item.Name), keyType);
                                }
                            }

                    ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftControl/Button");
                    if (ParentItem != null)
                        foreach (XmlElement item in ParentItem.ChildNodes)
                        {
                            var shiftT = shiftM;
                            if (item.HasAttribute("Trigger"))
                                int.TryParse(item.Attributes["Trigger"].Value, out shiftT);
                            UpdateDS4CSetting(device, item.Name, true, getX360ControlsByName(item.InnerText), "", DS4KeyType.None, shiftT);
                            shiftCustomMapButtons.Add(getDS4ControlsByName(item.Name), getX360ControlsByName(item.InnerText));
                        }
                    ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftControl/Macro");
                    if (ParentItem != null)
                        foreach (XmlElement item in ParentItem.ChildNodes)
                        {
                            shiftCustomMapMacros.Add(getDS4ControlsByName(item.Name), item.InnerText);
                            string[] skeys;
                            int[] keys;
                            if (!string.IsNullOrEmpty(item.InnerText))
                            {
                                skeys = item.InnerText.Split('/');
                                keys = new int[skeys.Length];
                            }
                            else
                            {
                                skeys = new string[0];
                                keys = new int[0];
                            }
                            for (var i = 0; i < keys.Length; i++)
                                keys[i] = int.Parse(skeys[i]);
                            var shiftT = shiftM;
                            if (item.HasAttribute("Trigger"))
                                int.TryParse(item.Attributes["Trigger"].Value, out shiftT);
                            UpdateDS4CSetting(device, item.Name, true, keys, "", DS4KeyType.None, shiftT);
                        }
                    ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftControl/Key");
                    if (ParentItem != null)
                        foreach (XmlElement item in ParentItem.ChildNodes)
                            if (ushort.TryParse(item.InnerText, out wvk))
                            {
                                var shiftT = shiftM;
                                if (item.HasAttribute("Trigger"))
                                    int.TryParse(item.Attributes["Trigger"].Value, out shiftT);
                                UpdateDS4CSetting(device, item.Name, true, wvk, "", DS4KeyType.None, shiftT);
                                shiftCustomMapKeys.Add(getDS4ControlsByName(item.Name), wvk);
                            }
                    ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftControl/Extras");
                    if (ParentItem != null)
                        foreach (XmlElement item in ParentItem.ChildNodes)
                            if (item.InnerText != string.Empty)
                            {
                                UpdateDS4CExtra(device, item.Name, true, item.InnerText);
                                shiftCustomMapExtras.Add(getDS4ControlsByName(item.Name), item.InnerText);
                            }
                            else
                                ParentItem.RemoveChild(item);
                    ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftControl/KeyType");
                    if (ParentItem != null)
                        foreach (XmlElement item in ParentItem.ChildNodes)
                            if (item != null)
                            {
                                keyType = DS4KeyType.None;
                                if (item.InnerText.Contains(DS4KeyType.ScanCode.ToString()))
                                    keyType |= DS4KeyType.ScanCode;
                                if (item.InnerText.Contains(DS4KeyType.Toggle.ToString()))
                                    keyType |= DS4KeyType.Toggle;
                                if (item.InnerText.Contains(DS4KeyType.Macro.ToString()))
                                    keyType |= DS4KeyType.Macro;
                                if (item.InnerText.Contains(DS4KeyType.HoldMacro.ToString()))
                                    keyType |= DS4KeyType.HoldMacro;
                                if (item.InnerText.Contains(DS4KeyType.Unbound.ToString()))
                                    keyType |= DS4KeyType.Unbound;
                                if (keyType != DS4KeyType.None)
                                {
                                    UpdateDS4CKeyType(device, item.Name, true, keyType);
                                    shiftCustomMapKeyTypes.Add(getDS4ControlsByName(item.Name), keyType);
                                }
                            }
                    //LoadButtons(buttons, "Control", customMapKeyTypes, customMapKeys, customMapButtons, customMapMacros, customMapExtras);
                    //LoadButtons(shiftbuttons, "ShiftControl", shiftCustomMapKeyTypes, shiftCustomMapKeys, shiftCustomMapButtons, shiftCustomMapMacros, shiftCustomMapExtras);                    
                }
            }
            //catch { Loaded = false; }
            /*if (Loaded)
            {
                this.customMapButtons[device] = customMapButtons;
                this.customMapKeys[device] = customMapKeys;
                this.customMapKeyTypes[device] = customMapKeyTypes;
                this.customMapMacros[device] = customMapMacros;
                this.customMapExtras[device] = customMapExtras;

                this.shiftCustomMapButtons[device] = shiftCustomMapButtons;
                this.shiftCustomMapKeys[device] = shiftCustomMapKeys;
                this.shiftCustomMapKeyTypes[device] = shiftCustomMapKeyTypes;
                this.shiftCustomMapMacros[device] = shiftCustomMapMacros;
                this.shiftCustomMapExtras[device] = shiftCustomMapExtras;
            }*/
            // Only add missing settings if the actual load was graceful
            if (missingSetting && Loaded) // && buttons != null)
                SaveProfile(device, profilepath);

            return Loaded;
        }

        //public void LoadButtons(System.Windows.Forms.Control[] buttons, string control, Dictionary<DS4Controls, DS4KeyType> customMapKeyTypes,
        //    Dictionary<DS4Controls, ushort> customMapKeys, Dictionary<DS4Controls, X360Controls> customMapButtons,
        //    Dictionary<DS4Controls, string> customMapMacros, Dictionary<DS4Controls, string> customMapExtras)
        //{
        //    var rootname = "DS4Windows";
        //    foreach (var button in buttons)
        //        try
        //        {
        //            if (m_Xdoc.SelectSingleNode(rootname) == null)
        //                rootname = "ScpControl";

        //            //bool foundBinding = false;
        //            button.Font = new Font(button.Font, FontStyle.Regular);

        //            var item = m_Xdoc.SelectSingleNode(string.Format("/" + rootname + "/" + control + "/KeyType/{0}", button.Name));
        //            if (item != null)
        //            {
        //                //foundBinding = true;
        //                var keyType = DS4KeyType.None;
        //                if (item.InnerText.Contains(DS4KeyType.Unbound.ToString()))
        //                {
        //                    keyType = DS4KeyType.Unbound;
        //                    button.Tag = "Unbound";
        //                    button.Text = "Unbound";
        //                }
        //                else
        //                {
        //                    var SC = item.InnerText.Contains(DS4KeyType.ScanCode.ToString());
        //                    var TG = item.InnerText.Contains(DS4KeyType.Toggle.ToString());
        //                    var MC = item.InnerText.Contains(DS4KeyType.Macro.ToString());
        //                    var MR = item.InnerText.Contains(DS4KeyType.HoldMacro.ToString());
        //                    button.Font = new Font(button.Font,
        //                        (SC ? FontStyle.Bold : FontStyle.Regular) | (TG ? FontStyle.Italic : FontStyle.Regular) |
        //                        (MC ? FontStyle.Underline : FontStyle.Regular) | (MR ? FontStyle.Strikeout : FontStyle.Regular));
        //                    if (item.InnerText.Contains(DS4KeyType.ScanCode.ToString()))
        //                        keyType |= DS4KeyType.ScanCode;
        //                    if (item.InnerText.Contains(DS4KeyType.Toggle.ToString()))
        //                        keyType |= DS4KeyType.Toggle;
        //                    if (item.InnerText.Contains(DS4KeyType.Macro.ToString()))
        //                        keyType |= DS4KeyType.Macro;
        //                }
        //                if (keyType != DS4KeyType.None)
        //                    customMapKeyTypes.Add(getDS4ControlsByName(item.Name), keyType);
        //            }
        //            string extras;
        //            item = m_Xdoc.SelectSingleNode(string.Format("/" + rootname + "/" + control + "/Extras/{0}", button.Name));
        //            if (item != null)
        //            {
        //                if (item.InnerText != string.Empty)
        //                {
        //                    extras = item.InnerText;
        //                    customMapExtras.Add(getDS4ControlsByName(button.Name), item.InnerText);
        //                }
        //                else
        //                {
        //                    m_Xdoc.RemoveChild(item);
        //                    extras = "0,0,0,0,0,0,0,0";
        //                }
        //            }
        //            else
        //                extras = "0,0,0,0,0,0,0,0";
        //            item = m_Xdoc.SelectSingleNode(string.Format("/" + rootname + "/" + control + "/Macro/{0}", button.Name));
        //            if (item != null)
        //            {
        //                var splitter = item.InnerText.Split('/');
        //                var keys = new int[splitter.Length];
        //                for (var i = 0; i < keys.Length; i++)
        //                {
        //                    keys[i] = int.Parse(splitter[i]);
        //                    if (keys[i] < 255) splitter[i] = ((System.Windows.Forms.Keys)keys[i]).ToString();
        //                    else if (keys[i] == 256) splitter[i] = "Left Mouse Button";
        //                    else if (keys[i] == 257) splitter[i] = "Right Mouse Button";
        //                    else if (keys[i] == 258) splitter[i] = "Middle Mouse Button";
        //                    else if (keys[i] == 259) splitter[i] = "4th Mouse Button";
        //                    else if (keys[i] == 260) splitter[i] = "5th Mouse Button";
        //                    else if (keys[i] > 300) splitter[i] = "Wait " + (keys[i] - 300) + "ms";
        //                }
        //                button.Text = "Macro";
        //                button.Tag = new KeyValuePair<int[], string>(keys, extras);
        //                customMapMacros.Add(getDS4ControlsByName(button.Name), item.InnerText);
        //            }
        //            else if (m_Xdoc.SelectSingleNode(string.Format("/" + rootname + "/" + control + "/Key/{0}", button.Name)) != null)
        //            {
        //                item = m_Xdoc.SelectSingleNode(string.Format("/" + rootname + "/" + control + "/Key/{0}", button.Name));
        //                if (ushort.TryParse(item.InnerText, out ushort wvk))
        //                {
        //                    //foundBinding = true;
        //                    customMapKeys.Add(getDS4ControlsByName(item.Name), wvk);
        //                    button.Tag = new KeyValuePair<int, string>(wvk, extras);
        //                    button.Text = ((System.Windows.Forms.Keys)wvk).ToString();
        //                }
        //            }
        //            else if (m_Xdoc.SelectSingleNode(string.Format("/" + rootname + "/" + control + "/Button/{0}", button.Name)) != null)
        //            {
        //                item = m_Xdoc.SelectSingleNode(string.Format("/" + rootname + "/" + control + "/Button/{0}", button.Name));
        //                //foundBinding = true;
        //                button.Tag = new KeyValuePair<string, string>(item.InnerText, extras);
        //                button.Text = item.InnerText;
        //                customMapButtons.Add(getDS4ControlsByName(button.Name), getX360ControlsByName(item.InnerText));
        //            }
        //            else
        //            {
        //                button.Tag = new KeyValuePair<object, string>(null, extras);
        //            }
        //        }
        //        catch
        //        {
        //        }
        //}

        public bool Load()
        {
            var Loaded = true;
            var missingSetting = false;

            try
            {
                if (File.Exists(m_Profile))
                {
                    XmlNode Item;

                    m_Xdoc.Load(m_Profile);

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/useExclusiveMode");
                        bool.TryParse(Item.InnerText, out useExclusiveMode);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/startMinimized");
                        bool.TryParse(Item.InnerText, out startMinimized);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/formWidth");
                        int.TryParse(Item.InnerText, out formWidth);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/formHeight");
                        int.TryParse(Item.InnerText, out formHeight);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/Controller1");
                        profilePath[0] = Item.InnerText;
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/Controller2");
                        profilePath[1] = Item.InnerText;
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/Controller3");
                        profilePath[2] = Item.InnerText;
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/Controller4");
                        profilePath[3] = Item.InnerText;
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/LastChecked");
                        DateTime.TryParse(Item.InnerText, out lastChecked);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/CheckWhen");
                        int.TryParse(Item.InnerText, out CheckWhen);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/Notifications");
                        if (!int.TryParse(Item.InnerText, out notifications))
                            notifications = bool.Parse(Item.InnerText) ? 2 : 0;
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/DisconnectBTAtStop");
                        bool.TryParse(Item.InnerText, out disconnectBTAtStop);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/SwipeProfiles");
                        bool.TryParse(Item.InnerText, out swipeProfiles);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/UseDS4ForMapping");
                        bool.TryParse(Item.InnerText, out ds4Mapping);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/QuickCharge");
                        bool.TryParse(Item.InnerText, out quickCharge);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/FirstXinputPort");
                        int.TryParse(Item.InnerText, out firstXinputPort);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/CloseMinimizes");
                        bool.TryParse(Item.InnerText, out closeMini);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/DownloadLang");
                        bool.TryParse(Item.InnerText, out downloadLang);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/FlashWhenLate");
                        bool.TryParse(Item.InnerText, out flashWhenLate);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/FlashWhenLateAt");
                        int.TryParse(Item.InnerText, out flashWhenLateAt);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/Profile/WhiteIcon");
                        bool.TryParse(Item.InnerText, out useWhiteIcon);
                    }
                    catch
                    {
                        missingSetting = true;
                    }
                    for (var i = 0; i < 4; i++)
                    {
                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/Profile/CustomLed" + (i + 1));
                            var ss = Item.InnerText.Split(':');
                            bool.TryParse(ss[0], out useCustomLeds[i]);
                            LightBarColour.TryParse(ss[1], ref m_CustomLeds[i]);
                        }
                        catch
                        {
                            missingSetting = true;
                        }
                    }
                }
            }
            catch
            {
            }
            if (missingSetting)
                Save();
            return Loaded;
        }

        public bool Save()
        {
            var Saved = true;

            XmlNode Node;

            m_Xdoc.RemoveAll();

            Node = m_Xdoc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
            m_Xdoc.AppendChild(Node);

            Node = m_Xdoc.CreateComment($" Profile Configuration Data. {DateTime.Now} ");
            m_Xdoc.AppendChild(Node);

            Node = m_Xdoc.CreateWhitespace("\r\n");
            m_Xdoc.AppendChild(Node);

            Node = m_Xdoc.CreateNode(XmlNodeType.Element, "Profile", null);


            var xmlUseExclNode = m_Xdoc.CreateNode(XmlNodeType.Element, "useExclusiveMode", null);
            xmlUseExclNode.InnerText = useExclusiveMode.ToString();
            Node.AppendChild(xmlUseExclNode);
            var xmlStartMinimized = m_Xdoc.CreateNode(XmlNodeType.Element, "startMinimized", null);
            xmlStartMinimized.InnerText = startMinimized.ToString();
            Node.AppendChild(xmlStartMinimized);
            var xmlFormWidth = m_Xdoc.CreateNode(XmlNodeType.Element, "formWidth", null);
            xmlFormWidth.InnerText = formWidth.ToString();
            Node.AppendChild(xmlFormWidth);
            var xmlFormHeight = m_Xdoc.CreateNode(XmlNodeType.Element, "formHeight", null);
            xmlFormHeight.InnerText = formHeight.ToString();
            Node.AppendChild(xmlFormHeight);

            var xmlController1 = m_Xdoc.CreateNode(XmlNodeType.Element, "Controller1", null);
            xmlController1.InnerText = profilePath[0];
            Node.AppendChild(xmlController1);
            var xmlController2 = m_Xdoc.CreateNode(XmlNodeType.Element, "Controller2", null);
            xmlController2.InnerText = profilePath[1];
            Node.AppendChild(xmlController2);
            var xmlController3 = m_Xdoc.CreateNode(XmlNodeType.Element, "Controller3", null);
            xmlController3.InnerText = profilePath[2];
            Node.AppendChild(xmlController3);
            var xmlController4 = m_Xdoc.CreateNode(XmlNodeType.Element, "Controller4", null);
            xmlController4.InnerText = profilePath[3];
            Node.AppendChild(xmlController4);

            var xmlLastChecked = m_Xdoc.CreateNode(XmlNodeType.Element, "LastChecked", null);
            xmlLastChecked.InnerText = lastChecked.ToString();
            Node.AppendChild(xmlLastChecked);
            var xmlCheckWhen = m_Xdoc.CreateNode(XmlNodeType.Element, "CheckWhen", null);
            xmlCheckWhen.InnerText = CheckWhen.ToString();
            Node.AppendChild(xmlCheckWhen);
            var xmlNotifications = m_Xdoc.CreateNode(XmlNodeType.Element, "Notifications", null);
            xmlNotifications.InnerText = notifications.ToString();
            Node.AppendChild(xmlNotifications);
            var xmlDisconnectBT = m_Xdoc.CreateNode(XmlNodeType.Element, "DisconnectBTAtStop", null);
            xmlDisconnectBT.InnerText = disconnectBTAtStop.ToString();
            Node.AppendChild(xmlDisconnectBT);
            var xmlSwipeProfiles = m_Xdoc.CreateNode(XmlNodeType.Element, "SwipeProfiles", null);
            xmlSwipeProfiles.InnerText = swipeProfiles.ToString();
            Node.AppendChild(xmlSwipeProfiles);
            var xmlDS4Mapping = m_Xdoc.CreateNode(XmlNodeType.Element, "UseDS4ForMapping", null);
            xmlDS4Mapping.InnerText = ds4Mapping.ToString();
            Node.AppendChild(xmlDS4Mapping);
            var xmlQuickCharge = m_Xdoc.CreateNode(XmlNodeType.Element, "QuickCharge", null);
            xmlQuickCharge.InnerText = quickCharge.ToString();
            Node.AppendChild(xmlQuickCharge);
            var xmlFirstXinputPort = m_Xdoc.CreateNode(XmlNodeType.Element, "FirstXinputPort", null);
            xmlFirstXinputPort.InnerText = firstXinputPort.ToString();
            Node.AppendChild(xmlFirstXinputPort);
            var xmlCloseMini = m_Xdoc.CreateNode(XmlNodeType.Element, "CloseMinimizes", null);
            xmlCloseMini.InnerText = closeMini.ToString();
            Node.AppendChild(xmlCloseMini);
            var xmlDownloadLang = m_Xdoc.CreateNode(XmlNodeType.Element, "DownloadLang", null);
            xmlDownloadLang.InnerText = downloadLang.ToString();
            Node.AppendChild(xmlDownloadLang);
            var xmlFlashWhenLate = m_Xdoc.CreateNode(XmlNodeType.Element, "FlashWhenLate", null);
            xmlFlashWhenLate.InnerText = flashWhenLate.ToString();
            Node.AppendChild(xmlFlashWhenLate);
            var xmlFlashWhenLateAt = m_Xdoc.CreateNode(XmlNodeType.Element, "FlashWhenLateAt", null);
            xmlFlashWhenLateAt.InnerText = flashWhenLateAt.ToString();
            Node.AppendChild(xmlFlashWhenLateAt);
            var xmlWhiteIcon = m_Xdoc.CreateNode(XmlNodeType.Element, "WhiteIcon", null);
            xmlWhiteIcon.InnerText = useWhiteIcon.ToString();
            Node.AppendChild(xmlWhiteIcon);

            for (var i = 0; i < 4; i++)
            {
                var xmlCustomLed = m_Xdoc.CreateNode(XmlNodeType.Element, "CustomLed" + (1 + i), null);
                xmlCustomLed.InnerText = useCustomLeds[i] + ":" + m_CustomLeds[i].Red + "," + m_CustomLeds[i].Green + "," + m_CustomLeds[i].Blue;
                Node.AppendChild(xmlCustomLed);
            }
            /* XmlNode xmlCustomLed2 = m_Xdoc.CreateNode(XmlNodeType.Element, "CustomLed2", null); xmlCustomLed2.InnerText = profilePath[1]; Node.AppendChild(xmlCustomLed2);
             XmlNode xmlCustomLed3 = m_Xdoc.CreateNode(XmlNodeType.Element, "CustomLed3", null); xmlCustomLed3.InnerText = profilePath[2]; Node.AppendChild(xmlCustomLed3);
             XmlNode xmlCustomLed4 = m_Xdoc.CreateNode(XmlNodeType.Element, "CustomLed4", null); xmlCustomLed4.InnerText = profilePath[3]; Node.AppendChild(xmlCustomLed4);*/

            m_Xdoc.AppendChild(Node);

            try
            {
                m_Xdoc.Save(m_Profile);
            }
            catch (UnauthorizedAccessException)
            {
                Saved = false;
            }
            return Saved;
        }


        private void CreateAction()
        {
            var m_Xdoc = new XmlDocument();
            XmlNode Node;

            Node = m_Xdoc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
            m_Xdoc.AppendChild(Node);

            Node = m_Xdoc.CreateComment($" Special Actions Configuration Data. {DateTime.Now} ");
            m_Xdoc.AppendChild(Node);

            Node = m_Xdoc.CreateWhitespace("\r\n");
            m_Xdoc.AppendChild(Node);

            Node = m_Xdoc.CreateNode(XmlNodeType.Element, "Actions", "");
            m_Xdoc.AppendChild(Node);
            m_Xdoc.Save(m_Actions);
        }

        public bool SaveAction(string name, string controls, int mode, string details, bool edit, string extras = "")
        {
            var saved = true;
            if (!File.Exists(m_Actions))
                CreateAction();
            m_Xdoc.Load(m_Actions);
            XmlNode Node;

            Node = m_Xdoc.CreateComment($" Special Actions Configuration Data. {DateTime.Now} ");
            foreach (XmlNode node in m_Xdoc.SelectNodes("//comment()"))
                node.ParentNode.ReplaceChild(Node, node);

            Node = m_Xdoc.SelectSingleNode("Actions");
            var el = m_Xdoc.CreateElement("Action");
            el.SetAttribute("Name", name);
            el.AppendChild(m_Xdoc.CreateElement("Trigger")).InnerText = controls;
            switch (mode)
            {
                case 1:
                    el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "Macro";
                    el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details;
                    if (extras != string.Empty)
                        el.AppendChild(m_Xdoc.CreateElement("Extras")).InnerText = extras;
                    break;
                case 2:
                    el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "Program";
                    el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details.Split('?')[0];
                    el.AppendChild(m_Xdoc.CreateElement("Arguements")).InnerText = extras;
                    el.AppendChild(m_Xdoc.CreateElement("Delay")).InnerText = details.Split('?')[1];
                    break;
                case 3:
                    el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "Profile";
                    el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details;
                    el.AppendChild(m_Xdoc.CreateElement("UnloadTrigger")).InnerText = extras;
                    break;
                case 4:
                    el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "Key";
                    el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details;
                    if (!string.IsNullOrEmpty(extras))
                    {
                        var exts = extras.Split('\n');
                        el.AppendChild(m_Xdoc.CreateElement("UnloadTrigger")).InnerText = exts[1];
                        el.AppendChild(m_Xdoc.CreateElement("UnloadStyle")).InnerText = exts[0];
                    }
                    break;
                case 5:
                    el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "DisconnectBT";
                    el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details;
                    break;
                case 6:
                    el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "BatteryCheck";
                    el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details;
                    break;
                case 7:
                    el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "MultiAction";
                    el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details;
                    break;
            }
            if (edit)
            {
                var oldxmlprocess = m_Xdoc.SelectSingleNode("/Actions/Action[@Name=\"" + name + "\"]");
                Node.ReplaceChild(el, oldxmlprocess);
            }
            else
            {
                Node.AppendChild(el);
            }
            m_Xdoc.AppendChild(Node);
            try
            {
                m_Xdoc.Save(m_Actions);
            }
            catch
            {
                saved = false;
            }
            LoadActions();
            return saved;
        }

        public void RemoveAction(string name)
        {
            m_Xdoc.Load(m_Actions);
            var Node = m_Xdoc.SelectSingleNode("Actions");
            var Item = m_Xdoc.SelectSingleNode("/Actions/Action[@Name=\"" + name + "\"]");
            if (Item != null)
                Node.RemoveChild(Item);
            m_Xdoc.AppendChild(Node);
            m_Xdoc.Save(m_Actions);
            LoadActions();
        }

        public bool LoadActions()
        {
            var saved = true;
            if (!File.Exists(Global.appdatapath + "\\Actions.xml"))
            {
                SaveAction("Disconnect Controller", "PS/Options", 5, "0", false);
                saved = false;
            }
            try
            {
                actions.Clear();
                var doc = new XmlDocument();
                doc.Load(Global.appdatapath + "\\Actions.xml");
                var actionslist = doc.SelectNodes("Actions/Action");
                string name, controls, type, details, extras, extras2;
                Mapping.actionDone.Clear();
                foreach (XmlNode x in actionslist)
                {
                    name = x.Attributes["Name"].Value;
                    controls = x.ChildNodes[0].InnerText;
                    type = x.ChildNodes[1].InnerText;
                    details = x.ChildNodes[2].InnerText;
                    Mapping.actionDone.Add(new Mapping.ActionState());
                    if (type == "Profile")
                    {
                        extras = x.ChildNodes[3].InnerText;
                        actions.Add(new SpecialAction(name, controls, type, details, 0, extras));
                    }
                    else if (type == "Macro")
                    {
                        if (x.ChildNodes[3] != null) extras = x.ChildNodes[3].InnerText;
                        else extras = string.Empty;
                        actions.Add(new SpecialAction(name, controls, type, details, 0, extras));
                    }
                    else if (type == "Key")
                    {
                        if (x.ChildNodes[3] != null)
                        {
                            extras = x.ChildNodes[3].InnerText;
                            extras2 = x.ChildNodes[4].InnerText;
                        }
                        else
                        {
                            extras = string.Empty;
                            extras2 = string.Empty;
                        }
                        if (!string.IsNullOrEmpty(extras))
                            actions.Add(new SpecialAction(name, controls, type, details, 0, extras2 + '\n' + extras));
                        else
                            actions.Add(new SpecialAction(name, controls, type, details));
                    }
                    else if (type == "DisconnectBT")
                    {
                        double doub;
                        if (double.TryParse(details, out doub))
                            actions.Add(new SpecialAction(name, controls, type, "", doub));
                        else
                            actions.Add(new SpecialAction(name, controls, type, ""));
                    }
                    else if (type == "BatteryCheck")
                    {
                        double doub;
                        if (double.TryParse(details.Split('|')[0], out doub))
                            actions.Add(new SpecialAction(name, controls, type, details, doub));
                        else if (double.TryParse(details.Split(',')[0], out doub))
                            actions.Add(new SpecialAction(name, controls, type, details, doub));
                        else
                            actions.Add(new SpecialAction(name, controls, type, details));
                    }
                    else if (type == "Program")
                    {
                        double doub;
                        if (x.ChildNodes[3] != null)
                        {
                            extras = x.ChildNodes[3].InnerText;
                            if (double.TryParse(x.ChildNodes[4].InnerText, out doub))
                                actions.Add(new SpecialAction(name, controls, type, details, doub, extras));
                            else
                                actions.Add(new SpecialAction(name, controls, type, details, 0, extras));
                        }
                        else
                        {
                            actions.Add(new SpecialAction(name, controls, type, details));
                        }
                    }
                    else if (type == "XboxGameDVR" || type == "MultiAction")
                    {
                        actions.Add(new SpecialAction(name, controls, type, details));
                    }
                }
            }
            catch
            {
                saved = false;
            }
            return saved;
        }


        public void UpdateDS4CSetting(int deviceNum, string buttonName, bool shift, object action, string exts, DS4KeyType kt, int trigger = 0)
        {
            DS4Controls dc;
            if (buttonName.StartsWith("bn"))
                dc = getDS4ControlsByName(buttonName);
            else
                dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);
            foreach (var dcs in Ds4Settings[deviceNum])
                if (dcs.control == dc)
                {
                    dcs.UpdateSettings(shift, action, exts, kt, trigger);
                    break;
                }
        }

        public void UpdateDS4CExtra(int deviceNum, string buttonName, bool shift, string exts)
        {
            DS4Controls dc;
            if (buttonName.StartsWith("bn"))
                dc = getDS4ControlsByName(buttonName);
            else
                dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);
            foreach (var dcs in Ds4Settings[deviceNum])
                if (dcs.control == dc)
                {
                    if (shift)
                        dcs.shiftExtras = exts;
                    else
                        dcs.extras = exts;
                    break;
                }
        }

        private void UpdateDS4CKeyType(int deviceNum, string buttonName, bool shift, DS4KeyType keyType)
        {
            DS4Controls dc;
            if (buttonName.StartsWith("bn"))
                dc = getDS4ControlsByName(buttonName);
            else
                dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);
            foreach (var dcs in Ds4Settings[deviceNum])
                if (dcs.control == dc)
                {
                    if (shift)
                        dcs.shiftKeyType = keyType;
                    else
                        dcs.keyType = keyType;
                    break;
                }
        }

        public object GetDS4Action(int deviceNum, string buttonName, bool shift)
        {
            DS4Controls dc;
            if (buttonName.StartsWith("bn"))
                dc = getDS4ControlsByName(buttonName);
            else
                dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);
            foreach (var dcs in Ds4Settings[deviceNum])
                if (dcs.control == dc)
                {
                    if (shift)
                        return dcs.shiftAction;
                    else
                        return dcs.action;
                }
            return null;
        }

        public string GetDS4Extra(int deviceNum, string buttonName, bool shift)
        {
            DS4Controls dc;
            if (buttonName.StartsWith("bn"))
                dc = getDS4ControlsByName(buttonName);
            else
                dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);
            foreach (var dcs in Ds4Settings[deviceNum])
                if (dcs.control == dc)
                {
                    if (shift)
                        return dcs.shiftExtras;
                    else
                        return dcs.extras;
                }
            return null;
        }

        public DS4KeyType GetDS4KeyType(int deviceNum, string buttonName, bool shift)
        {
            DS4Controls dc;
            if (buttonName.StartsWith("bn"))
                dc = getDS4ControlsByName(buttonName);
            else
                dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);
            foreach (var dcs in Ds4Settings[deviceNum])
                if (dcs.control == dc)
                {
                    if (shift)
                        return dcs.shiftKeyType;
                    else
                        return dcs.keyType;
                }
            return DS4KeyType.None;
        }

        public int GetDS4STrigger(int deviceNum, string buttonName)
        {
            DS4Controls dc;
            if (buttonName.StartsWith("bn"))
                dc = getDS4ControlsByName(buttonName);
            else
                dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);
            foreach (var dcs in Ds4Settings[deviceNum])
                if (dcs.control == dc)
                    return dcs.shiftTrigger;
            return 0;
        }

        public ControlSettings getDS4CSetting(int deviceNum, string buttonName)
        {
            DS4Controls dc;
            if (buttonName.StartsWith("bn"))
                dc = getDS4ControlsByName(buttonName);
            else
                dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);
            foreach (var dcs in Ds4Settings[deviceNum])
                if (dcs.control == dc)
                    return dcs;
            return null;
        }

        public bool HasCustomActions(int deviceNum)
        {
            foreach (var dcs in Ds4Settings[deviceNum])
                if (dcs.action != null || dcs.shiftAction != null)
                    return true;
            return false;
        }


        public bool HasCustomExtras(int deviceNum)
        {
            foreach (var dcs in Ds4Settings[deviceNum])
                if (dcs.extras != null || dcs.shiftExtras != null)
                    return true;
            return false;
        }
    }
}