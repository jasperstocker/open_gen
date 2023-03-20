using System;
using opengen.maths;
using UnityEngine;

namespace opengen.types
{
    [Serializable]
    public struct Vector2Fixed
    {
        public static float SCALE = 0.0001f;//0.1mm

        public int x;
        public int y;

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

        public Vector2Fixed(int x = 0, int y = 0)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2Fixed(float x = 0, float y = 0)
        {
            this.x = ConvertFromWorldUnits(x);
            this.y = ConvertFromWorldUnits(y);
        }

        public Vector2Fixed(Vector2 vector2)
        {
            x = ConvertFromWorldUnits(vector2.x);
            y = ConvertFromWorldUnits(vector2.y);
        }

        public Vector2Fixed(Vector2Fixed vector2)
        {
            x = vector2.x;
            y = vector2.y;
        }

        public Vector2Fixed(Vector3 vector3, bool flat)
        {
            x = ConvertFromWorldUnits(vector3.x);
            if(!flat)
            {
                y = ConvertFromWorldUnits(vector3.y);
            }
            else
            {
                y = ConvertFromWorldUnits(vector3.z);
            }
        }

        public static Vector2Fixed Zero { get { return new Vector2Fixed(); } }
        public static Vector2Fixed One { get { return new Vector2Fixed(1, 1); } }
        public static Vector2Fixed Up { get { return new Vector2Fixed(0, 1); } }
        public static Vector2Fixed Down { get { return new Vector2Fixed(0, -1); } }
        public static Vector2Fixed Left { get { return new Vector2Fixed(-1, 0); } }
        public static Vector2Fixed Right { get { return new Vector2Fixed(1, 0); } }

        public float magnitude
        {
            get { return vector2.magnitude; }
        }

        public int SqrMagnitudeInt()
        {
            return x * x + y * y;
        }

        public float SqrMagnitudefloat()
        {
            return vector2.sqrMagnitude;
        }

        public override string ToString()
        {
            return string.Format("( {0} , {1} )", vx, vy);
        }

        public static float DistanceWorld(Vector2Fixed from, Vector2Fixed to)
        {
            return Vector2.Distance(from.vector2, to.vector2);
        }

        public bool Equals(Vector2Fixed p)
        {

            // Return true if the fields match:
            return (x == p.x) && (y == p.y);
        }

        public override int GetHashCode()
        {
            return (x ^ y).GetHashCode();
        }

        public override bool Equals(object a)
        {
            if(a.GetType() != typeof(Vector2Fixed))
            {
                return false;
            }

            return Equals((Vector2Fixed)a);
        }

        public Vector2Fixed Move(Vector3 amount, bool isFlat)
        {
            x += Numbers.RoundToInt(amount.x / SCALE);
            if(isFlat)
            {
                y += Numbers.RoundToInt(amount.z / SCALE);
            }
            else
            {
                y += Numbers.RoundToInt(amount.y / SCALE);
            }

            return this;
        }
        
        public Vector2Fixed Move(Vector2 amount)
        {
            x += Numbers.RoundToInt(amount.x / SCALE);
            y += Numbers.RoundToInt(amount.y / SCALE);
            return this;
        }

        public Vector2 vector2
        {
            get { return new Vector2(x * SCALE, y * SCALE); }
            set
            {
                x = Numbers.RoundToInt(value.x / SCALE);
                y = Numbers.RoundToInt(value.y / SCALE);
            }
        }

        public Vector3 vector3XY
        {
            get { return new Vector3(x * SCALE, y * SCALE, 0); }
            set
            {
                x = Numbers.RoundToInt(value.x / SCALE);
                y = Numbers.RoundToInt(value.y / SCALE);
            }
        }

        public Vector3 vector3XZ
        {
            get { return new Vector3(x * SCALE, 0, y * SCALE); }
            set
            {
                x = Numbers.RoundToInt(value.x / SCALE);
                y = Numbers.RoundToInt(value.z / SCALE);
            }
        }

        public static Vector2Fixed up {
            get { return new Vector2Fixed(0,1);}
        }

        public static bool operator ==(Vector2Fixed a, Vector2Fixed b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(Vector2Fixed a, Vector2Fixed b)
        {
            return a.x != b.x || a.y != b.y;
        }

        public static Vector2Fixed operator +(Vector2Fixed a, Vector2Fixed b)
        {
            return new Vector2Fixed(a.x + b.x, a.y + b.y);
        }

        public static Vector2Fixed operator +(Vector2Fixed a, Vector2 b)
        {
            return new Vector2Fixed(a.x + ConvertFromWorldUnits(b.x), a.y + ConvertFromWorldUnits(b.y));
        }

        public static Vector2Fixed operator +(Vector2 a, Vector2Fixed b)
        {
            return new Vector2Fixed(ConvertFromWorldUnits(a.x) + b.x, ConvertFromWorldUnits(a.y) + b.y);
        }

        public static Vector2Fixed operator -(Vector2Fixed a, Vector2Fixed b)
        {
            return new Vector2Fixed(a.x - b.x, a.y - b.y);
        }

        public static Vector2Fixed operator -(Vector2Fixed a, Vector2 b)
        {
            return new Vector2Fixed(a.x - ConvertFromWorldUnits(b.x), a.y - ConvertFromWorldUnits(b.y));
        }

        public static Vector2Fixed operator -(Vector2 a, Vector2Fixed b)
        {
            return new Vector2Fixed(ConvertFromWorldUnits(a.x) - b.x, ConvertFromWorldUnits(a.y) - b.y);
        }

        public static Vector2Fixed operator *(Vector2Fixed a, Vector2Fixed b)
        {
            return new Vector2Fixed(a.x * b.x, a.y * b.y);
        }

        public static Vector2Fixed operator *(Vector2Fixed a, float b)
        {
            int bM = ConvertFromWorldUnits(b);
            return new Vector2Fixed(a.x * bM, a.y * bM);
        }

        public static Vector2Fixed operator *(Vector2Fixed a, int b)
        {
            return new Vector2Fixed(a.x * b, a.y * b);
        }

        public static Vector2Fixed operator /(Vector2Fixed a, Vector2Fixed b)
        {
            return new Vector2Fixed(a.x / b.x, a.y / b.y);
        }

        public static Vector2Fixed operator /(Vector2Fixed a, float b)
        {
            int bM = ConvertFromWorldUnits(b);
            return new Vector2Fixed(a.x / bM, a.y / bM);
        }

        public static Vector2Fixed operator /(Vector2Fixed a, int b)
        {
            return new Vector2Fixed(a.x / b, a.y / b);
        }

        public static float Dot(Vector2Fixed a, Vector2Fixed b)
        {
            return (a.x * b.x) + (a.y * b.y);
        }

        public static Vector2Fixed Lerp(Vector2Fixed a, Vector2Fixed b, float t)
        {
            Vector2Fixed output = new Vector2Fixed();
            output.x = Numbers.RoundToInt(Numbers.Lerp(a.x, b.x, t));
            output.y = Numbers.RoundToInt(Numbers.Lerp(a.y, b.y, t));
            return output;
        }

        public static float Angle(Vector2Fixed from, Vector2Fixed to)
        {
            return Mathf.Atan2(to.y - from.y, to.x - from.x);
        }
        
        public static float SignAngle(Vector2Fixed from, Vector2Fixed to)
        {
            Vector2Fixed dir = new Vector2Fixed((Vector2)(to.vector2 - from.vector2).normalized);
            float angle = Angle(Up, dir);
            Vector3 cross = Vector3.Cross(Vector3.forward, dir.vector3XZ);
            if (cross.z > 0)
            {
                angle = -angle;
            }

            return angle;
        }

        public static float SignAngle(Vector2Fixed dir)
        {
            float angle = Angle(Up, dir);
            Vector3 cross = Vector3.Cross(Vector3.forward, dir.vector3XZ);
            if (cross.z > 0)
            {
                angle = -angle;
            }

            return angle;
        }

        public static float SignAngleDirection(Vector2Fixed dirForward, Vector2Fixed dirAngle)
        {
            float angle = Angle(dirForward, dirAngle);
            Vector2Fixed cross = Rotate(dirForward, 90);
            float crossDot = Dot(cross, dirAngle);
            if (crossDot < 0)
            {
                angle = -angle;
            }

            return angle;
        }

        public static Vector2Fixed Rotate(Vector2Fixed input, float degrees)
        {
            float sin = Mathf.Sin(degrees * Numbers.Deg2Rad);
            float cos = Mathf.Cos(degrees * Numbers.Deg2Rad);

            float tx = input.x;
            float ty = input.y;
            Vector2Fixed output = new Vector2Fixed();
            output.x = Numbers.RoundToInt((cos * tx) - (sin * ty));
            output.y = Numbers.RoundToInt((sin * tx) + (cos * ty));
            return output;
        }

        public static Vector2Fixed Rotate(Vector2 input, float degrees)
        {
            float sin = Mathf.Sin(degrees * Numbers.Deg2Rad);
            float cos = Mathf.Cos(degrees * Numbers.Deg2Rad);

            float tx = input.x;
            float ty = input.y;
            Vector2Fixed output = new Vector2Fixed();
            output.x = Numbers.RoundToInt((cos * tx) - (sin * ty));
            output.y = Numbers.RoundToInt((sin * tx) + (cos * ty));
            return output;
        }

        public static int ConvertFromWorldUnits(float input)
        {
            return Numbers.RoundToInt(input / SCALE);
        }

        public static float ConvertToWorldUnits(int input)
        {
            return input * SCALE;
        }
        
        public static int SqrMagnitudeInt(Vector2Fixed a, Vector2Fixed b)
        {
            return a.x * b.x + a.y * b.y;
        }

        public static float SqrMagnitudefloat(Vector2Fixed a, Vector2Fixed b)
        {
            return (a.vector2 - b.vector2).sqrMagnitude;
        }

        public static Vector2[] Parse(Vector2Fixed[] input) {
            int length = input.Length;
            Vector2[] output = new Vector2[length];
            for (int p = 0; p < length; p++)
            {
                output[p] = input[p].vector2;
            }

            return output;
        }

        public static Vector2Fixed[] Parse(Vector2[] input) {
            int length = input.Length;
            Vector2Fixed[] output = new Vector2Fixed[length];
            for(int p = 0; p < length; p++)
            {
                output[p].vx = input[p].x;
                output[p].vy = input[p].y;
            }
            return output;
        }
    }
}