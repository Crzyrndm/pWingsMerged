using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProceduralWings
{
    public static class Utils
    {
        public const double Deg2Rad = Math.PI / 180.0;
        public const double Rad2Deg = 180.0 / Math.PI;
        public static T Clamp<T>(T val, T min, T max) where T : IComparable
        {
            if (val.CompareTo(min) < 0) // val less than min
                return min;
            if (val.CompareTo(max) > 0) // val greater than max
                return max;
            return val;
        }
    }
}
