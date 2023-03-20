using System;
using System.Collections.Generic;
using opengen.maths;
using UnityEngine;

namespace opengen.types
{
    [Serializable]
    public class DelaunayPoint
    {
        /// <summary>
        /// Used only for generating a unique ID for each instance of this class that gets generated
        /// </summary>
        private static int _counter;

        /// <summary>
        /// Used for identifying an instance of a class; can be useful in troubleshooting when geometry goes weird
        /// (e.g. when trying to identify when Triangle objects are being created with the same Point object twice)
        /// </summary>
        private readonly int _instanceId = _counter++;

        private readonly float _x;
        private readonly float _y;

        public float x => _x;
        public float y => _y;

        public Vector2 vector2 => new Vector2(_x, _y);

        public HashSet<DelaunayTriangle> AdjacentTriangles { get; } = new HashSet<DelaunayTriangle>();

        public DelaunayPoint(float x, float y)
        {
            _x = x;
            _y = y;
        }

        public DelaunayPoint(Vector2 p)
        {
            _x = p.x;
            _y = p.y;
        }

        public DelaunayPoint(Vector2Fixed p)
        {
            _x = p.vx;
            _y = p.vy;
        }
        
        public float magnitude
        {
            get { return Mathf.Sqrt(SqrMagnitude()); }
        }

        public float SqrMagnitude()
        {
            return x * x + y * y;
        }

        public override string ToString()
        {
            return $"( {x:F4} , {y:F4} )";
        }

        public static float Distance(DelaunayPoint from, DelaunayPoint to)
        {
            return (from - to).magnitude;
        }
        
        public DelaunayPoint normalized => Normalise(new DelaunayPoint(x, y));

        // public void Normalise()
        // {
        //     this = Normalise(this);
        // }

        public static DelaunayPoint Normalise(DelaunayPoint vector2)
        {
            float mag = vector2.magnitude;
            if (mag > Numbers.Epsilon)
            {
                vector2 = vector2 / mag;
            }
            else
            {
                vector2 = new DelaunayPoint(Vector2.zero);
            }

            return vector2;
        }

        public bool Equals(DelaunayPoint p)
        {
            bool xe = Math.Abs(x - p.x) < Numbers.Epsilon;
            bool ye = Math.Abs(y - p.y) < Numbers.Epsilon;
            return xe && ye;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2);
        }
        
        public static DelaunayPoint RotateTowards(DelaunayPoint from, DelaunayPoint to, float maxDegrees) {
            float angleFrom = DelaunayPoint.Angle(DelaunayPoint.up, from);
            float angleTo = DelaunayPoint.Angle(DelaunayPoint.up, to);
            float deltaAngle = Angles.DeltaAngle(angleFrom, angleTo);
            deltaAngle = Math.Min(deltaAngle, maxDegrees);
            return Rotate(from, deltaAngle);
        }

        public static DelaunayPoint ClampLerp(DelaunayPoint from, DelaunayPoint to, float maxDegrees, float lerp) {
            float angleFrom = DelaunayPoint.Angle(DelaunayPoint.up, from);
            float angleTo = DelaunayPoint.Angle(DelaunayPoint.up, to);
            float deltaAngle = Angles.DeltaAngle(angleFrom, angleTo);
            deltaAngle = Math.Min(deltaAngle, maxDegrees);
            DelaunayPoint useTo = Rotate(from, deltaAngle);
            return DelaunayPoint.Lerp(from, useTo, lerp);
        }

        public override bool Equals(object a)
        {
            if(a.GetType() != typeof(DelaunayPoint))
            {
                return false;
            }

            return Equals((DelaunayPoint)a);
        }

        public static bool operator ==(DelaunayPoint a, DelaunayPoint b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(DelaunayPoint a, DelaunayPoint b)
        {
            return !a.Equals(b);
        }

        public static DelaunayPoint operator +(DelaunayPoint a, DelaunayPoint b)
        {
            return new DelaunayPoint(a.x + b.x, a.y + b.y);
        }

        public static DelaunayPoint operator -(DelaunayPoint a)
        {
            return new DelaunayPoint(-a.x, -a.y);
        }

        public static DelaunayPoint operator -(DelaunayPoint a, DelaunayPoint b)
        {
            return new DelaunayPoint(a.x - b.x, a.y - b.y);
        }

        public static DelaunayPoint operator *(DelaunayPoint a, DelaunayPoint b)
        {
            return new DelaunayPoint(a.x * b.x, a.y * b.y);
        }

        public static DelaunayPoint operator *(DelaunayPoint a, float b)
        {
            return new DelaunayPoint(a.x * b, a.y * b);
        }

        public static DelaunayPoint operator *(float a, DelaunayPoint b)
        {
            return new DelaunayPoint(a * b.x, a * b.y);
        }

        public static DelaunayPoint operator /(DelaunayPoint a, DelaunayPoint b)
        {
            return new DelaunayPoint(a.x / b.x, a.y / b.y);
        }

        public static DelaunayPoint operator /(DelaunayPoint a, float b)
        {
            return new DelaunayPoint(a.x / b, a.y / b);
        }

        public static DelaunayPoint operator /(float a, DelaunayPoint b)
        {
            return new DelaunayPoint(a / b.x, a / b.y);
        }

        public static float Dot(DelaunayPoint a, DelaunayPoint b)
        {
            return (a.x * b.x) + (a.y * b.y);
        }

        public static DelaunayPoint Lerp(DelaunayPoint a, DelaunayPoint b, float t)
        {
            return new DelaunayPoint(Numbers.RoundToInt(Numbers.Lerp(a.x, b.x, t)), Numbers.RoundToInt(Numbers.Lerp(a.y, b.y, t)));
        }

        public static float Angle(DelaunayPoint from, DelaunayPoint to)
        {
            return Mathf.Atan2(to.y - from.y, to.x - from.x);
        }
        
        public static float SignAngle(DelaunayPoint from, DelaunayPoint to)
        {
            DelaunayPoint vector = to - from;
            DelaunayPoint dir = vector.normalized;
            float angle = Angle(up, dir);
            Vector3 dirV3 = new Vector3(dir.x, 0, dir.y);
            Vector3 cross = Vector3.Cross(Vector3.up, dirV3);
            if (cross.z > 0)
            {
                angle = -angle;
            }

            return angle;
        }

        public static float SignAngle(DelaunayPoint dir)
        {
            float angle = Angle(up, dir);
            Vector3 dirV3 = new Vector3(dir.x, 0, dir.y);
            Vector3 cross = Vector3.Cross(Vector3.forward, dirV3);
            if (cross.z > 0)
            {
                angle = -angle;
            }

            return angle;
        }

        public static float SignAngleDirection(DelaunayPoint dirForward, DelaunayPoint dirAngle)
        {
            float angle = Angle(dirForward, dirAngle);
            DelaunayPoint cross = Rotate(dirForward, 90);
            float crossDot = Dot(cross, dirAngle);
            if (crossDot < 0)
            {
                angle = -angle;
            }

            return angle;
        }

        public static DelaunayPoint Rotate(DelaunayPoint input, float degrees)
        {
            float sin = Mathf.Sin(degrees * Numbers.Deg2Rad);
            float cos = Mathf.Cos(degrees * Numbers.Deg2Rad);

            float tx = input.x;
            float ty = input.y;
            // ReSharper disable once UseObjectOrCollectionInitializer - not readable
            DelaunayPoint output = new DelaunayPoint(Numbers.RoundToInt((cos * tx) - (sin * ty)), Numbers.RoundToInt((sin * tx) + (cos * ty)));
            return output;
        }
        
        public static DelaunayPoint Min(Vector3 a, Vector3 b)
        {
            return new DelaunayPoint(Math.Min(a.x, b.x), Math.Min(a.y, b.y));
        }

        public static DelaunayPoint Max(Vector3 a, Vector3 b)
        {
            return new DelaunayPoint(Math.Max(a.x, b.x), Math.Max(a.y, b.y));
        }
        
        public static DelaunayPoint zero => new DelaunayPoint(0, 0);
        public static DelaunayPoint one => new DelaunayPoint(1, 1);
        public static DelaunayPoint up => new DelaunayPoint(0, 1);
        public static DelaunayPoint down => new DelaunayPoint(0, -1);
        public static DelaunayPoint left => new DelaunayPoint(-1, 0);
        public static DelaunayPoint right => new DelaunayPoint(1, 0);
    }
}