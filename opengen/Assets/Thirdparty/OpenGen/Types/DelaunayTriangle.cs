using System;
using System.Collections.Generic;
using System.Linq;
using opengen.maths;
using UnityEngine;

namespace opengen.types
{
    [Serializable]
    public class DelaunayTriangle
    {
        [SerializeField] private DelaunayPoint _p0;
        [SerializeField] private DelaunayPoint _p1;
        [SerializeField] private DelaunayPoint _p2;
        [SerializeField] private float _radiusSquared;
        [SerializeField] private DelaunayPoint _circumcenter;

        public DelaunayPoint this[int index]
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

        public DelaunayPoint p0 => _p0;
        public DelaunayPoint p1 => _p1;
        public DelaunayPoint p2 => _p2;
        public float radiusSquared => _radiusSquared;
        public DelaunayPoint circumcenter => _circumcenter;

        public bool Contains(DelaunayPoint p)
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

        public DelaunayTriangle(DelaunayPoint p0, DelaunayPoint p1, DelaunayPoint p2)
        {
            if (p0 == p1 || p0 == p2 || p1 == p2)
            {
                throw new ArgumentException("Must be 3 distinct points");
            }

            if (!IsCounterClockwise(p0, p1, p2))
            {
                _p0 = p0;
                _p1 = p2;
                _p2 = p1;
            }
            else
            {
                _p0 = p0;
                _p1 = p1;
                _p2 = p2;
            }

            _p0.AdjacentTriangles.Add(this);
            _p1.AdjacentTriangles.Add(this);
            _p2.AdjacentTriangles.Add(this);
            CalculateCircumcircle();
        }

        public DelaunayPoint CalculateCenter()
        {
            return CalculateCenter(_p0, _p1, _p2);
        }

        public bool CheckSize()
        {
            if ((_p0 - _p1).SqrMagnitude() < Numbers.Epsilon)
            {
                return false;
            }

            if ((_p0 - _p2).SqrMagnitude() < Numbers.Epsilon)
            {
                return false;
            }

            if ((_p2 - _p1).SqrMagnitude() < Numbers.Epsilon)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calculate the normal of a triangle
        /// </summary>
        public static DelaunayPoint CalculateCenter(DelaunayPoint v0, DelaunayPoint v1, DelaunayPoint v2)
        {
            return (v0 + v1 + v2) / 3f;
        }

        public static float SignedArea(DelaunayPoint p0, DelaunayPoint p1, DelaunayPoint p2)
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

        public static bool PointInTriangle(DelaunayPoint point, DelaunayPoint t0, DelaunayPoint t1, DelaunayPoint t2)
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

            DelaunayPoint center = new DelaunayPoint(aux1 / div, aux2 / div);
            _circumcenter = center;
            _radiusSquared = (center.x - _p0.x) * (center.x - _p0.x) + (center.y - _p0.y) * (center.y - _p0.y);
        }

        private bool IsCounterClockwise(DelaunayPoint point1, DelaunayPoint point2, DelaunayPoint point3)
        {
            float result = (point2.x - point1.x) * (point3.y - point1.y) -
                           (point3.x - point1.x) * (point2.y - point1.y);
            return result > 0;
        }

        public bool SharesEdgeWith(DelaunayTriangle triangle)
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

        public bool IsPointInsideCircumcircle(DelaunayPoint point)
        {
            float xdelta = point.x - _circumcenter.x;
            float ydelta = point.y - _circumcenter.y;
            // float dSquared = (point.x - _circumcenter.x) * (point.x - _circumcenter.x) +
            //                     (point.y - _circumcenter.y) * (point.y - _circumcenter.y);
            float dSquared = xdelta * xdelta + ydelta * ydelta;
            return dSquared < _radiusSquared;
        }
        
        public IEnumerable<DelaunayTriangle> TrianglesWithSharedEdge {
            get {
                var neighbors = new HashSet<DelaunayTriangle>();
                for (int i = 0; i < 3; i++)
                {
                    var trianglesWithSharedEdge = this[i].AdjacentTriangles.Where(o =>
                    {
                        return o != this && SharesEdgeWith(o);
                    });
                    neighbors.UnionWith(trianglesWithSharedEdge);
                }
        
                return neighbors;
            }
        }

        public Vector2[] Points()
        {
            return new[] { _p0.vector2, _p1.vector2, _p2.vector2 };
        }
    }
}