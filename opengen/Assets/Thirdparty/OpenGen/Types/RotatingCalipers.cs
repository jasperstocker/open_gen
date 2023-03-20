using System;
using System.Collections.Generic;
using opengen.maths;
using opengen.maths.shapes;
using UnityEngine;

namespace opengen.types
{
    public struct RotatingCalipers
    {
        private Vector2 _origin;
        private Vector2 _size;
        private float _angle;

        public RotatingCalipers(IList<Vector2> shape)
        {
            _origin = Vector2.zero;
            _size = Vector2.zero;
            _angle = 0;
            
            int pointCount = shape.Count;
            float[] defWeighting = new float[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                defWeighting[i] = 1;
            }

            Initialise(shape, defWeighting);
        }
        
        public RotatingCalipers(IList<Vector2> shape, IList<float> weights)
        {
            _origin = Vector2.zero;
            _size = Vector2.zero;
            _angle = 0;
            Initialise(shape, weights);
        }
        
        private void Initialise(IList<Vector2> shape, IList<float> weights)
        {
            int pointCount = shape.Count;
            if (pointCount < 3)
            {
                return;
            }

            float xMin = shape[0].x;
            float yMin = shape[0].y;
            float xMax = shape[0].x;
            float yMax = shape[0].y;
            
            float smallestArea = float.PositiveInfinity;
            for (int p = 0; p < pointCount; p++)
            {
                int pb = p < pointCount - 1 ? p + 1 : 0;
                Vector2 p0 = shape[p];
                Vector2 p1 = shape[pb];
                Vector2 dir = (p1 - p0).normalized;
                if (dir.sqrMagnitude < Numbers.kEpsilon)
                {
                    continue;//ignore invalid sides
                }

                float angle = SignAngle(dir);
                
                if (p > 0)
                {
                    if (xMin > p0.x)
                    {
                        xMin = p0.x;
                    }

                    if (xMax < p0.x)
                    {
                        xMax = p0.x;
                    }

                    if (yMin > p0.y)
                    {
                        yMin = p0.y;
                    }

                    if (yMax < p0.y)
                    {
                        yMax = p0.y;
                    }
                }

                float rxMin = p0.x;
                float ryMin = p0.y;
                float rxMax = p0.x;
                float ryMax = p0.y;
                
                for (int i = 0; i < pointCount; i++)
                {
                    Vector2 rotatedPoint = shape[i].Rotate(angle);
                    if (i == 0)
                    {
                        rxMin = rotatedPoint.x;
                        ryMin = rotatedPoint.y;
                        rxMax = rotatedPoint.x;
                        ryMax = rotatedPoint.y;
                    }
                    else
                    {
                        if (rxMin > rotatedPoint.x)
                        {
                            rxMin = rotatedPoint.x;
                        }

                        if (rxMax < rotatedPoint.x)
                        {
                            rxMax = rotatedPoint.x;
                        }

                        if (ryMin > rotatedPoint.y)
                        {
                            ryMin = rotatedPoint.y;
                        }

                        if (ryMax < rotatedPoint.y)
                        {
                            ryMax = rotatedPoint.y;
                        }
                    }
                }
                // Debug.Log(rxMin+" "+rxMax);

                float width = rxMax - rxMin;
                float height = ryMax - ryMin;
                float area = width * height;
                float weightedArea = area * (1 - weights[p] + Numbers.kEpsilon);
                if (weightedArea < smallestArea)
                {
                    smallestArea = area;
                    _size = new Vector2(width, height);
                    _angle = angle;
                }
            }

            _origin = new Vector2(xMin + (xMax - xMin) * 0.5f, yMin + (yMax - yMin) * 0.5f);
            // Debug.DrawLine(new Vector3(xMin, 0, yMin), new Vector3(xMin, 10, yMin), Color.red, 20);
            // Debug.DrawLine(new Vector3(xMax, 0, yMax), new Vector3(xMax, 20, yMax), Color.red, 20);
            //Debug.DrawLine(new Vector3(xMin, 0, yMin), new Vector3(xMax, 0, yMax), Color.red, 20);

            // OBBox bb = new OBBox(new Rect(new Vector2(xMin, yMin), _size), _angle);
            // Vector2[] points = bb.Points;
            // for (int i = 0; i < 4; i++)
            // {
            //     int ib = i < 3 ? i + 1 : 0;
            //     
            //     Debug.DrawLine(points[i].Vector3Flat(4), points[ib].Vector3Flat(4), Color.red, 20);
            // }
        }

        public float CalculateArea()
        {
            return _size.x * _size.y;
        }

        public void CalculateShortCrossSection(out float length, out Vector2 direction, out Vector2 cutCenter)
        {
            cutCenter = _origin;
            
            length = Mathf.Min(_size.x, _size.y);
            
            Vector2 right = Vector2.right.Rotate(-_angle);
            Vector2 up = right.Rotate90Clockwise();
            direction = _size.x > _size.y ? up : right;
        }

        public void CalculateLongCrossSection(out float length, out Vector2 direction, out Vector2 cutCenter)
        {
            cutCenter = _origin;
            
            length = Mathf.Max(_size.x, _size.y);
            
            Vector2 right = Vector2.right.Rotate(-_angle);
            Vector2 up = right.Rotate90Clockwise();
            direction = _size.x < _size.y ? up : right;
        }
        
        private float SignAngle(Vector2 dir) 
        {
            float angle = Vector2.Angle(Vector2.up, dir);
            if (dir.x < 0)
            {
                angle = -angle;
            }

            return angle;
        }
    }
}