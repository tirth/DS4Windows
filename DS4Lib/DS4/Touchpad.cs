using System;

namespace DS4Lib.DS4
{
    public class TouchpadEventArgs : EventArgs
    {
        public readonly TouchReadings[] TouchReadings;
        public readonly DateTime timeStamp;
        public readonly bool touchButtonPressed;
        public TouchpadEventArgs(DateTime utcTimestamp, bool tButtonDown, TouchReadings t0, TouchReadings t1 = null)
        {
            if (t1 != null)
            {
                TouchReadings = new TouchReadings[2];
                TouchReadings[0] = t0;
                TouchReadings[1] = t1;
            }
            else if (t0 != null)
            {
                TouchReadings = new TouchReadings[1];
                TouchReadings[0] = t0;
            }
            touchButtonPressed = tButtonDown;
            timeStamp = utcTimestamp;
        }
    }

    public class TouchReadings
    {
        public readonly int hwX, hwY, deltaX, deltaY;
        public readonly byte touchID;
        public readonly TouchReadings PreviousTouchReadings;
        public TouchReadings(int X, int Y,  byte tID, TouchReadings prevTouchReadings = null)
        {
            hwX = X;
            hwY = Y;
            touchID = tID;
            PreviousTouchReadings = prevTouchReadings;
            if (PreviousTouchReadings != null)
            {
                deltaX = X - PreviousTouchReadings.hwX;
                deltaY = Y - PreviousTouchReadings.hwY;
            }
        }
    }

    public class Touchpad
    {
        public event EventHandler<TouchpadEventArgs> TouchesBegan; // finger one or two landed (or both, or one then two, or two then one; any touches[] count increase)
        public event EventHandler<TouchpadEventArgs> TouchesMoved; // deltaX/deltaY are set because one or both fingers were already down on a prior sensor reading
        public event EventHandler<TouchpadEventArgs> TouchesEnded; // all fingers lifted
        public event EventHandler<TouchpadEventArgs> TouchButtonDown; // touchpad pushed down until the button clicks
        public event EventHandler<TouchpadEventArgs> TouchButtonUp; // touchpad button released
        public event EventHandler<EventArgs> TouchUnchanged; // no status change for the touchpad itself... but other sensors may have changed, or you may just want to do some processing

        public static readonly int TOUCHPAD_DATA_OFFSET = 35;
        internal int lastTouchPadX1, lastTouchPadY1,
            lastTouchPadX2, lastTouchPadY2; // tracks 0, 1 or 2 touches; we maintain touch 1 and 2 separately
        internal bool lastTouchPadIsDown;
        internal bool lastIsActive1, lastIsActive2;
        internal byte lastTouchID1, lastTouchID2;
        internal byte[] previousPacket = new byte[8];

        // We check everything other than the not bothering with not-very-useful TouchPacketCounter.
        private bool PacketChanged(byte[] data, int touchPacketOffset)
        {
            var changed = false;
            for (var i = 0; i < previousPacket.Length; i++)
            {
                var oldValue = previousPacket[i];
                previousPacket[i] = data[i + TOUCHPAD_DATA_OFFSET + touchPacketOffset];
                if (previousPacket[i] != oldValue)
                    changed = true;
            }
            return changed;
        }

        public void handleTouchpad(byte[] data, State sensors, int touchPacketOffset = 0)
        {
            var touchPadIsDown = sensors.TouchButton;
            if (!PacketChanged(data, touchPacketOffset) && touchPadIsDown == lastTouchPadIsDown)
            {
                TouchUnchanged?.Invoke(this, EventArgs.Empty);
                return;
            }
            var touchID1 = (byte)(data[0 + TOUCHPAD_DATA_OFFSET + touchPacketOffset] & 0x7F);
            var touchID2 = (byte)(data[4 + TOUCHPAD_DATA_OFFSET + touchPacketOffset] & 0x7F);
            var currentX1 = data[1 + TOUCHPAD_DATA_OFFSET + touchPacketOffset] + (data[2 + TOUCHPAD_DATA_OFFSET + touchPacketOffset] & 0xF) * 255;
            var currentY1 = ((data[2 + TOUCHPAD_DATA_OFFSET + touchPacketOffset] & 0xF0) >> 4) + data[3 + TOUCHPAD_DATA_OFFSET + touchPacketOffset] * 16;
            var currentX2 = data[5 + TOUCHPAD_DATA_OFFSET + touchPacketOffset] + (data[6 + TOUCHPAD_DATA_OFFSET + touchPacketOffset] & 0xF) * 255;
            var currentY2 = ((data[6 + TOUCHPAD_DATA_OFFSET + touchPacketOffset] & 0xF0) >> 4) + data[7 + TOUCHPAD_DATA_OFFSET + touchPacketOffset] * 16;

            TouchpadEventArgs args;
            if (sensors.Touch1 || sensors.Touch2)
            {
                if (sensors.Touch1 && !lastIsActive1 || sensors.Touch2 && !lastIsActive2)
                {
                    if (TouchesBegan != null)
                    {
                        if (sensors.Touch1 && sensors.Touch2)
                            args = new TouchpadEventArgs(sensors.ReportTimeStamp, sensors.TouchButton, new TouchReadings(currentX1, currentY1, touchID1), new TouchReadings(currentX2, currentY2, touchID2));
                        else if (sensors.Touch1)
                            args = new TouchpadEventArgs(sensors.ReportTimeStamp, sensors.TouchButton, new TouchReadings(currentX1, currentY1, touchID1));
                        else
                            args = new TouchpadEventArgs(sensors.ReportTimeStamp, sensors.TouchButton, new TouchReadings(currentX2, currentY2, touchID2));

                        TouchesBegan(this, args);
                    }
                }
                else if (sensors.Touch1 == lastIsActive1 && sensors.Touch2 == lastIsActive2 && TouchesMoved != null)
                {
                    TouchReadings tPrev, t0, t1;

                    if (sensors.Touch1 && sensors.Touch2)
                    {
                        tPrev = new TouchReadings(lastTouchPadX1, lastTouchPadY1, lastTouchID1);
                        t0 = new TouchReadings(currentX1, currentY1, touchID1, tPrev);
                        tPrev = new TouchReadings(lastTouchPadX2, lastTouchPadY2, lastTouchID2);
                        t1 = new TouchReadings(currentX2, currentY2, touchID2, tPrev);
                    }
                    else if (sensors.Touch1)
                    {
                        tPrev = new TouchReadings(lastTouchPadX1, lastTouchPadY1, lastTouchID1);
                        t0 = new TouchReadings(currentX1, currentY1, touchID1, tPrev);
                        t1 = null;
                    }
                    else
                    {
                        tPrev = new TouchReadings(lastTouchPadX2, lastTouchPadY2, lastTouchID2);
                        t0 = new TouchReadings(currentX2, currentY2, touchID2, tPrev);
                        t1 = null;
                    }
                    args = new TouchpadEventArgs(sensors.ReportTimeStamp, sensors.TouchButton, t0, t1);

                    TouchesMoved(this, args);
                }

                if (!lastTouchPadIsDown && touchPadIsDown && TouchButtonDown != null)
                {
                    if (sensors.Touch1 && sensors.Touch2)
                        args = new TouchpadEventArgs(sensors.ReportTimeStamp, sensors.TouchButton, new TouchReadings(currentX1, currentY1, touchID1), new TouchReadings(currentX2, currentY2, touchID2));
                    else if (sensors.Touch1)
                        args = new TouchpadEventArgs(sensors.ReportTimeStamp, sensors.TouchButton, new TouchReadings(currentX1, currentY1, touchID1));
                    else
                        args = new TouchpadEventArgs(sensors.ReportTimeStamp, sensors.TouchButton, new TouchReadings(currentX2, currentY2, touchID2));

                    TouchButtonDown(this, args);
                }
                else if (lastTouchPadIsDown && !touchPadIsDown && TouchButtonUp != null)
                {
                    if (sensors.Touch1 && sensors.Touch2)
                        args = new TouchpadEventArgs(sensors.ReportTimeStamp, sensors.TouchButton, new TouchReadings(currentX1, currentY1, touchID1), new TouchReadings(currentX2, currentY2, touchID2));
                    else if (sensors.Touch1)
                        args = new TouchpadEventArgs(sensors.ReportTimeStamp, sensors.TouchButton, new TouchReadings(currentX1, currentY1, touchID1));
                    else
                        args = new TouchpadEventArgs(sensors.ReportTimeStamp, sensors.TouchButton, new TouchReadings(currentX2, currentY2, touchID2));

                    TouchButtonUp(this, args);
                }

                if (sensors.Touch1)
                {
                    lastTouchPadX1 = currentX1;
                    lastTouchPadY1 = currentY1;
                }
                if (sensors.Touch2)
                {
                    lastTouchPadX2 = currentX2;
                    lastTouchPadY2 = currentY2;
                }
                lastTouchPadIsDown = touchPadIsDown;
            }
            else
            {
                if (touchPadIsDown && !lastTouchPadIsDown)
                    TouchButtonDown?.Invoke(this, new TouchpadEventArgs(sensors.ReportTimeStamp, sensors.TouchButton, null));
                else if (!touchPadIsDown && lastTouchPadIsDown)
                    TouchButtonUp?.Invoke(this, new TouchpadEventArgs(sensors.ReportTimeStamp, sensors.TouchButton, null));

                if ((lastIsActive1 || lastIsActive2) && TouchesEnded != null)
                {
                    if (lastIsActive1 && lastIsActive2)
                        args = new TouchpadEventArgs(sensors.ReportTimeStamp, sensors.TouchButton, new TouchReadings(lastTouchPadX1, lastTouchPadY1, touchID1), new TouchReadings(lastTouchPadX2, lastTouchPadY2, touchID2));
                    else if (lastIsActive1)
                        args = new TouchpadEventArgs(sensors.ReportTimeStamp, sensors.TouchButton, new TouchReadings(lastTouchPadX1, lastTouchPadY1, touchID1));
                    else
                        args = new TouchpadEventArgs(sensors.ReportTimeStamp, sensors.TouchButton, new TouchReadings(lastTouchPadX2, lastTouchPadY2, touchID2));

                    TouchesEnded(this, args);
                }
            }

            lastIsActive1 = sensors.Touch1;
            lastIsActive2 = sensors.Touch2;
            lastTouchID1 = touchID1;
            lastTouchID2 = touchID2;
            lastTouchPadIsDown = touchPadIsDown;
        }
    }
}
