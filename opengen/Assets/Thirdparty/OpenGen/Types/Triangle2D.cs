using System;
using System.Collections.Generic;
using opengen.maths;
using UnityEngine;

namespace opengen.types
{
    [Serializable]
    public struct Triangle2D
    {
        [SerializeField] private Vector2 _p0;
        [SerializeField] private Vector2 _p1;
        [SerializeField] private Vector2 _p2;

        private bool _isCircumcircleCalculated;
        private float _radiusSquared;
        private Vector2 _circumcenter;

        public Vector2 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _p0;
                    case 1:
                        return _p1;
                    case 2:
                        return _p2;
                }

                throw new Exception("Triangle3D Index out of range");
            }
        }

        public Vector2 p0 => _p0;
        public Vector2 p1 => _p1;
        public Vector2 p2 => _p2;

        public float radiusSquared
        {
            get
            {
                if(!_isCircumcircleCalculated)
                {
                    CalculateCircumcircle();
                }

                return radiusSquared;
            }
        }

        public Vector2 circumcenter
        {
            get
            {
                if(!_isCircumcircleCalculated)
                {
                    CalculateCircumcircle();
                }

                return _circumcenter;
            }
        }

        public bool Contains(Vector2 p)
        {
            if (_p0 == p)
            {
                return true;
            }

            if (_p1 == p)
            {
                return true;
            }

            if (_p2 == p)
            {
                return true;
            }

            return false;
        }

        public Vector2 Incenter()
        {
            return CalculateIncenter(_p0, _p1, _p2);
        }

        public Triangle2D(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            _p0 = p0;
            _p1 = p1;
            _p2 = p2;
            _radiusSquared = 0;
            _circumcenter = Vector2.zero;
            _isCircumcircleCalculated = false;
        }

        public Triangle2D(Vector2Fixed v0, Vector2Fixed v1, Vector2Fixed v2)
        {
            _p0 = v0.vector2;
            _p1 = v1.vector2;
            _p2 = v2.vector2;
            _radiusSquared = 0;
            _circumcenter = Vector2.zero;
            _isCircumcircleCalculated = false;
        }

        public Vector2 CalculateCenter()
        {
            return CalculateCenter(_p0, _p1, _p2);
        }

        public bool CheckSize()
        {
            if ((_p0 - _p1).sqrMagnitude < Numbers.Epsilon)
            {
                return false;
            }

            if ((_p0 - _p2).sqrMagnitude < Numbers.Epsilon)
            {
                return false;
            }

            if ((_p2 - _p1).sqrMagnitude < Numbers.Epsilon)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calculate the normal of a triangle
        /// </summary>
        public static Vector2 CalculateCenter(Vector2 v0, Vector2 v1, Vector2 v2)
        {
            return (v0 + v1 + v2) / 3f;
        }

        public static float SignedArea(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            return 0.5f * (-p1.y * p2.x + p0.y * (-p1.x + p2.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y);
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

        public static bool PointInTriangle(Vector2 point, Vector2 t0, Vector2 t1, Vector2 t2)
        {
            float s = t0.y * t2.x - t0.x * t2.y + (t2.y - t0.y) * point.x + (t0.x - t2.x) * point.y;
            float t = t0.x * t1.y - t0.y * t1.x + (t0.y - t1.y) * point.x + (t1.x - t0.x) * point.y;

            if ((s < 0) != (t < 0))
            {
                return false;
            }

            float signedArea = SignedArea(t0, t1, t2);
            if (signedArea < 0.0)
            {
                s = -s;
                t = -t;
                signedArea = -signedArea;
            }

            return s > 0 && t > 0 && (s + t) < signedArea;
        }
        
        private void CalculateCircumcircle()
        {
            // https://codefound.wordpress.com/2013/02/21/how-to-compute-a-circumcircle/#more-58
            // https://en.wikipedia.org/wiki/Circumscribed_circle
            float dA = _p0.x * _p0.x + _p0.y * _p0.y;
            float dB = _p1.x * _p1.x + _p1.y * _p1.y;
            float dC = _p2.x * _p2.x + _p2.y * _p2.y;

            float aux1 = (dA * (_p2.y - _p1.y) + dB * (_p0.y - _p2.y) + dC * (_p1.y - _p0.y));
            float aux2 = -(dA * (_p2.x - _p1.x) + dB * (_p0.x - _p2.x) + dC * (_p1.x - _p0.x));
            float div = (2 * (_p0.x * (_p2.y - _p1.y) + _p1.x * (_p0.y - _p2.y) + _p2.x * (_p1.y - _p0.y)));

            if (div == 0)
            {
                throw new DivideByZeroException();
            }

            Vector2 center = new Vector2(aux1 / div, aux2 / div);
            _circumcenter = center;
            _radiusSquared = (center.x - _p0.x) * (center.x - _p0.x) + (center.y - _p0.y) * (center.y - _p0.y);
            _isCircumcircleCalculated = true;
        }

        private bool IsCounterClockwise(Vector2 point1, Vector2 point2, Vector2 point3)
        {
            float result = (point2.x - point1.x) * (point3.y - point1.y) -
                           (point3.x - point1.x) * (point2.y - point1.y);
            return result > 0;
        }

        public bool SharesEdgeWith(Triangle2D triangle)
        {
            int sharedVertices = 0;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    if(this[i] == triangle[j])
                    {
                        sharedVertices++;
                    }
                }

            return sharedVertices == 2;
        }

        public bool IsPointInsideCircumcircle(Vector2 point)
        {
            if(!_isCircumcircleCalculated)
            {
                CalculateCircumcircle();
            }

            float dSquared = (point.x - _circumcenter.x) * (point.x - _circumcenter.x) +
                             (point.y - _circumcenter.y) * (point.y - _circumcenter.y);
            return dSquared < _radiusSquared;
        }
        
        // public IEnumerable<Triangle2D> TrianglesWithSharedEdge {
        //     get {
        //         var neighbors = new HashSet<Triangle2D>();
        //         for (int i = 0; i < 3; i++)
        //         {
        //             var trianglesWithSharedEdge = this[i].AdjacentTriangles.Where(o =>
        //             {
        //                 return o != this && SharesEdgeWith(o);
        //             });
        //             neighbors.UnionWith(trianglesWithSharedEdge);
        //         }
        //
        //         return neighbors;
        //     }
        // }

        public static Vector2 CalculateIncenter(Vector2 a, Vector2 b, Vector2 c)
        {
            // Calculate the lengths of the sides of the triangle
            float ab = Vector2.Distance(a, b);
            float ac = Vector2.Distance(a, c);
            float bc = Vector2.Distance(b, c);

            // Calculate the incenter of the triangle
            return (ab * c + ac * b + bc * a) / (ab + ac + bc);
        }
    }
}