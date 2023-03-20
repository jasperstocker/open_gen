using System;
using opengen.maths;
using UnityEngine;

namespace opengen.types
{
    public struct Triangle3D
    {
        public Vector3 v0;
        public Vector3 v1;
        public Vector3 v2;

        public Vector3 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return v0;
                    case 1:
                        return v1;
                    case 2:
                        return v2;
                }

                throw new Exception("Triangle3D Index out of range");
            }
        }

        public Triangle3D(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
        }

        public Triangle3D(Vector3Fixed v0, Vector3Fixed v1, Vector3Fixed v2)
        {
            this.v0 = v0.vector3;
            this.v1 = v1.vector3;
            this.v2 = v2.vector3;
        }

        public Vector3 CalculateNormal()
        {
            return CalculateNormal(v0, v1, v2);
        }

        public Vector3 CalculateCenter()
        {
            return CalculateCenter(v0, v1, v2);
        }

        public bool CheckSize()
        {
            if ((v0 - v1).sqrMagnitude < Numbers.Epsilon)
            {
                return false;
            }

            if ((v0 - v2).sqrMagnitude < Numbers.Epsilon)
            {
                return false;
            }

            if ((v2 - v1).sqrMagnitude < Numbers.Epsilon)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calculate the normal of a triangle
        /// </summary>
        public static Vector3 CalculateCenter(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            return (v0 + v1 + v2) / 3f;
        }

        public static Vector3 CalculateNormal(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            return Vector3.Cross((v1 - v0).normalized, (v2 - v0).normalized).normalized;
        }

        public static float AreaHeron(float lengthA, float lengthB, float lengthC)
        {
            float s = (lengthA + lengthB + lengthC) * 0.5f;
            return Mathf.Sqrt(s * (s - lengthA) * (s - lengthB) * (s - lengthB));
        }

        /// <summary>
        /// SAS Formula
        /// Calculate the area of a triangle from two sides and an angle
        /// Side, Angle, Side
        /// </summary>
        public static float AreaSas(float lengthA, float angle, float lengthB)
        {
            return 0.5f * lengthA * lengthB * Mathf.Sin(angle * Numbers.Deg2Rad);
        }

        /// <summary>
        /// SAS Formula
        /// Calculate the opposite edge of a triangle from two sides and an angle
        /// Side, Angle, Side
        /// </summary>
        public static float FindOpposite(float lengthA, float angle, float lengthB)
        {
            return Mathf.Sqrt((lengthA * lengthA) + (lengthB * lengthB) -
                              2 * (lengthA * lengthB) * Mathf.Cos(angle * Numbers.Deg2Rad));
        }
    }
}