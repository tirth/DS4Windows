namespace DS4Lib.DS4
{
    public class StateExposed
    {
        private readonly State _state;
        private byte[] _accel = { 0, 0, 0, 0, 0, 0 },
                        _gyro = { 0, 0, 0, 0, 0, 0 };

        public StateExposed()
        {
            _state = new State();
        }

        public StateExposed(State state)
        {
            _state = state;
        }

        bool Square => _state.Square;
        bool Triangle => _state.Triangle;
        bool Circle => _state.Circle;
        bool Cross => _state.Cross;
        bool DpadUp => _state.DpadUp;
        bool DpadDown => _state.DpadDown;
        bool DpadLeft => _state.DpadLeft;
        bool DpadRight => _state.DpadRight;
        bool L1 => _state.L1;
        bool L3 => _state.L3;
        bool R1 => _state.R1;
        bool R3 => _state.R3;
        bool Share => _state.Share;
        bool Options => _state.Options;
        bool PS => _state.PS;
        bool Touch1 => _state.Touch1;
        bool Touch2 => _state.Touch2;
        bool TouchButton => _state.TouchButton;
        byte LX => _state.LX;
        byte RX => _state.RX;
        byte LY => _state.LY;
        byte RY => _state.RY;
        byte L2 => _state.L2;
        byte R2 => _state.R2;
        int Battery => _state.Battery;

        /// <summary> Holds raw DS4 input data from 14 to 19 </summary>
        public byte[] Accel { set { _accel = value; } }
        /// <summary> Holds raw DS4 input data from 20 to 25 </summary>
        public byte[] Gyro { set { _gyro = value; } }

        /// <summary> Yaw leftward/counter-clockwise/turn to port or larboard side </summary>
        /// <remarks> Add double the previous result to this delta and divide by three.</remarks>
        public int AccelX => (short)((ushort)(_accel[2] << 8) | _accel[3]) / 256;

        /// <summary> Pitch upward/backward </summary>
        /// <remarks> Add double the previous result to this delta and divide by three.</remarks>
        public int AccelY => (short)((ushort)(_accel[0] << 8) | _accel[1] ) / 256;

        /// <summary> roll left/L side of controller down/starboard raising up </summary>
        /// <remarks> Add double the previous result to this delta and divide by three.</remarks>
        public int AccelZ => (short)((ushort)(_accel[4] << 8) | _accel[5]) / 256;

        /// <summary> R side of controller upward </summary>
        /// <remarks> Add double the previous result to this delta and divide by three.</remarks>
        public int GyroX => (short)((ushort)(_gyro[0] << 8) | _gyro[1]) / 64;

        /// <summary> touchpad and button face side of controller upward </summary>
        /// <remarks> Add double the previous result to this delta and divide by three.</remarks>
        public int GyroY => (short)((ushort)(_gyro[2] << 8) | _gyro[3]) / 64;

        /// <summary> Audio/expansion ports upward and light bar/shoulders/bumpers/USB port downward </summary>
        /// <remarks> Add double the previous result to this delta and divide by three.</remarks>
        public int GyroZ => (short)((ushort)(_gyro[4] << 8) | _gyro[5]) / 64;
    }
}
