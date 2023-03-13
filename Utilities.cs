using Hkmp.Math;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptureTheFlag
{
    internal static class Utilities
    {
        /// <summary>
        /// Float to string
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string f2s(float x)
        {
           return x.ToString("0.00", CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// Vector2 to string
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static string v2s(Vector2 v)
        {
            return $"{f2s(v.X)}{Constants.Separator}{f2s(v.Y)}";
        }

        public static Vector2 s2v( string s)
        {
            var split = s.Split(Constants.SplitSep);
            return new Vector2(s2f(split[0]), s2f(split[1]));
        }

        public static float s2f(string v)
        {
            return float.Parse(v, CultureInfo.InvariantCulture);
        }
        public static int s2i(string s)
        {
            return int.Parse(s, CultureInfo.InvariantCulture);
        }

        public static string i2s(int i) { 
            return i.ToString(CultureInfo.InvariantCulture); 
        }
    }
}
