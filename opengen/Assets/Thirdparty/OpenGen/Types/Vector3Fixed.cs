using System;
using opengen.maths;
using UnityEngine;

namespace opengen.types
{
    [Serializable]
    public struct Vector3Fixed
    {
        private const float SCALE = 0.0001f;//0.1mm

        public int x;
        public int y;
        public int z;

        //scale conversion values
        public float vx
        {
            get { return x * SCALE; }
            set { x = ConvertFromWorldUnits(value); }
        }

        public float vy
        {
            get { return y * SCALE; }
            set { y = ConvertFromWorldUnits(value); }
        }

        public float vz
        {
            get { return z * SCALE; }
            set { z = ConvertFromWorldUnits(value); }
        }

        public Vector3Fixed(int x = 0, int y = 0, int z = 0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3Fixed(float x = 0, float y = 0, float z = 0)
        {
            this.x = ConvertFromWorldUnits(x);
            this.y = ConvertFromWorldUnits(y);
            this.z = ConvertFromWorldUnits(z);
        }

        public Vector3Fixed(Vector2 vector2, bool flat)
        {
            if (!flat)
            {
                x = ConvertFromWorldUnits(vector2.x);
                y = ConvertFromWorldUnits(vector2.y);
                z = 0;
            }
            else
            {
                x = ConvertFromWorldUnits(vector2.x);
                y = 0;
                z = ConvertFromWorldUnits(vector2.y);
            }
        }

        public Vector3Fixed(Vector2Fixed vector2)
        {
            x = vector2.x;
            y = 0;
            z = vector2.y;
        }

        public Vector3Fixed(Vector3 vector3)
        {
            x = ConvertFromWorldUnits(vector3.x);
            y = ConvertFromWorldUnits(vector3.y);
            z = ConvertFromWorldUnits(vector3.z);
        }

        public static Vector3Fixed zero { get { return new Vector3Fixed(); } }
        public static Vector3Fixed one { get { return new Vector3Fixed(1, 1, 1); } }
        public static Vector3Fixed up { get { return new Vector3Fixed(0, 1, 0); } }
        public static Vector3Fixed down { get { return new Vector3Fixed(0, -1, 0); } }
        public static Vector3Fixed left { get { return new Vector3Fixed(-1, 0, 0); } }
        public static Vector3Fixed right { get { return new Vector3Fixed(1, 0, 0); } }
        public static Vector3Fixed forward { get { return new Vector3Fixed(0, 0, 1); } }
        public static Vector3Fixed backward { get { return new Vector3Fixed(0, 0, -1); } }

        public float magnitude
        {
            get { return vector3.magnitude; }
        }

        public int SqrMagnitudeInt()
        {
            return x * x + y * y + z * z;
        }

        public float SqrMagnitudefloat()
        {
            return vector3.sqrMagnitude;
        }

        public override string ToString()
        {
            return string.Format("( {0} , {1} , {2} )", vx, vy, vz);
        }

        public static float DistanceWorld(Vector3Fixed from, Vector3Fixed to)
        {
            return Vector3.Distance(from.vector3, to.vector3);
        }

        public bool Equals(Vector3Fixed p)
        {

            // Return true if the fields match:
            return (x == p.x) && (y == p.y) && (z == p.z);
        }

        public override int GetHashCode()
        {
            return x ^ y ^ z;
        }

        public override bool Equals(object a)
        {
            if(a.GetType() != typeof(Vector3Fixed))
            {
                return false;
            }

            return Equals((Vector3Fixed)a);
        }

        public Vector3Fixed Move(Vector3 amount)
        {
            x += Numbers.RoundToInt(amount.x / SCALE);
            y += Numbers.RoundToInt(amount.y / SCALE);
            z += Numbers.RoundToInt(amount.z / SCALE);
            return this;
        }

        public Vector3Fixed MoveXy(Vector2 amount)
        {
            x += Numbers.RoundToInt(amount.x / SCALE);
            y += Numbers.RoundToInt(amount.y / SCALE);
            return this;
        }

        public Vector3Fixed MoveXz(Vector2 amount)
        {
            x += Numbers.RoundToInt(amount.x / SCALE);
            z += Numbers.RoundToInt(amount.y / SCALE);
            return this;
        }

        public Vector3 vector3
        {
            get { return new Vector3(x * SCALE, y * SCALE, z * SCALE); }
            set
            {
                x = Numbers.RoundToInt(value.x / SCALE);
                y = Numbers.RoundToInt(value.y / SCALE);
                z = Numbers.RoundToInt(value.z / SCALE);
            }
        }

        public Vector2 vector2Xy
        {
            get { return new Vector2(x * SCALE, y * SCALE); }
            set
            {
                x = Numbers.RoundToInt(value.x / SCALE);
                y = Numbers.RoundToInt(value.y / SCALE);
            }
        }

        public Vector2 vector2Xz
        {
            get { return new Vector2(x * SCALE, z * SCALE); }
            set
            {
                x = Numbers.RoundToInt(value.x / SCALE);
                z = Numbers.RoundToInt(value.y / SCALE);
            }
        }

        public Vector2Fixed vector2FixedXz
        {
            get { return new Vector2Fixed(vx, vz); }
            set
            {
                vx = value.vx;
                vz = value.vy;
            }
        }
        
        public static bool operator ==(Vector3Fixed a, Vector3Fixed b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }

        public static bool operator !=(Vector3Fixed a, Vector3Fixed b)
        {
            return a.x != b.x || a.y != b.y || a.z != b.z;
        }

        public static Vector3Fixed operator +(Vector3Fixed a, Vector3Fixed b)
        {
            return new Vector3Fixed(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3Fixed operator +(Vector3Fixed a, Vector3 b)
        {
            return new Vector3Fixed(a.x + ConvertFromWorldUnits(b.x), a.y + ConvertFromWorldUnits(b.y), a.z + ConvertFromWorldUnits(b.z));
        }

        public static Vector3Fixed operator +(Vector3 a, Vector3Fixed b)
        {
            return new Vector3Fixed(ConvertFromWorldUnits(a.x) + b.x, ConvertFromWorldUnits(a.y) + b.y, ConvertFromWorldUnits(a.z) + b.z);
        }

        public static Vector3Fixed operator -(Vector3Fixed a, Vector3Fixed b)
        {
            return new Vector3Fixed(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3Fixed operator -(Vector3Fixed a, Vector3 b)
        {
            return new Vector3Fixed(a.x - ConvertFromWorldUnits(b.x), a.y - ConvertFromWorldUnits(b.y), a.z - ConvertFromWorldUnits(b.z));
        }

        public static Vector3Fixed operator -(Vector3 a, Vector3Fixed b)
        {
            return new Vector3Fixed(ConvertFromWorldUnits(a.x) - b.x, ConvertFromWorldUnits(a.y) - b.y, ConvertFromWorldUnits(a.z) - b.z);
        }

        public static Vector3Fixed operator *(Vector3Fixed a, Vector3Fixed b)
        {
            return new Vector3Fixed(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static Vector3Fixed operator *(Vector3Fixed a, float b)
        {
            int bM = ConvertFromWorldUnits(b);
            return new Vector3Fixed(a.x * bM, a.y * bM, a.z * bM);
        }

        public static Vector3Fixed operator *(Vector3Fixed a, int b)
        {
            int bM = ConvertFromWorldUnits(b);
            return new Vector3Fixed(a.x * bM, a.y * bM, a.z * bM);
        }

        public static Vector3Fixed operator /(Vector3Fixed a, Vector3Fixed b)
        {
            return new Vector3Fixed(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        public static Vector3Fixed operator /(Vector3Fixed a, float b)
        {
            int bM = ConvertFromWorldUnits(b);
            return new Vector3Fixed(a.x / bM, a.y / bM);
        }

        public static Vector3Fixed operator /(Vector3Fixed a, int b)
        {
            int bM = ConvertFromWorldUnits(b);
            return new Vector3Fixed(a.x / bM, a.y / bM, a.z / bM);
        }

        public static float Dot(Vector3Fixed a, Vector3Fixed b)
        {
            return (a.x * b.x) + (a.y * b.y) + (a.z * b.z);
        }

        public static Vector3Fixed Lerp(Vector3Fixed a, Vector3Fixed b, float t)
        {
            Vector3Fixed output = new Vector3Fixed();
            output.x = Numbers.RoundToInt(Numbers.Lerp(a.x, b.x, t));
            output.y = Numbers.RoundToInt(Numbers.Lerp(a.y, b.y, t));
            output.z = Numbers.RoundToInt(Numbers.Lerp(a.z, b.z, t));
            return output;
        }

        public static float Angle(Vector3Fixed from, Vector3Fixed to)
        {
            return Vector3.Angle(from.vector3, to.vector3);
        }

        public static float SignAngle(Vector3Fixed from, Vector3Fixed to)
        {
            Vector3 dir = (to.vector3 - from.vector3).normalized;
            float angle = Vector3.Angle(Vector3.up, dir);
            Vector3 cross = Vector3.Cross(Vector3.forward, dir);
            if (cross.z > 0)
            {
                angle = -angle;
            }

            return angle;
        }

        public static float SignAngle(Vector3Fixed dir)
        {
            return SignAngle(forward, dir);
        }

        public static int ConvertFromWorldUnits(float input)
        {
            return Numbers.RoundToInt(input / SCALE);
        }

        public static float ConvertToWorldUnits(int input)
        {
            return input * SCALE;
        }

        public static int SqrMagnitudeInt(Vector3Fixed a, Vector3Fixed b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static float SqrMagnitudefloat(Vector3Fixed a, Vector3Fixed b)
        {
            return (a.vector3 - b.vector3).sqrMagnitude;
        }

        public static Vector3[] Parse(Vector3Fixed[] input)
        {
            int length = input.Length;
            Vector3[] output = new Vector3[length];
            for (int p = 0; p < length; p++)
            {
                output[p] = input[p].vector3;
            }

            return output;
        }

        public static Vector3Fixed[] Parse(Vector3[] input)
        {
            int length = input.Length;
            Vector3Fixed[] output = new Vector3Fixed[length];
            for (int p = 0; p < length; p++)
            {
                output[p].vx = input[p].x;
                output[p].vy = input[p].y;
            }
            return output;
        }
    }
}