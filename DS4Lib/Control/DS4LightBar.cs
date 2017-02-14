using System;
using System.Drawing;
using DS4Lib.DS4;
using static DS4Lib.Control.Global;

namespace DS4Lib.Control
{
    public class DS4LightBar
    {
        private static readonly byte[ /* Light On duration */, /* Light Off duration */] BatteryIndicatorDurations =
        {
            {0, 0}, // 0 is for "charging" OR anything sufficiently-"charged"
            {28, 252},
            {56, 224},
            {84, 196},
            {112, 168},
            {140, 140},
            {168, 112},
            {196, 84},
            {224, 56}, // on 80% of the time at 80, etc.
            {252, 28} // on 90% of the time at 90
        };

        static double[] counters = new double[4] {0, 0, 0, 0};
        public static double[] fadetimer = new double[4] {0, 0, 0, 0};
        static bool[] fadedirection = new bool[4] {false, false, false, false};
        static DateTime oldnow = DateTime.UtcNow;
        public static bool[] forcelight = new bool[4] {false, false, false, false};
        public static LightBarColour[] forcedColor = new LightBarColour[4];
        public static byte[] forcedFlash = new byte[4];

        public static void updateLightBar(Device device, int deviceNum, State cState, StateExposed eState, Mouse tp)
        {
            LightBarColour color;
            if (!defualtLight && !forcelight[deviceNum])
            {
                if (UseCustomLed[deviceNum])
                {
                    if (LedAsBatteryIndicator[deviceNum])
                    {
                        var fullColor = CustomColour[deviceNum];
                        var lowColor = LowColour[deviceNum];

                        color = getTransitionedColor(lowColor, fullColor, device.Battery);
                    }
                    else
                        color = CustomColour[deviceNum];
                }
                else
                {
                    if (Rainbow[deviceNum] > 0)
                    {
// Display rainbow
                        var now = DateTime.UtcNow;
                        if (now >= oldnow + TimeSpan.FromMilliseconds(10)) //update by the millisecond that way it's a smooth transtion
                        {
                            oldnow = now;
                            if (device.Charging)
                                counters[deviceNum] -= 1.5 * 3 / Rainbow[deviceNum];
                            else
                                counters[deviceNum] += 1.5 * 3 / Rainbow[deviceNum];
                        }
                        if (counters[deviceNum] < 0)
                            counters[deviceNum] = 180000;
                        if (counters[deviceNum] > 180000)
                            counters[deviceNum] = 0;
                        if (LedAsBatteryIndicator[deviceNum])
                            color = HuetoRGB((float)counters[deviceNum] % 360, (byte)(2.55 * device.Battery));
                        else
                            color = HuetoRGB((float)counters[deviceNum] % 360, 255);
                    }
                    else if (LedAsBatteryIndicator[deviceNum])
                    {
                        //if (device.Charging == false || device.Battery >= 100) // when charged, don't show the charging animation
                        {
                            var fullColor = MainColour[deviceNum];
                            var lowColor = LowColour[deviceNum];

                            color = getTransitionedColor(lowColor, fullColor, (uint)device.Battery);
                        }
                    }
                    else
                    {
                        color = MainColour[deviceNum];
                    }
                }

                if (device.Battery <= FlashAt[deviceNum] && !defualtLight && !device.Charging)
                {
                    if (!(FlashColour[deviceNum].Red == 0 &&
                          FlashColour[deviceNum].Green == 0 &&
                          FlashColour[deviceNum].Blue == 0))
                        color = FlashColour[deviceNum];
                    if (FlashType[deviceNum] == 1)
                    {
                        if (fadetimer[deviceNum] <= 0)
                            fadedirection[deviceNum] = true;
                        else if (fadetimer[deviceNum] >= 100)
                            fadedirection[deviceNum] = false;
                        if (fadedirection[deviceNum])
                            fadetimer[deviceNum] += 1;
                        else
                            fadetimer[deviceNum] -= 1;
                        color = getTransitionedColor(color, new LightBarColour(0, 0, 0), fadetimer[deviceNum]);
                    }
                }

                if (IdleDisconnectTimeout[deviceNum] > 0 && LedAsBatteryIndicator[deviceNum] && (!device.Charging || device.Battery >= 100))
                {
//Fade lightbar by idle time
                    var timeratio = new TimeSpan(DateTime.UtcNow.Ticks - device.LastActive.Ticks);
                    var botratio = timeratio.TotalMilliseconds;
                    var topratio = TimeSpan.FromSeconds(IdleDisconnectTimeout[deviceNum]).TotalMilliseconds;
                    var ratio = botratio / topratio * 100;
                    if (ratio >= 50 && ratio <= 100)
                        color = getTransitionedColor(color, new LightBarColour(0, 0, 0), (uint)((ratio - 50) * 2));
                    else if (ratio >= 100)
                        color = getTransitionedColor(color, new LightBarColour(0, 0, 0), 100);
                }
                if (device.Charging && device.Battery < 100)
                    switch (ChargingType[deviceNum])
                    {
                        case 1:
                            if (fadetimer[deviceNum] <= 0)
                                fadedirection[deviceNum] = true;
                            else if (fadetimer[deviceNum] >= 105)
                                fadedirection[deviceNum] = false;
                            if (fadedirection[deviceNum])
                                fadetimer[deviceNum] += .1;
                            else
                                fadetimer[deviceNum] -= .1;
                            color = getTransitionedColor(color, new LightBarColour(0, 0, 0), fadetimer[deviceNum]);
                            break;
                        case 2:
                            counters[deviceNum] += .167;
                            color = HuetoRGB((float)counters[deviceNum] % 360, 255);
                            break;
                        case 3:
                            color = ChargingColour[deviceNum];
                            break;
                        default:
                            break;
                    }
            }
            else if (forcelight[deviceNum])
            {
                color = forcedColor[deviceNum];
            }
            else if (shuttingdown)
                color = new LightBarColour(0, 0, 0);
            else
            {
                if (device.ConnectionType == ConnectionType.BT)
                    color = new LightBarColour(32, 64, 64);
                else
                    color = new LightBarColour(0, 0, 0);
            }
            var distanceprofile = ProfilePath[deviceNum].ToLower().Contains("distance") || tempprofilename[deviceNum].ToLower().Contains("distance");
            if (distanceprofile && !defualtLight)
            {
                //Thing I did for Distance
                var rumble = device.LeftHeavySlowRumble / 2.55f;
                var max = Math.Max(color.Red, Math.Max(color.Green, color.Blue));
                if (device.LeftHeavySlowRumble > 100)
                    color = getTransitionedColor(new LightBarColour(max, max, 0), new LightBarColour(255, 0, 0), rumble);
                else
                    color = getTransitionedColor(color, getTransitionedColor(new LightBarColour(max, max, 0), new LightBarColour(255, 0, 0), 39.6078f),
                        device.LeftHeavySlowRumble);
            }
            var haptics = new HapticState
            {
                LightBarColour = color
            };
            if (haptics.IsLightBarSet())
            {
                if (forcelight[deviceNum] && forcedFlash[deviceNum] > 0)
                {
                    haptics.LightBarFlashDurationOff = haptics.LightBarFlashDurationOn = (byte)(25 - forcedFlash[deviceNum]);
                    haptics.LightBarExplicitlyOff = true;
                }
                else if (device.Battery <= FlashAt[deviceNum] && FlashType[deviceNum] == 0 && !defualtLight && !device.Charging)
                {
                    var level = device.Battery / 10;
                    //if (level >= 10)
                    //level = 0; // all values of ~0% or >~100% are rendered the same
                    haptics.LightBarFlashDurationOn = BatteryIndicatorDurations[level, 0];
                    haptics.LightBarFlashDurationOff = BatteryIndicatorDurations[level, 1];
                }
                else if (distanceprofile && device.LeftHeavySlowRumble > 155) //also part of Distance
                {
                    haptics.LightBarFlashDurationOff = haptics.LightBarFlashDurationOn = (byte)(-device.LeftHeavySlowRumble + 265);
                    haptics.LightBarExplicitlyOff = true;
                }
                else
                {
                    //haptics.LightBarFlashDurationOff = haptics.LightBarFlashDurationOn = 1;
                    haptics.LightBarFlashDurationOff = haptics.LightBarFlashDurationOn = 0;
                    haptics.LightBarExplicitlyOff = true;
                }
            }
            else
            {
                haptics.LightBarExplicitlyOff = true;
            }
            if (device.LightBarOnDuration != haptics.LightBarFlashDurationOn && device.LightBarOnDuration != 1 && haptics.LightBarFlashDurationOn == 0)
                haptics.LightBarFlashDurationOff = haptics.LightBarFlashDurationOn = 1;
            if (device.LightBarOnDuration == 1) //helps better reset the color
                System.Threading.Thread.Sleep(5);
            device.pushHapticState(haptics);
        }

        public static bool defualtLight = false, shuttingdown = false;

        public static LightBarColour HuetoRGB(float hue, byte sat)
        {
            var C = sat;
            var X = (int)(C * (1 - Math.Abs(hue / 60 % 2 - 1)));
            if (0 <= hue && hue < 60)
                return new LightBarColour(C, (byte)X, 0);
            else if (60 <= hue && hue < 120)
                return new LightBarColour((byte)X, C, 0);
            else if (120 <= hue && hue < 180)
                return new LightBarColour(0, C, (byte)X);
            else if (180 <= hue && hue < 240)
                return new LightBarColour(0, (byte)X, C);
            else if (240 <= hue && hue < 300)
                return new LightBarColour((byte)X, 0, C);
            else if (300 <= hue && hue < 360)
                return new LightBarColour(C, 0, (byte)X);
            else
                return new LightBarColour(Color.Red);
        }
    }
}