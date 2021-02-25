using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Raycasting_Engine_CSharp
{
    public static class ColorUtilities
    {
        public static Color Add(Color color, int change)
        {
            int rVal = Convert.ToInt32(MathUtilities.Clamp(color.R + change, 0, 255));
            int gVal = Convert.ToInt32(MathUtilities.Clamp(color.G + change, 0, 255));
            int bVal = Convert.ToInt32(MathUtilities.Clamp(color.B + change, 0, 255));
            return Color.FromArgb(rVal, gVal, bVal);
        }
        public static Color Multiply(Color color, double change)
        {
            int rVal = Convert.ToInt32(MathUtilities.Clamp(color.R * change, 0, 255));
            int gVal = Convert.ToInt32(MathUtilities.Clamp(color.G * change, 0, 255));
            int bVal = Convert.ToInt32(MathUtilities.Clamp(color.B * change, 0, 255));
            return Color.FromArgb(rVal, gVal, bVal);
        }
        public static Color Negative(Color color)
        {
            int rVal = Convert.ToInt32(MathUtilities.Clamp(255 - (color.R * (color.R / 63.75)), 0, 255));
            int gVal = Convert.ToInt32(MathUtilities.Clamp(255 - (color.G * (color.G / 63.75)), 0, 255));
            int bVal = Convert.ToInt32(MathUtilities.Clamp(255 - (color.B * (color.B / 63.75)), 0, 255));
            return Color.FromArgb(rVal, gVal, bVal);
        }
        public static Color Desaturate(Color color, double saturation)
        {
            saturation = MathUtilities.Clamp(saturation, 0, 1);
            int average = (color.R + color.G + color.B) / 3;
            int rVal = Convert.ToInt32(MathUtilities.Clamp(MathUtilities.Lerp(color.R, average, 1 - saturation), 0, 255));
            int gVal = Convert.ToInt32(MathUtilities.Clamp(MathUtilities.Lerp(color.G, average, 1 - saturation), 0, 255));
            int bVal = Convert.ToInt32(MathUtilities.Clamp(MathUtilities.Lerp(color.B, average, 1 - saturation), 0, 255));
            return Color.FromArgb(rVal, gVal, bVal);
        }
        public static Color Mix(Color frontColor, Color backColor, double mixture)
        {
            mixture = MathUtilities.Clamp(mixture, 0, 1);
            int rVal = Convert.ToInt32(MathUtilities.Clamp(MathUtilities.Lerp(frontColor.R, backColor.R, mixture), 0, 255));
            int gVal = Convert.ToInt32(MathUtilities.Clamp(MathUtilities.Lerp(frontColor.G, backColor.G, mixture), 0, 255));
            int bVal = Convert.ToInt32(MathUtilities.Clamp(MathUtilities.Lerp(frontColor.B, backColor.B, mixture), 0, 255));
            return Color.FromArgb(rVal, gVal, bVal);
        }
    }
}
