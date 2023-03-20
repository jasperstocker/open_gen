using System;
using UnityEngine;

namespace opengen.maths
{
    public static class Numbers
    {
        public static float Infinity = float.PositiveInfinity;
        public static float NegativeInfinity = float.NegativeInfinity;// minfinity?
        public const float Deg2Rad = 0.01745329251994f; // Mathf.PI * 2f / 360f;
        public const float Rad2Deg = 57.29577951309314f; // 1f / Deg2Rad;
        public const float Epsilon = 1.401298E-45f;
        public const float kEpsilon = 0.00001f;
        public static int RoundToInt(float f)
        {
            return (int)Mathf.Round(f);
        }
        
        public static int FloorToInt(float f)
        {
            return (int)Mathf.Floor(f);
        }
        
        public static int CeilToInt(float f)
        {
            return (int)Mathf.Ceil(f);
        }
        
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }

            return value;
        }

        // Clamps value between min and max and returns value.
        // Set the position of the transform to be that of the time
        // but never less than 1 or more than 3
        //
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }

            return value;
        }

        // Clamps value between 0 and 1 and returns value
        public static float Clamp01(float value)
        {
            if (value < 0F)
            {
                return 0F;
            }
            else if (value > 1F)
            {
                return 1F;
            }
            else
            {
                return value;
            }
        }

        // Interpolates between /a/ and /b/ by /t/. /t/ is clamped between 0 and 1.
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Clamp01(t);
        }

        // Interpolates between /a/ and /b/ by /t/ without clamping the interpolant.
        public static float LerpUnclamped(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        // Interpolates between /min/ and /max/ with smoothing at the limits.
        public static float SmoothStep(float from, float to, float t)
        {
            t = Clamp01(t);
            t = -2.0F * t * t * t + 3.0F * t * t;
            return to * t + from * (1F - t);
        }
        
        /// <summary>
        /// Convert an index and a 2D width into X/Y coords
        /// </summary>
        public static int[] IndexCoords(int index, int width) 
        {
            int xIndex = RoundToInt((index % width));
            float yRaw = index / (float)width;
            int yIndex = RoundToInt(Math.Sign(yRaw) * Mathf.Floor(Math.Abs(yRaw)));
            return new[] { xIndex, yIndex };
        }

        private static bool SameSign(float a, float b) {
            return ((a * b) >= 0f);
        }

        public static bool Approximately(float a, float b, float epsilon = kEpsilon)
        {
            return Math.Abs(b - a) < Math.Max(epsilon * Math.Max(Math.Abs(a), Math.Abs(b)), epsilon);
        }

        public static int NextIndex(int currentIndex, int size)
        {
            return currentIndex < size - 1 ? currentIndex + 1 : 0;
        }

        public static int PreviousIndex(int currentIndex, int size)
        {
            return currentIndex > 0 ? currentIndex - 1 : size - 1;
        }

        public static float Sign(float value)
        {
            return (value < 0f) ? -1f : 1f;
        }

        public static int Sign(int value)
        {
            return (value < 0) ? -1 : 1;
        }

        public static bool IsAlmostZero(float value, float epsilon = kEpsilon)
        {
            return Math.Abs(value) <= epsilon;
        }
        
        public static float InvSqrt(float x)
        {
            float xhalf = 0.5f * x;
            int i = BitConverter.SingleToInt32Bits(x);
            i = 0x5f3759df - (i >> 1);
            x = BitConverter.Int32BitsToSingle(i);
            x *= (1.5f - xhalf * x * x);
            return x;
        }

        // Not seen a marked improvement in this implementation...
        // probably due to the bit conversions
        public static float FastSqrt(float x)
        {
            return 1f / InvSqrt(x);
        }
    }
}