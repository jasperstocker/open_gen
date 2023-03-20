﻿

using System;
using UnityEngine;

namespace opengen.maths
{
    public static class Circles
    {
        public static float Circumference(float radius)
        {
            return 2 * Mathf.PI * radius;
        }

        public static float Radius(float circumference)
        {
            return circumference / (2 * Mathf.PI);
        }

        /// <summary>
        /// calculate arc length
        /// </summary>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="centralAngle">angle of arc in radians</param>
        /// <returns></returns>
        public static float ArcLength(float radius, float centralAngle)
        {
            return radius * centralAngle;
        }

        /// <summary>
        /// Calculate the central angle
        /// </summary>
        /// <param name="chordLength">The length of the chord</param>
        /// <param name="radius">The ardius of the circle</param>
        /// <returns></returns>
        public static float CentralAngle(float chordLength, float radius)
        {
            return 2 * Mathf.Asin(chordLength / (2 * radius));
        }

        /// <summary>
        /// Calculate the central angle
        /// </summary>
        /// <param name="arcLength">The arc length</param>
        /// <param name="radius">The ardius of the circle</param>
        /// <returns></returns>
        public static float CentralAngleFromArcLength(float arcLength, float radius)
        {
            return Numbers.Rad2Deg * (arcLength / radius);
        }

        /// <summary>
        /// Calculate chord length
        /// </summary>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="angle">Angle in radians of the chord</param>
        /// <returns></returns>
        public static float ChordLength(float radius, float angle)
        {
            return 2 * radius * Mathf.Sin(angle / 2);
        }

        /// <summary>
        /// Calculate the segment height
        /// The height from the midpoint on the chord to the radius
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="angle">Angle in radians</param>
        /// <returns></returns>
        public static float SegmentHeight(float radius, float angle)
        {
            return radius * (1 - Mathf.Cos(angle / 2));
        }

        /// <summary>
        /// Calculate the height from the center to the chord midpoint
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="angle">Angle in radians</param>
        /// <returns></returns>
        public static float TriangluarHeight(float radius, float angle)
        {
            return radius - SegmentHeight(radius, angle);
        }
    }
}