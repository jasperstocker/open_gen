using System;
using opengen.maths;

namespace opengen.types
{
    public class FloatUtils
    {
        public static bool IsZero(float value)
        {
            return Math.Abs(value) < Numbers.Epsilon;
        }
    }
}