using System;
using System.ComponentModel;
using DS4Lib.DS4;

namespace DS4Windows
{
    public partial class X360Device : ScpDevice
    {
        private const string DS3_BUS_CLASS_GUID = "{F679F562-3164-42CE-A4DB-E7DDBE723909}";

        // Device 0 is the virtual USB hub itself, and we leave devices 1-10 available for other software (like the Scarlet.Crush DualShock driver itself)
        private const int CONTROLLER_OFFSET = 1;

        private int _firstController = 1;
        public int FirstController
        {
            get { return _firstController; }
            set { _firstController = value > 0 ? value : 1; }
        }

        protected int Scale(int value, bool flip)
        {
            value -= 0x80;

            if (value == -128)
                value = -127;

            if (flip)
                value *= -1;

            return (int)(value * 258.00787401574803149606299212599f);
        }
        
        public X360Device() : base(DS3_BUS_CLASS_GUID)
        {
            InitializeComponent();
        }

        public X360Device(IContainer container) : base(DS3_BUS_CLASS_GUID)
        {
            container.Add(this);

            InitializeComponent();
        }


        /* public override Boolean Open(int Instance = 0)
        {
            if (base.Open(Instance))
            {
            }

            return true;
        } */

        public override bool Open(string DevicePath)
        {
            m_Path = DevicePath;
            m_WinUsbHandle = (IntPtr)INVALID_HANDLE_VALUE;

            if (GetDeviceHandle(m_Path))
                m_IsActive = true;

            return true;
        }

        public override bool Start()
        {
            if (IsActive)
            {
            }

            return true;
        }

        public override bool Stop()
        {
            if (IsActive)
            {
                //Unplug(0);
            }

            return base.Stop();
        }

        public override bool Close()
        {
            if (IsActive)
            {
                Unplug(0);
            }

            return base.Close();
        }


        public void Parse(State state, byte[] Output, int device)
        {
            Output[0] = 0x1C;
            Output[4] = (byte)(device + _firstController);
            Output[9] = 0x14;

            for (var i = 10; i < Output.Length; i++)
            {
                Output[i] = 0;
            }
            if (state.Share) Output[10] |= (byte)(1 << 5); // Back
            if (state.L3) Output[10] |= (byte)(1 << 6); // Left  Thumb
            if (state.R3) Output[10] |= (byte)(1 << 7); // Right Thumb
            if (state.Options) Output[10] |= (byte)(1 << 4); // Start

            if (state.DpadUp) Output[10] |= (byte)(1 << 0); // Up
            if (state.DpadRight) Output[10] |= (byte)(1 << 3); // Down
            if (state.DpadDown) Output[10] |= (byte)(1 << 1); // Right
            if (state.DpadLeft) Output[10] |= (byte)(1 << 2); // Left

            if (state.L1) Output[11] |= (byte)(1 << 0); // Left  Shoulder
            if (state.R1) Output[11] |= (byte)(1 << 1); // Right Shoulder

            if (state.Triangle) Output[11] |= (byte)(1 << 7); // Y
            if (state.Circle) Output[11] |= (byte)(1 << 5); // B
            if (state.Cross) Output[11] |= (byte)(1 << 4); // A
            if (state.Square) Output[11] |= (byte)(1 << 6); // X

            if (state.PS) Output[11] |= (byte)(1 << 2); // Guide     

            Output[12] = state.L2; // Left Trigger
            Output[13] = state.R2; // Right Trigger

            var ThumbLX = Scale(state.LX, false);
            var ThumbLY = -Scale(state.LY, false);
            var ThumbRX = Scale(state.RX, false);
            var ThumbRY = -Scale(state.RY, false);
            Output[14] = (byte)((ThumbLX >> 0) & 0xFF); // LX
            Output[15] = (byte)((ThumbLX >> 8) & 0xFF);
            Output[16] = (byte)((ThumbLY >> 0) & 0xFF); // LY
            Output[17] = (byte)((ThumbLY >> 8) & 0xFF);
            Output[18] = (byte)((ThumbRX >> 0) & 0xFF); // RX
            Output[19] = (byte)((ThumbRX >> 8) & 0xFF);
            Output[20] = (byte)((ThumbRY >> 0) & 0xFF); // RY
            Output[21] = (byte)((ThumbRY >> 8) & 0xFF);
        }

        public bool Plugin(int Serial)
        {
            if (IsActive)
            {
                var Transfered = 0;
                var Buffer = new byte[16];

                Buffer[0] = 0x10;
                Buffer[1] = 0x00;
                Buffer[2] = 0x00;
                Buffer[3] = 0x00;

                Serial += _firstController;
                Buffer[4] = (byte)((Serial >> 0) & 0xFF);
                Buffer[5] = (byte)((Serial >> 8) & 0xFF);
                Buffer[6] = (byte)((Serial >> 16) & 0xFF);
                Buffer[7] = (byte)((Serial >> 24) & 0xFF);

                return DeviceIoControl(m_FileHandle, 0x2A4000, Buffer, Buffer.Length, null, 0, ref Transfered, IntPtr.Zero);
            }

            return false;
        }

        public bool Unplug(int Serial)
        {
            if (IsActive)
            {
                var Transfered = 0;
                var Buffer = new byte[16];

                Buffer[0] = 0x10;
                Buffer[1] = 0x00;
                Buffer[2] = 0x00;
                Buffer[3] = 0x00;

                Serial += _firstController;
                Buffer[4] = (byte)((Serial >> 0) & 0xFF);
                Buffer[5] = (byte)((Serial >> 8) & 0xFF);
                Buffer[6] = (byte)((Serial >> 16) & 0xFF);
                Buffer[7] = (byte)((Serial >> 24) & 0xFF);

                return DeviceIoControl(m_FileHandle, 0x2A4004, Buffer, Buffer.Length, null, 0, ref Transfered, IntPtr.Zero);
            }

            return false;
        }

        public bool UnplugAll() //not yet implemented, not sure if will
        {
            if (IsActive)
            {
                var Transfered = 0;
                var Buffer = new byte[16];

                Buffer[0] = 0x10;
                Buffer[1] = 0x00;
                Buffer[2] = 0x00;
                Buffer[3] = 0x00;

                return DeviceIoControl(m_FileHandle, 0x2A4004, Buffer, Buffer.Length, null, 0, ref Transfered, IntPtr.Zero);
            }

            return false;
        }


        public bool Report(byte[] Input, byte[] Output)
        {
            if (IsActive)
            {
                var Transfered = 0;

                return DeviceIoControl(m_FileHandle, 0x2A400C, Input, Input.Length, Output, Output.Length, ref Transfered, IntPtr.Zero) &&
                       Transfered > 0;
            }

            return false;
        }
    }
}