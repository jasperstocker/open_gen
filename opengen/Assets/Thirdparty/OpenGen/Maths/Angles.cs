using System;
using opengen.types;
using UnityEngine;

namespace opengen.maths
{
    public static class Angles
    {
        public static float ClampAngle360(float input)
        {
            float output = input % 360;
            if(output < 0)
            {
                output += 360;
            }

            return output;
        }

        public static float ClampAngle(float input)
        {
            float output = input % 360;
            if(output > 180)
            {
                output += -360;
            }

            if(output < -180)
            {
                output += 360;
            }

            return output;
        }
        
        public static float SignAngle(Vector2 from, Vector2 to) 
        {
            return SignAngle(to - from);
        }

        public static float SignAngle(Vector2 dir) 
        {
            Vector3 dirV3 = new Vector3(dir.x, dir.y, 0).normalized;
            float angle = Vector2.Angle(Vector2.up, dir);
            Vector3 cross = Vector3.Cross(Vector3.up, dirV3);
            if (cross.z > 0)
            {
                angle = -angle;
            }

            return angle;
        }
        
        public static float CalculateAngle(Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 ab = b - a;
            Vector2 cb = b - c;
            return Mathf.Acos(Vector2.Dot(ab, cb) / (ab.magnitude * cb.magnitude)) * Mathf.Rad2Deg;
        }

        public static float SignAngleNormalised(Vector2 dir) 
        {
            Vector3 dirV3 = new (dir.x, dir.y, 0);
            float angle = Vector2.Angle(Vector2.up, dir);
            Vector3 cross = Vector3.Cross(Vector3.up, dirV3);
            if (cross.z > 0)
            {
                angle = -angle;
            }

            return angle;
        }

        public static float SignAngleRelativeAntiClockwise(Vector2 normal, Vector2 vector)
        {
            float dot = normal.x * vector.x + normal.y * vector.y;//# dot product between [x1, y1] and [x2, y2]
            float det = normal.x * vector.y - normal.y * vector.x;//# determinant
            return Mathf.Atan2(det, dot) * Numbers.Rad2Deg;//  # atan2(y, x) or atan2(sin, cos)
        }

        public static float SignAngleRelativeClockwise(Vector2 normal, Vector2 vector)
        {
            float dot = vector.x * normal.x + vector.y * normal.y;//# dot product between [x1, y1] and [x2, y2]
            float det = vector.x * normal.y - vector.y * normal.x;//# determinant
            return Mathf.Atan2(det, dot) * Numbers.Rad2Deg;//  # atan2(y, x) or atan2(sin, cos)
        }

        public static float SignAngleRelative(Vector2 normal, Vector2 vector)
        {
            return SignAngleRelativeClockwise(normal, vector);
        }

        public static float SignAngleDirection(Vector2 dirForward, Vector2 dirAngle) 
        {
            float angle = Vector2.Angle(dirForward, dirAngle);
            Vector2 cross = Rotate(dirForward, 90);
            float crossDot = Vector2.Dot(cross, dirAngle);
            if (crossDot < 0)
            {
                angle = -angle;
            }

            return angle;
        }
        
        public static Vector2 Rotate(Vector2 input, float degrees) {
            float sin = -Mathf.Sin(degrees * Numbers.Deg2Rad);
            float cos = Mathf.Cos(degrees * Numbers.Deg2Rad);

            float tx = input.x;
            float ty = input.y;
            input.x = (cos * tx) - (sin * ty);
            input.y = (sin * tx) + (cos * ty);
            return input;
        }

        public static int[] SortPointByAngle(Vector2Fixed[] points, Vector2Fixed center)
        {
            int pointCount = points.Length;
            int[] output = new int[pointCount];
            float[] angles = new float[pointCount];
            for(int a = 0; a < pointCount; a++)
            {
                angles[a] = Vectors.SignAngle(points[a] - center);
            }

            bool[] used = new bool[pointCount];
            for(int a = 0; a < pointCount; a++)
            {
                float lowestAngle = 180;
                int index = -1;
                for(int ax = 0; ax < pointCount; ax++)
                {
                    if(used[ax])
                    {
                        continue;
                    }

                    if(angles[ax] <= lowestAngle)
                    {
                        lowestAngle = angles[ax];
                        index = ax;
                    }
                }

                if(index != -1)
                {
                    output[a] = index;
                    used[index] = true;
                }
            }

            return output;
        }

        public static int[] SortPointByAngle(Vector2[] points, Vector2 center)
        {
            int pointCount = points.Length;
            int[] output = new int[pointCount];
            float[] angles = new float[pointCount];
            for(int a = 0; a < pointCount; a++)
            {
                angles[a] = Vectors.SignAngle(points[a] - center);
            }

            bool[] used = new bool[pointCount];
            for(int a = 0; a < pointCount; a++)
            {
                float lowestAngle = 180;
                int index = -1;
                for(int ax = 0; ax < pointCount; ax++)
                {
                    if(used[ax])
                    {
                        continue;
                    }

                    if(angles[ax] <= lowestAngle)
                    {
                        lowestAngle = angles[ax];
                        index = ax;
                    }
                }

                if(index != -1)
                {
                    output[a] = index;
                    used[index] = true;
                }
            }

            return output;
        }

        // Same as ::ref::Lerp but makes sure the values interpolate correctly when they wrap around 360 degrees.
        public static float LerpAngle(float a, float b, float t)
        {
            float delta = Repeat((b - a), 360);
            if (delta > 180)
            {
                delta -= 360;
            }

            return a + delta * Numbers.Clamp01(t);
        }

        // Moves a value /current/ towards /target/.
        static public float MoveTowards(float current, float target, float maxDelta)
        {
            if (Mathf.Abs(target - current) <= maxDelta)
            {
                return target;
            }

            return current + Mathf.Sign(target - current) * maxDelta;
        }

        // Same as ::ref::MoveTowards but makes sure the values interpolate correctly when they wrap around 360 degrees.
        static public float MoveTowardsAngle(float current, float target, float maxDelta)
        {
            float deltaAngle = DeltaAngle(current, target);
            if (-maxDelta < deltaAngle && deltaAngle < maxDelta)
            {
                return target;
            }

            target = current + deltaAngle;
            return MoveTowards(current, target, maxDelta);
        }
        
        public static float DeltaAngle(float current, float target)
        {
            float delta = Repeat((target - current), 360);
            if (delta > 180.0F)
            {
                delta -= 360.0F;
            }

            return delta;
        }

        // Loops the value t, so that it is never larger than length and never smaller than 0.
        public static float Repeat(float t, float length)
        {
            return Numbers.Clamp(t - Mathf.Floor(t / length) * length, 0.0f, length);
        }
    }
}