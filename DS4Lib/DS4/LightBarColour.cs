using System;
using System.Drawing;

namespace DS4Lib.DS4
{
    public struct LightBarColour
    {
        public byte Red;
        public byte Green;
        public byte Blue;

        public LightBarColour(Color c)
        {
            Red = c.R;
            Green = c.G;
            Blue = c.B;
        }

        public LightBarColour(byte r, byte g, byte b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }

        public override bool Equals(object obj)
        {
            if (obj is LightBarColour dsc)
                return Red == dsc.Red && Green == dsc.Green && Blue == dsc.Blue;

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public Color ToColor => Color.FromArgb(Red, Green, Blue);

        public Color ToColorA
        {
            get
            {
                var alphacolor = Math.Max(Red, Math.Max(Green, Blue));
                var reg = Color.FromArgb(Red, Green, Blue);
                var full = HueToRgb(reg.GetHue(), reg.GetBrightness(), reg);
                return Color.FromArgb(alphacolor > 205 ? 255 : alphacolor + 50, full);
            }
        }

        private static Color HueToRgb(float hue, float light, Color rgb)
        {
            var L = (float)Math.Max(.5, light);
            var C = 1 - Math.Abs(2 * L - 1);
            var X = C * (1 - Math.Abs(hue / 60 % 2 - 1));
            var m = L - C / 2;

            float R = 0, G = 0, B = 0;

            if (light == 1)
                return Color.White;

            if (rgb.R == rgb.G && rgb.G == rgb.B)
                return Color.White;

            if (0 <= hue && hue < 60)
            {
                R = C;
                G = X;
            }
            else if (60 <= hue && hue < 120)
            {
                R = X;
                G = C;
            }
            else if (120 <= hue && hue < 180)
            {
                G = C;
                B = X;
            }
            else if (180 <= hue && hue < 240)
            {
                G = X;
                B = C;
            }
            else if (240 <= hue && hue < 300)
            {
                R = X;
                B = C;
            }
            else if (300 <= hue && hue < 360)
            {
                R = C;
                B = X;
            }
            return Color.FromArgb((int)((R + m) * 255), (int)((G + m) * 255), (int)((B + m) * 255));
        }

        public static bool TryParse(string value, ref LightBarColour lightBarColour)
        {
            try
            {
                var ss = value.Split(',');
                return byte.TryParse(ss[0], out lightBarColour.Red) && byte.TryParse(ss[1], out lightBarColour.Green) &&
                       byte.TryParse(ss[2], out lightBarColour.Blue);
            }
            catch
            {
                return false;
            }
        }

        public override string ToString() => $"Red: {Red} Green: {Green} Blue: {Blue}";
    }
}