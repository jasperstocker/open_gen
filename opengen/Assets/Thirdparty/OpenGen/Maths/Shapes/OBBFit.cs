using System;
using System.Collections.Generic;
using opengen.types;
using UnityEngine;


namespace opengen.maths.shapes
{
    /// <summary>
    /// Rotating Calipers! Init!
    /// </summary>
    public class OBBFit 
    {
        private List<OBBox> _obbList = new();
        private List<float> _areaList = new();

        public OBBox Create(Vector2[] points)
        {
            if (Shapes.IsConvex(points))
            {
                return CreateConvex(points);
            }

            //else
            return CreateConcave(points);
        }

        public OBBox Create(Vector2[] points, bool isConvex)
        {
            if (isConvex)
            {
                return CreateConvex(points);
            }

            //else
            return CreateConcave(points);
        }

        public List<OBBox> CreateSorted(Vector2[] points)
        {
            if (Shapes.IsConvex(points))
            {
                return CreateConvexSortedList(points);
            }

            //else
            return CreateConcaveSorted(points);
        }

        public List<OBBox> CreateSorted(Vector2[] points, bool isConvex)
        {
            if (isConvex)
            {
                return CreateConvexSortedList(points);
            }

            //else
            return CreateConcaveSorted(points);
        }

        public OBBox CreateConcave(Vector2[] points)
        {
            List<int> convexPoints = Convex.CalculateConvexShape(points);

            int convexSize = convexPoints.Count;
            Vector2[] convexHull = new Vector2[convexSize];
            for (int c = 0; c < convexSize; c++)
            {
                convexHull[c] = points[convexPoints[c]];
            }

            return CreateConvex(convexHull);
        }

        public List<OBBox> CreateConcaveSorted(Vector2[] points)
        {
            List<int> convexPoints = Convex.CalculateConvexShape(points);

            int convexSize = convexPoints.Count;
            Vector2[] convexHull = new Vector2[convexSize];
            for (int c = 0; c < convexSize; c++)
            {
                convexHull[c] = points[convexPoints[c]];
            }

            return CreateConvexSortedList(convexHull);
        }

        public static OBBox CreateConvex(IList<Vector2> points)
        {
            int pointCount = points.Count;
            if (pointCount == 0)
            {
                throw new Exception("No points sent!");
            }

            OBBox defaultBoxFit = GetBox();
            OBBox output = defaultBoxFit;
            float minArea = Numbers.Infinity;
            for (int p = 0; p < pointCount; p++)
            {
                Vector2 p0 = points[p];
                Vector2 p1 = points[(p + 1) % pointCount];
                Vector2 dir = (p1 - p0).normalized;
                if (dir.sqrMagnitude < Numbers.Epsilon)
                {
                    continue;//ignore duplicate points
                }

                float angle = Angles.SignAngle(dir);

                AABBox _bounds = new();
                for (int o = 0; o < pointCount; o++)//encapsulate rotated points
                {
                    _bounds.Encapsulate(points[o].Rotate(angle));
                }

                Vector2 center = _bounds.center.Rotate(-angle);
                OBBox candidate = GetBox(center, dir, _bounds.height, new Vector2(-dir.y, dir.x), _bounds.width);
                if(_bounds.Area() < minArea)
                {
                    if(output != defaultBoxFit)
                    {
                        PutBox(output);
                    }

                    output = candidate;
                }
                else
                {
                    PutBox(candidate);
                }
            }

            return output;
        }

        public List<OBBox> CreateConvexSortedList(Vector2[] points)
        {
            List<OBBox> output = new();
            _obbList.Clear();
            _areaList.Clear();
            if (points.Length == 0)
            {
                throw new Exception("No points sent!");
            }

            int pointCount = points.Length;
            for (int p = 0; p < pointCount; p++)
            {
                Vector2 p0 = points[p];
                Vector2 p1 = points[(p + 1) % pointCount];
                Vector2 dir = (p1 - p0).normalized;
                if (dir.sqrMagnitude < Numbers.Epsilon)
                {
                    continue;//ignore invalid sides
                }

                float angle = Angles.SignAngle(dir);

                AABBox _bounds = new();
                for (int o = 0; o < pointCount; o++)//encapsulate rotated points
                {
                    _bounds.Encapsulate(points[o].Rotate(angle));
                }

                Vector2 center = _bounds.center.Rotate(-angle);
                OBBox boxFit = GetBox(center, dir, _bounds.height, dir.Rotate90Clockwise(), _bounds.width);
                _obbList.Add(boxFit);
                _areaList.Add(_bounds.Area());
            }

            int obbCount = _obbList.Count;
            OBBox defaultBoxFit = GetBox();
            for(int i = 0; i < obbCount; i++)
            {
                float smallestArea = float.PositiveInfinity;
                OBBox candidate = defaultBoxFit;
                int index = -1;
                for(int j = 0; j < _obbList.Count; j++)
                {
                    if(smallestArea > _areaList[j])
                    {
                        candidate = _obbList[j];
                        smallestArea = _areaList[j];
                        index = j;
                    }
                }

                if(index != -1)
                {
                    output.Add(candidate);
                    _obbList.RemoveAt(index);
                    _areaList.RemoveAt(index);
                }
            }

            PutBox(defaultBoxFit);
            while(_obbList.Count > 0) {
                PutBox(_obbList[0]);
                _obbList.RemoveAt(0);
            }

            return output;
        }

        public static OBBox CreateAlongSide(Vector2[] points, int index)
        {
            if (points.Length == 0)
            {
                throw new Exception("No points sent!");
            }

            int pointCount = points.Length;
            OBBox output = GetBox();

            Vector2 p0 = points[index];
            Vector2 p1 = points[index < pointCount - 1 ? index + 1 : 0];
            Vector2 dir = (p1 - p0).normalized;
            float angle = Angles.SignAngle(dir);

            AABBox bounds = new();
            for (int o = 0; o < pointCount; o++)//encapsulate rotated points
            {
                bounds.Encapsulate(points[o].Rotate(angle));
            }

            Vector2 center = bounds.center.Rotate(-angle);
            output.SetValues(center, dir, bounds.height, dir.Rotate90Clockwise(), bounds.width);

            return output;
        }

        //STATICS
        //POOL
        private static List<OBBox> _obPool = new();

        public static void Release() {
            _obPool.Clear();
        }

        public static OBBox GetBox() {
            OBBox output;
            if (_obPool.Count > 0) {
                output = _obPool[0];
                _obPool.RemoveAt(0);
            }
            else {
                output = new OBBox();
            }
            return output;
        }

        public static OBBox GetBox(Vector2 newCenter, Vector2 dirA, float sizeA, Vector2 dirB, float sizeB) {
            OBBox output;
//            if (_obPool.Count > 0) {
//                output = _obPool[0];
//                output.SetValues(newCenter, dirA, sizeA, dirB, sizeB);
//                _obPool.RemoveAt(0);
//            }
//            else {
                output = new OBBox(newCenter, dirA, sizeA, dirB, sizeB);
//            }
            return output;
        }

        public static void PutBox(OBBox boxFit) {
            boxFit.Clear();
            _obPool.Add(boxFit);
        }
    }
}

