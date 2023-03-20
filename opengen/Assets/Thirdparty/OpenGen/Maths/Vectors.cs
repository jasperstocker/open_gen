using System;
using opengen.types;
using UnityEngine;



namespace opengen.maths
{
    public static class Vectors
    {
        public static float SignAngle(Vector2 from, Vector2 to)
        {
            Vector2 dir = (to - @from).normalized;
            float angle = Vector2.Angle(Vector2.up, dir);
            Vector3 dirV3 = new(dir.x, dir.y, 0);
            Vector3 cross = Vector3.Cross(Vector3.up, dirV3);
            if(cross.z > 0)
            {
                angle = -angle;
            }

            return angle;
        }

        public static float SignAngle(Vector2 dir)
        {
            float angle = Vector2.Angle(Vector2.up, dir);
            Vector3 dirV3 = new(dir.x, dir.y, 0);
            Vector3 cross = Vector3.Cross(Vector3.up, dirV3);
            if(cross.z > 0)
            {
                angle = -angle;
            }

            return angle;
        }

        public static float SignAngle(Vector2Fixed dir)
        {
            float angle = Vector2Fixed.Angle(Vector2Fixed.up, dir);
            Vector3 cross = Vector3.Cross(Vector3.up, dir.vector3XY);
            if(cross.z > 0)
            {
                angle = -angle;
            }

            return angle;
        }

        public static float SignAngleDirection(Vector2 dirForward, Vector2 dirAngle)
        {
            float angle = Vector2.Angle(dirForward, dirAngle);
            Vector2 cross = Rotate(dirForward, 90);
            float crossDot = Vector2.Dot(cross, dirAngle);
            if(crossDot < 0)
            {
                angle = -angle;
            }

            return angle;
        }

        public static bool Compare(Vector2 a, Vector2 b, float accuracy = 0.001f)
        {
            return (b - a).sqrMagnitude < accuracy;
        }

        public static Vector3 ToV3(Vector2 input)
        {
            return new Vector3(input.x, 0, input.y);
        }

        public static Vector2 ToV2(Vector3 input)
        {
            return new Vector2(input.x, input.z);
        }

        public static Vector3[] ToV3(Vector2[] input)
        {
            int inputLength = input.Length;
            Vector3[] output = new Vector3[inputLength];
            for(int i = 0; i < inputLength; i++)
            {
                output[i] = new Vector3(input[i].x, 0, input[i].y);
            }

            return output;
        }

        public static Vector2[] ToV2(Vector3[] input)
        {
            int inputLength = input.Length;
            Vector2[] output = new Vector2[inputLength];
            for(int i = 0; i < inputLength; i++)
            {
                output[i] = new Vector2(input[i].x, input[i].z);
            }

            return output;
        }

        public static Vector2 Rotate(Vector2 input, float degrees)
        {
            float sin = Mathf.Sin(degrees * Numbers.Deg2Rad);
            float cos = Mathf.Cos(degrees * Numbers.Deg2Rad);
            float tx = input.x;
            float ty = input.y;
            input.x = (cos * tx) - (sin * ty);
            input.y = (sin * tx) + (cos * ty);
            return input;
        }

        public static Vector2 Rotate90Clockwise(Vector2 input)
        {
            return new Vector2(-input.y, input.x);
        }

        public static Vector2 Rotate90AntiClockwise(Vector2 input)
        {
            return new Vector2(input.y, -input.x);
        }

        public static Vector2 RotateTowards(Vector2 from, Vector2 to, float maxDegrees)
        {
            float angleFrom = Vector2.Angle(Vector2.up, @from);
            float angleTo = Vector2.Angle(Vector2.up, to);
            float deltaAngle = Angles.DeltaAngle(angleFrom, angleTo);
            deltaAngle = Mathf.Min(deltaAngle, maxDegrees);
            return Rotate(@from, deltaAngle);
        }

        public static Vector2 ClampLerp(Vector2 from, Vector2 to, float maxDegrees, float lerp)
        {
            float angleFrom = Vector2.Angle(Vector2.up, @from);
            float angleTo = Vector2.Angle(Vector2.up, to);
            float deltaAngle = Angles.DeltaAngle(angleFrom, angleTo);
            deltaAngle = Mathf.Min(deltaAngle, maxDegrees);
            Vector2 useTo = Rotate(@from, deltaAngle);
            return Vector2.Lerp(@from, useTo, lerp);
        }

        public static float Cross(Vector2 a, Vector2 b, Vector2 c)
        {
            float x1 = b.x - a.x;
            float y1 = b.y - a.y;
            float x2 = c.x - b.x;
            float y2 = c.y - b.y;
            return x1 * y2 - x2 * y1;
        }

        public static float Cross(Vector2 a, Vector2 b)
        {
            return a.x * b.y - b.x * a.y;
        }

        /// <summary>
        /// Calcaulte the Tangent from a direction
        /// </summary>
        /// <param name="tangentDirection">the normalized right direction of the tangent</param>
        public static Vector4 CalculateTangent(Vector3 tangentDirection)
        {
            Vector4 tangent = new();
            tangent.x = tangentDirection.x;
            tangent.y = tangentDirection.y;
            tangent.z = tangentDirection.z;
            tangent.w = 1;//TODO: Check whether we need to flip the bi normal - I don't think we do with these planes
            return tangent;
        }

        /// <summary>
        /// Calculate the normal of a triangle
        /// </summary>
        /// <param name="points">Only three points will be used in calculation</param>
        public static Vector3 CalculateNormal(Vector3[] points)
        {
            if(points.Length < 3)
            {
                return Vector3.down;//most likely to look wrong
            }

            return CalculateNormal(points[0], points[1], points[2]);
        }

        /// <summary>
        /// Calculate the normal of a triangle
        /// </summary>
        public static Vector3 CalculateNormal(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            return Vector3.Cross((p1 - p0).normalized, (p2 - p0).normalized).normalized;
        }

        // public static Vector3 ProjectVectorOnPlane(Vector3 point, Vector3 normal)
        // {
        //     Vector3 offset = Vector3.Project(point, normal);
        //     return point - offset;
        // }

        public static Vector2 Project(Vector2 vector, Vector2 onNormal)
        {
            float num = Vector2.Dot(onNormal, onNormal);
            if(num < Numbers.Epsilon)
            {
                return Vector2.zero;
            }

            return onNormal * Vector2.Dot(vector, onNormal) / num;
        }

        public static Vector2 Normal(Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 dirA = (a - b).normalized;
            Vector2 dirB = (b - c).normalized;
            Vector2 croA = Rotate(dirA, 90);
            Vector2 croB = Rotate(dirB, 90);
            return (croA + croB).normalized;
        }
    }
}