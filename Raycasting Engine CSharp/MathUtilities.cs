using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting_Engine_CSharp
{
    public static class MathUtilities
    {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if ((max.CompareTo(min) < 0) || (min.CompareTo(max) > 0))
            {
                T cache = min;
                min = max;
                max = cache;
            }
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }
        public static double Lerp(this double origin, double destination, double change)
        {
            return origin * (1 - change) + destination * change;
        }
    }
}
