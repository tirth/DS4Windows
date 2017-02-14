using System;
using System.Collections.Generic;

namespace DS4Lib.Control
{
    [Flags]
    public enum DS4KeyType : byte
    {
        None = 0,
        ScanCode = 1,
        Toggle = 2,
        Unbound = 4,
        Macro = 8,
        HoldMacro = 16,
        RepeatMacro = 32
    }; //Increment by exponents of 2*, starting at 2^0

    public enum Ds3PadId : byte
    {
        None = 0xFF,
        One = 0x00,
        Two = 0x01,
        Three = 0x02,
        Four = 0x03,
        All = 0x04
    };

    public enum DS4Controls : byte
    {
        None,
        LXNeg,
        LXPos,
        LYNeg,
        LYPos,
        RXNeg,
        RXPos,
        RYNeg,
        RYPos,
        L1,
        L2,
        L3,
        R1,
        R2,
        R3,
        Square,
        Triangle,
        Circle,
        Cross,
        DpadUp,
        DpadRight,
        DpadDown,
        DpadLeft,
        PS,
        TouchLeft,
        TouchUpper,
        TouchMulti,
        TouchRight,
        Share,
        Options,
        GyroXPos,
        GyroXNeg,
        GyroZPos,
        GyroZNeg,
        SwipeLeft,
        SwipeRight,
        SwipeUp,
        SwipeDown
    };

    public enum X360Controls : byte
    {
        None,
        LXNeg,
        LXPos,
        LYNeg,
        LYPos,
        RXNeg,
        RXPos,
        RYNeg,
        RYPos,
        LB,
        LT,
        LS,
        RB,
        RT,
        RS,
        X,
        Y,
        B,
        A,
        DpadUp,
        DpadRight,
        DpadDown,
        DpadLeft,
        Guide,
        Back,
        Start,
        LeftMouse,
        RightMouse,
        MiddleMouse,
        FourthMouse,
        FifthMouse,
        WUP,
        WDOWN,
        MouseUp,
        MouseDown,
        MouseLeft,
        MouseRight,
        Unbound
    };

    public class DebugEventArgs : EventArgs
    {
        protected DateTime m_Time = DateTime.Now;
        protected string m_Data = string.Empty;
        protected bool warning;

        public DebugEventArgs(string Data, bool warn)
        {
            m_Data = Data;
            warning = warn;
        }

        public DateTime Time => m_Time;
        public string Data => m_Data;
        public bool Warning => warning;
    }

    public class MappingDoneEventArgs : EventArgs
    {
        protected int deviceNum = -1;

        public MappingDoneEventArgs(int DeviceID)
        {
            deviceNum = DeviceID;
        }

        public int DeviceID => deviceNum;
    }

    public class ReportEventArgs : EventArgs
    {
        protected Ds3PadId m_Pad = Ds3PadId.None;
        protected byte[] m_Report = new byte[64];

        public ReportEventArgs()
        {
        }

        public ReportEventArgs(Ds3PadId Pad)
        {
            m_Pad = Pad;
        }

        public Ds3PadId Pad
        {
            get { return m_Pad; }
            set { m_Pad = value; }
        }

        public byte[] Report
        {
            get { return m_Report; }
        }
    }

    public class SpecialAction
    {
        public string name;
        public List<DS4Controls> trigger = new List<DS4Controls>();
        public string type;
        public string controls;
        public List<int> macro = new List<int>();
        public string details;
        public List<DS4Controls> uTrigger = new List<DS4Controls>();
        public string ucontrols;
        public double delayTime;
        public string extra;
        public bool pressRelease;
        public DS4KeyType keyType;

        public SpecialAction(string name, string controls, string type, string details, double delay = 0, string extras = "")
        {
            this.name = name;
            this.type = type;
            this.controls = controls;
            delayTime = delay;
            var ctrls = controls.Split('/');
            foreach (var s in ctrls)
                trigger.Add(getDS4ControlsByName(s));
            if (type == "Macro")
            {
                var macs = details.Split('/');
                foreach (var s in macs)
                {
                    int v;
                    if (int.TryParse(s, out v))
                        macro.Add(v);
                }
                if (extras.Contains("Scan Code"))
                    keyType |= DS4KeyType.ScanCode;
            }
            else if (type == "Key")
            {
                this.details = details.Split(' ')[0];
                if (!string.IsNullOrEmpty(extras))
                {
                    var exts = extras.Split('\n');
                    pressRelease = exts[0] == "Release";
                    ucontrols = exts[1];
                    var uctrls = exts[1].Split('/');
                    foreach (var s in uctrls)
                        uTrigger.Add(getDS4ControlsByName(s));
                }
                if (details.Contains("Scan Code"))
                    keyType |= DS4KeyType.ScanCode;
            }
            else if (type == "Program")
            {
                this.details = details;
                if (extras != string.Empty)
                    extra = extras;
            }
            else if (type == "XboxGameDVR")
            {
                var dets = details.Split(',');
                var macros = new List<string>();
                //string dets = "";
                var typeT = 0;
                for (var i = 0; i < 3; i++)
                {
                    if (int.TryParse(dets[i], out typeT))
                    {
                        switch (typeT)
                        {
                            case 0:
                                macros.Add("91/71/71/91");
                                break;
                            case 1:
                                macros.Add("91/164/82/82/164/91");
                                break;
                            case 2:
                                macros.Add("91/164/44/44/164/91");
                                break;
                            case 3:
                                macros.Add(dets[3] + "/" + dets[3]);
                                break;
                            case 4:
                                macros.Add("91/164/71/71/164/91");
                                break;
                        }
                    }
                }
                this.type = "MultiAction";
                type = "MultiAction";
                this.details = string.Join(",", macros);
            }
            else
                this.details = details;

            if (type != "Key" && !string.IsNullOrEmpty(extras))
            {
                ucontrols = extras;
                var uctrls = extras.Split('/');
                foreach (var s in uctrls)
                    uTrigger.Add(getDS4ControlsByName(s));
            }
        }

        private DS4Controls getDS4ControlsByName(string key)
        {
            switch (key)
            {
                case "Share":
                    return DS4Controls.Share;
                case "L3":
                    return DS4Controls.L3;
                case "R3":
                    return DS4Controls.R3;
                case "Options":
                    return DS4Controls.Options;
                case "Up":
                    return DS4Controls.DpadUp;
                case "Right":
                    return DS4Controls.DpadRight;
                case "Down":
                    return DS4Controls.DpadDown;
                case "Left":
                    return DS4Controls.DpadLeft;

                case "L1":
                    return DS4Controls.L1;
                case "R1":
                    return DS4Controls.R1;
                case "Triangle":
                    return DS4Controls.Triangle;
                case "Circle":
                    return DS4Controls.Circle;
                case "Cross":
                    return DS4Controls.Cross;
                case "Square":
                    return DS4Controls.Square;

                case "PS":
                    return DS4Controls.PS;
                case "Left Stick Left":
                    return DS4Controls.LXNeg;
                case "Left Stick Up":
                    return DS4Controls.LYNeg;
                case "Right Stick Left":
                    return DS4Controls.RXNeg;
                case "Right Stick Up":
                    return DS4Controls.RYNeg;

                case "Left Stick Right":
                    return DS4Controls.LXPos;
                case "Left Stick Down":
                    return DS4Controls.LYPos;
                case "Right Stick Right":
                    return DS4Controls.RXPos;
                case "Right Stick Down":
                    return DS4Controls.RYPos;
                case "L2":
                    return DS4Controls.L2;
                case "R2":
                    return DS4Controls.R2;

                case "Left Touch":
                    return DS4Controls.TouchLeft;
                case "Multitouch":
                    return DS4Controls.TouchMulti;
                case "Upper Touch":
                    return DS4Controls.TouchUpper;
                case "Right Touch":
                    return DS4Controls.TouchRight;

                case "Swipe Up":
                    return DS4Controls.SwipeUp;
                case "Swipe Down":
                    return DS4Controls.SwipeDown;
                case "Swipe Left":
                    return DS4Controls.SwipeLeft;
                case "Swipe Right":
                    return DS4Controls.SwipeRight;

                case "Tilt Up":
                    return DS4Controls.GyroZNeg;
                case "Tilt Down":
                    return DS4Controls.GyroZPos;
                case "Tilt Left":
                    return DS4Controls.GyroXPos;
                case "Tilt Right":
                    return DS4Controls.GyroXNeg;
            }
            return 0;
        }
    }
}