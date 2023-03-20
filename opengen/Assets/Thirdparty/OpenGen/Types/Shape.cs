using System;
using System.Collections.Generic;
using UnityEngine;

namespace opengen.types
{
    /// <summary>
    /// 2D poly shape
    /// with precalculated internal values like area and center
    /// </summary>
    [Serializable]
    public class Shape
    {
        protected List<Vector2> _points;
        protected float _area;
        protected AABBox _bounds;
        protected Vector2 _center;
        protected Vector2 _centroid;

        protected Shape()
        {
            _points = new List<Vector2>();
        }

        protected Shape(Vector2[] define)
        {
            _points = new List<Vector2>(define.Length);
            for (int i = 0; i < define.Length; i++)
            {
                _points.Add(new Vector2(define[i].x, define[i].y));
            }

            Initialise(_points);
        }

        protected void Initialise(List<Vector2> shape)
        {
            _points = shape;
            UpdateInternals();
        }

        public int pointCount => _points.Count;
        public float area => _area;
        public Vector2[] points => _points.ToArray();
        public AABBox bounds => _bounds;
        public Vector2 center => _center;
        public Vector2 centroid => _centroid;

        public Vector2 this[int index]
        {
            get { return _points[index]; }
            set { _points[index] = value; }
        }

        public Vector3 GetV3(int index)
        {
            return new Vector3(_points[index].x, 0, _points[index].y);
        }

        public void SetFromV3(int index, Vector3 value)
        {
            _points[index] = new Vector2(value.x, value.z);
        }

        protected void CalculateArea()
        {
            _area = CalculateConvex(_points);
        }

        protected void CalculateCenters()
        {
            _center = _bounds.center;
            _centroid = GetCentroid();
        }

        public void UpdateInternals()
        {
            CalculateArea();
            _bounds = new AABBox(_points.ToArray());
            CalculateCenters();
        }

        public bool PointInside(Vector2 point)
        {
            Rect polyBounds = new Rect(0, 0, 0, 0);
            foreach (Vector2 polyPoint in _points)
            {
                if (polyBounds.xMin > polyPoint.x)
                {
                    polyBounds.xMin = polyPoint.x;
                }

                if (polyBounds.xMax < polyPoint.x)
                {
                    polyBounds.xMax = polyPoint.x;
                }

                if (polyBounds.yMin > polyPoint.y)
                {
                    polyBounds.yMin = polyPoint.y;
                }

                if (polyBounds.yMax < polyPoint.y)
                {
                    polyBounds.yMax = polyPoint.y;
                }
            }

            if (!polyBounds.Contains(point))
            {
                return false;
            }

            Vector2 pointRight = point + new Vector2(polyBounds.width, 0);

            int numberOfPolyPoints = _points.Count;
            int numberOfCrossOvers = 0;
            for (int i = 0; i < numberOfPolyPoints; i++)
            {
                Vector2 p0 = _points[i];
                Vector2 p1 = _points[i < numberOfPolyPoints - 1 ? i + 1 : 0];
                if (FastLineIntersection(point, pointRight, p0, p1))
                {
                    numberOfCrossOvers++;
                }
            }

            return numberOfCrossOvers > 0 && numberOfCrossOvers % 2 != 0;
        }

        private bool FastLineIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            if (a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2)
            {
                return false;
            }

            return (Ccw(a1, b1, b2) != Ccw(a2, b1, b2)) && (Ccw(a1, a2, b1) != Ccw(a1, a2, b2));
        }

        private bool Ccw(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return ((p2.x - p1.x) * (p3.y - p1.y) > (p2.y - p1.y) * (p3.x - p1.x));
        }


        private Vector2 GetCentroid()
        {
            float accumulatedArea = 0.0f;
            float centerX = 0.0f;
            float centerY = 0.0f;

            int count = _points.Count;
            Vector2 pointX = _points[count - 1];
            for (int i = 0; i < count; i++)
            {
                Vector2 point = _points[i];
                float temp = point.x * pointX.y - pointX.x * point.y;
                accumulatedArea += temp;
                centerX += (point.x + pointX.x) * temp;
                centerY += (point.y + pointX.y) * temp;
                pointX = point;
            }

            if (Mathf.Abs(accumulatedArea) < 1E-7f)
            {
                return Vector2.zero; // Avoid division by zero
            }

            accumulatedArea *= 3f;
            return new Vector2(centerX / accumulatedArea, centerY / accumulatedArea);
        }

        public float CalculateConvex(List<Vector2> points)
        {
            int pointCount = points.Count;
            Vector2[] pointsV2 = new Vector2[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                pointsV2[i] = new Vector2(points[i].x, points[i].y);
            }

            return CalculateConvex(pointsV2);
        }

        public float CalculateConvex(Vector2[] points)
        {
            int pointCount = points.Length;
            if (pointCount < 3)
            {
                return 0;
            }

            float output = 0;
            for (int i = 2; i < pointCount; i++)
            {
                Vector2 p0 = points[0];
                Vector2 p1 = points[1];
                Vector2 p2 = points[i];

                float l0 = Vector2.Distance(p0, p1);
                float l1 = Vector2.Distance(p1, p2);
                float l2 = Vector2.Distance(p2, p0);

                output += TriangleAreaHeron(l0, l1, l2);
            }

            return output;
        }

        private float TriangleSignedArea(Vector2 p0, Vector2 p1, Vector3 p2)
        {
            return Mathf.Abs(0.5f * (-p1.y * p2.x + p0.y * (-p1.x + p2.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y));
        }

        public float TriangleAreaHeron(float lengthA, float lengthB, float lengthC)
        {
            float s = (lengthA + lengthB + lengthC) * 0.5f;
            return Mathf.Sqrt(s * (s - lengthA) * (s - lengthB) * (s - lengthB));
        }
    }
}