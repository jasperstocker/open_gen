using System;
using System.Collections.Generic;
using System.Linq;
using opengen.maths.shapes;
using opengen.types;
using UnityEngine;


namespace opengen.maths
{
    public static class Shapes
    {
        public static bool PointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            var s = p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y;
            var t = p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y;

            if ((s < 0) != (t < 0))
            {
                return false;
            }

            var A = -p1.y * p2.x + p0.y * (p2.x - p1.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y;
            if (A < 0.0)
            {
                s = -s;
                t = -t;
                A = -A;
            }

            return s > 0 && t > 0 && (s + t) < A;
        }

        public static bool IsConvex(IList<Vector2> points)
        {
            Vector2 center = GetCenter(points);
            int pointCount = points.Count;

            float furthestPointDistance = 0;
            int furthestPointIndex = 0;
            for (int p = 0; p < pointCount; p++)
            {
                float sqrMag = (points[p] - center).sqrMagnitude;
                if (sqrMag > furthestPointDistance)
                {
                    furthestPointDistance = sqrMag;
                    furthestPointIndex = p;
                }
            }

            float sign = 0;
            for (int p = 0; p < pointCount; p++)
            {
                Vector2 pa = points[(p + furthestPointIndex) % pointCount];
                Vector2 pb = points[(p + 1 + furthestPointIndex) % pointCount];
                Vector2 pc = points[(p + 2 + furthestPointIndex) % pointCount];
                float cross = Vectors.Cross(pa, pb, pc);
                float crossSign = Mathf.Sign(cross);
                if (p == 0)
                {
                    sign = crossSign;
                    continue;
                }

                if (sign != crossSign)
                {
                    return false;
                }
            }

            return true;
        }

        private static Vector2 GetCenter(IList<Vector2> points)
        {
            AABBox bounds = new();
            int pointCount = points.Count;
            for (int i = 0; i < pointCount; i++)
            {
                bounds.Encapsulate(points[i]);
            }

            return bounds.center;
        }

        public static bool PointInside(Vector2 point, IList<Vector2> poly)
        {
            Rect polyBounds = new(0, 0, 0, 0);
            foreach (Vector2 polyPoint in poly)
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

            int numberOfPolyPoints = poly.Count;
            int numberOfCrossOvers = 0;
            for (int i = 0; i < numberOfPolyPoints; i++)
            {
                Vector2 p0 = poly[i];
                Vector2 p1 = poly[i < numberOfPolyPoints - 1 ? i + 1 : 0];
                if (Lines.FastLineIntersection(point, pointRight, p0, p1))
                {
                    numberOfCrossOvers++;
                }
            }

            return numberOfCrossOvers > 0 && numberOfCrossOvers % 2 != 0;
        }

        public static bool PointInside(Vector2 point, IList<Vector2> mainShape, List<int> subShape)
        {
            Rect polyBounds = new(0, 0, 0, 0);
            foreach (int polyPointIndex in subShape)
            {
                Vector2 polyPoint = mainShape[polyPointIndex];
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

            int numberOfPolyPoints = subShape.Count;
            int numberOfCrossOvers = 0;
            for (int i = 0; i < numberOfPolyPoints; i++)
            {
                int i0 = subShape[i];
                int i1 = subShape[i < numberOfPolyPoints - 1 ? i + 1 : 0];
                
                Vector2 p0 = mainShape[i0];
                Vector2 p1 = mainShape[i1];
                if (Lines.FastLineIntersection(point, pointRight, p0, p1))
                {
                    numberOfCrossOvers++;
                }
            }

            return numberOfCrossOvers > 0 && numberOfCrossOvers % 2 != 0;
        }

        public static bool PointInside(Vector2 point, List<Vector2> mainShape, List<int> subShape)
        {
            Rect polyBounds = new(0, 0, 0, 0);
            int numberOfPolyPoints = subShape.Count;
            for (int i = 0; i < numberOfPolyPoints; i++)
            {
                int subShapeIndex = subShape[i];
                Vector2 polyPoint = mainShape[subShapeIndex];
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

            // Debug.Log(subShape.Count);
            // Debug.Log(polyBounds);
            // Debug.Log(point);
            if (!polyBounds.Contains(point))
            {
                // Debug.Log($"outside bounds");
                return false;
            }

            Vector2 pointRight = point + new Vector2(polyBounds.width, 0);

            int numberOfCrossOvers = 0;
            for (int i = 0; i < numberOfPolyPoints; i++)
            {
                int i0 = subShape[i];
                int i1 = subShape[i < numberOfPolyPoints - 1 ? i + 1 : 0];
                
                Vector2 p0 = mainShape[i0];
                Vector2 p1 = mainShape[i1];
                if (Segments.Intersects(point, pointRight, p0, p1))
                {
                    numberOfCrossOvers++;
                }
            }
            // Debug.Log($"PointInside {numberOfCrossOvers}");

            return numberOfCrossOvers > 0 && numberOfCrossOvers % 2 != 0;
        }

        public static bool PointInside(Vector2 point, List<Vector2> poly)
        {
            Rect polyBounds = new(0, 0, 0, 0);
            foreach (Vector2 polyPoint in poly)
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

            int numberOfPolyPoints = poly.Count;
            int numberOfCrossOvers = 0;
            for (int i = 0; i < numberOfPolyPoints; i++)
            {
                Vector2 p0 = poly[i];
                Vector2 p1 = poly[i < numberOfPolyPoints - 1 ? i + 1 : 0];
                if (Lines.FastLineIntersection(point, pointRight, p0, p1))
                {
                    numberOfCrossOvers++;
                }
            }

            return numberOfCrossOvers > 0 && numberOfCrossOvers % 2 != 0;
        }

        public static bool PointInside(Vector2Fixed point, Vector2Fixed[] poly)
        {
            AABBox bounds = new();
            foreach (Vector2Fixed polyPoint in poly)
            {
                bounds.Encapsulate(polyPoint.vector2);
            }

            if (!bounds.Contains(point.vector2))
            {
                return false;
            }

            Vector2Fixed pointRight = point + new Vector2Fixed(bounds.width, 0);

            int numberOfPolyPoints = poly.Length;
            int numberOfCrossOvers = 0;
            for (int i = 0; i < numberOfPolyPoints; i++)
            {
                Vector2Fixed p0 = poly[i];
                Vector2Fixed p1 = poly[(i + 1) % numberOfPolyPoints];
                if (Lines.FastLineIntersection(point, pointRight, p0, p1))
                {
                    numberOfCrossOvers++;
                }
            }
            //            if(numberOfCrossOvers % 2 != 0) bounds.DrawDebug(Color.green);

            return numberOfCrossOvers % 2 != 0;
        }

        public static bool OBBInside(OBBox box, IList<Vector2> poly)
        {
            Rect polyBounds = new(0, 0, 0, 0);
            foreach (Vector2 polyPoint in poly)
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

            if (!polyBounds.Overlaps(box.bounds))
            {
                return false;
            }

            Vector2[] points = box.Points;
            for (int o = 0; o < 4; o++)
            {
                Vector2 point = points[o];
                Vector2 pointRight = point + new Vector2(polyBounds.width, 0);

                int numberOfPolyPoints = poly.Count;
                int numberOfCrossOvers = 0;
                for (int i = 0; i < numberOfPolyPoints; i++)
                {
                    Vector2 p0 = poly[i];
                    Vector2 p1 = poly[i < numberOfPolyPoints - 1 ? i + 1 : 0];
                    if (Lines.FastLineIntersection(point, pointRight, p0, p1))
                    {
                        numberOfCrossOvers++;
                    }
                }

                if (numberOfCrossOvers == 0)
                {
                    return false;
                }

                if (numberOfCrossOvers % 2 == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ShapesIntersect(IList<Vector2> a, IList<Vector2> b)
        {
            int aSize = a.Count;
            int bSize = b.Count;

            for (int ax = 0; ax < aSize; ax++)
            {
                Vector2 p0 = a[ax];
                int ay = ax < aSize - 1 ? ax + 1 : 0;
                Vector2 p1 = a[ay];
                for (int bx = 0; bx < bSize; bx++)
                {
                    Vector2 p2 = b[bx];
                    int by = bx < bSize - 1 ? bx + 1 : 0;
                    Vector2 p3 = b[@by];

                    if (Lines.FastLineIntersection(p0, p1, p2, p3))
                    {
                        return true;
                    }
                }
            }

            if (PointInside(a[0], b))
            {
                return true;
            }

            if (PointInside(b[0], a))
            {
                return true;
            }

            return false;
        }

        public static bool IsPointInsidePolygon(Vector2 point, IList<Vector2> points)
        {
            int pointCount = points.Count;

            if (pointCount < 3)
            {
                return false;
            }

            var it = 0;
            Vector2 first = points[it];
            var oddNodes = false;

            for (var i = 0; i < pointCount; i++)
            {
                Vector2 node1 = points[it];
                it++;
                Vector2 node2 = i == pointCount - 1 ? first : points[it];

                float x = point.x;
                float y = point.y;

                if (node1.y < y && node2.y >= y || node2.y < y && node1.y >= y)
                {
                    if (node1.x + (y - node1.y) / (node2.y - node1.y) * (node2.x - node1.x) < x)
                    {
                        oddNodes = !oddNodes;
                    }
                }
            }

            return oddNodes;
        }

        public static bool IsShapeInsidePolygonSimple(IList<Vector2> inside, IList<Vector2> shape)
        {
            int insideCount = inside.Count;
            for (int i = 0; i < insideCount; i++)
            {
                if (!IsPointInsidePolygon(inside[i], shape))
                {
                    return false;
                }
            }

            return true;
        }

        public static Vector2[] Simplify(IList<Vector2> shape, float epsilon = 0.1f)
        {
            int shapeSize = shape.Count;

            List<Vector2> newOuter = new(shapeSize);
            for (int i = 0; i < shapeSize; i++)
            {
                Vector2 a = shape[i];
                Vector2 b = shape[i < shapeSize - 1 ? i + 1 : 0];

                if (a != b)
                {
                    newOuter.Add(a);
                }
            }

            shape = newOuter.ToArray();
            shapeSize = shape.Count;

            if (shapeSize < 4)
            {
                return shape.ToArray();
            }

            newOuter.Clear();
            newOuter.Add(shape[0]);
            for (int i = 1; i < shapeSize; i++)
            {
                Vector2 a = shape[i - 1];
                Vector2 b = shape[i];
                Vector2 c = shape[i < shapeSize - 1 ? i + 1 : 0];

                if (!Lines.Collinear(a, b, c, epsilon))
                {
                    newOuter.Add(b);
                }
            }

            Vector2 ax = shape[shapeSize - 1];
            Vector2 bx = shape[0];
            Vector2 cx = shape[1];
            if (Lines.Collinear(ax, bx, cx, epsilon))
            {
                newOuter.RemoveAt(0);
            }

            return newOuter.ToArray();
        }

        public static Vector2Fixed[] Simplify(Vector2Fixed[] shape, float epsilon = 0.1f)
        {
            int shapeSize = shape.Length;
            if (shapeSize < 4)
            {
                return shape;
            }

            List<Vector2Fixed> newOuter = new(shapeSize);
            newOuter.Add(shape[0]);
            for (int i = 1; i < shapeSize; i++)
            {
                Vector2 a = shape[i - 1].vector2;
                Vector2 b = shape[i].vector2;
                Vector2 c = shape[i < shapeSize - 1 ? i + 1 : 0].vector2;

                if (!Lines.Collinear(a, b, c, epsilon))
                {
                    newOuter.Add(shape[i]);
                }
            }

            Vector2 ax = shape[shapeSize - 1].vector2;
            Vector2 bx = shape[0].vector2;
            Vector2 cx = shape[1].vector2;
            if (Lines.Collinear(ax, bx, cx, epsilon))
            {
                newOuter.RemoveAt(0);
            }

            return newOuter.ToArray();
        }
        
        public static Vector2 GetCentroid(IEnumerable<Vector2> poly)
        {
            float accumulatedArea = 0.0f;
            float centerX = 0.0f;
            float centerY = 0.0f;
            
            Vector2 pointX = poly.Last();
            foreach (Vector2 point in poly)
            {
                float temp = point.x * pointX.y - pointX.x * point.y;
                accumulatedArea += temp;
                centerX += (point.x + pointX.x) * temp;
                centerY += (point.y + pointX.y) * temp;
                pointX = point;
            }
            if (Mathf.Abs(accumulatedArea) < 1E-7f)
            {
                return Vector2.zero;  // Avoid division by zero
            }

            accumulatedArea *= 3f;
            return new Vector2(centerX / accumulatedArea, centerY / accumulatedArea);
        }
        
        public static Vector2 GetCentroid(IEnumerable<Vector3> poly)
        {
            float accumulatedArea = 0.0f;
            float centerX = 0.0f;
            float centerY = 0.0f;
            
            Vector3 pointX = poly.Last();
            foreach (Vector3 point in poly)
            {
                float temp = point.x * pointX.z - pointX.x * point.z;
                accumulatedArea += temp;
                centerX += (point.x + pointX.x) * temp;
                centerY += (point.z + pointX.z) * temp;
                pointX = point;
            }
            if (Mathf.Abs(accumulatedArea) < 1E-7f)
            {
                return Vector2.zero;  // Avoid division by zero
            }

            accumulatedArea *= 3f;
            return new Vector2(centerX / accumulatedArea, centerY / accumulatedArea);
        }

        /// <summary>
        /// Calculate the nearest point in a shape given a point
        /// </summary>
        /// <param name="toPoint">Point to measure from</param>
        /// <param name="points">Shape to measure against</param>
        /// <param name="returnNumber">Number of points to return, in order of nearest first</param>
        /// <returns></returns>
        public static Vector2[] NearestPoints(Vector2 toPoint, IList<Vector2> points, int returnNumber) {
            int[] indicies = NearestPointsIndicies(toPoint, points, returnNumber);
            Vector2[] output = new Vector2[returnNumber];
            for (int r = 0; r < returnNumber; r++)
            {
                output[r] = points[indicies[r]];
            }

            return output;
        }

        /// <summary>
        /// Return the nearest point indices of a shape to a specific point
        /// </summary>
        /// <param name="toPoint">Point to measure from</param>
        /// <param name="points">Shape to measure against</param>
        /// <param name="returnNumber">Number of indicies to return, in order of nearest first</param>
        /// <returns></returns>
        public static int[] NearestPointsIndicies(Vector2 toPoint, IList<Vector2> points, int returnNumber) {
            int pointCount = points.Count;
            float[] sqrD = new float[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                sqrD[i] = (points[i] - toPoint).sqrMagnitude;
            }

            int[] output = new int[returnNumber];
            for (int r = 0; r < returnNumber; r++) {
                float minDist = float.PositiveInfinity;
                int index = 0;
                for (int i = 0; i < pointCount; i++) {
                    if (sqrD[i] < Numbers.Epsilon)
                    {
                        continue;//don't consider the same point man!
                    }

                    if (minDist > sqrD[i]) {
                        index = i;
                        minDist = sqrD[i];
                    }
                }
                sqrD[index] = float.PositiveInfinity;//make selection unselectedble in next round
                output[r] = index;
            }

            return output;
        }
        
        public static Vector2[] OrderPoints(IList<Vector2> shape)
        {
            int pointCount = shape.Count;
            if(pointCount < 3)
            {
                return shape.ToArray();
            }

            AABBox bounds = new(shape);
            Vector2 center = bounds.center;
            float[] angles = new float[pointCount];
            Vector2[] output = new Vector2[pointCount];

            for (int i = 0; i < pointCount; i++)
            {
                Vector2 dir = (shape[i] - center).normalized;
                float angle = Vector2.Angle(Vector2.up, dir);
                Vector3 dirV3 = new(dir.x, dir.y, 0);
                Vector3 cross = Vector3.Cross(Vector3.up, dirV3);
                if (cross.z > 0)
                {
                    angle = -angle;
                }

                angles[i] = angle;
            }

            int orderedCount = 0;
            while (orderedCount < pointCount)
            {
                int index = 0;
                float minAngle = Mathf.Infinity;
                for (int i = 0; i < pointCount; i++)
                {
                    if (angles[i] < minAngle)
                    {
                        index = i;
                        minAngle = angles[i];
                    }
                }

                output[orderedCount] = shape[index];
                angles[index] = Mathf.Infinity;
                orderedCount++;
            }

            return output;
        }

        public static bool Intersects(IList<Vector2> shape)
        {
            int size = shape.Count;
            for (int i = 0; i < size; i++)
            {
                int ib = i < size - 1 ? i + 1 : 0;

                Vector2 p0 = shape[i];
                Vector2 p1 = shape[ib];
                
                for (int j = ib + 1; j < size; j++)
                {
                    int jb = j < size - 1 ? j + 1 : 0;

                    Vector2 p2 = shape[j];
                    Vector2 p3 = shape[jb];

                    if (Segments.FastIntersection(p0, p1, p2, p3))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        public static bool Intersects(IList<Vector2> a, IList<Vector2> b)
        {
            
            //add AABB check to speed up
            
            int aSize = a.Count;
            int bSize = b.Count;

            for (int ax = 0; ax < aSize; ax++)
            {
                Vector2 p0 = a[ax];
                int ay = ax < aSize - 1 ? ax + 1 : 0;
                Vector2 p1 = a[ay];
                for (int bx = 0; bx < bSize; bx++)
                {
                    Vector2 p2 = b[bx];
                    int by = bx < bSize - 1 ? bx + 1 : 0;
                    Vector2 p3 = b[by];

                    if (Lines.FastLineIntersection(p0, p1, p2, p3))
                    {
                        return true;
                    }
                }
            }
            
            if (PointInside(a[0], b))
            {
                return true;
            }

            if (PointInside(b[0], a))
            {
                return true;
            }

            return false;
        }
        
        public static Vector2[] SortPointsByDistanceFromCenter(IList<Vector2> polygon)
        {
            // Calculate the center point of the polygon
            AABBox bounds = new (polygon);
            Vector2 center = bounds.center;

            // Create a list of tuples containing each point and its distance from the center
            List<Tuple<Vector2, float>> pointsWithDistances = new();
            for (int i = 0; i < polygon.Count; i++)
            {
                float distance = Vector2.Distance(polygon[i], center);
                pointsWithDistances.Add(new Tuple<Vector2, float>(polygon[i], distance));
            }

            // Sort the list of tuples by distance from the center
            pointsWithDistances.Sort((a, b) => a.Item2.CompareTo(b.Item2));

            // Convert the sorted list of tuples back into an array of points
            Vector2[] sortedPoints = new Vector2[polygon.Count];
            for (int i = 0; i < sortedPoints.Length; i++)
            {
                sortedPoints[i] = pointsWithDistances[i].Item1;
            }

            return sortedPoints;
        }

        public static Vector2[] SplitShape(IList<Vector2> input, float minimumLength)
        {
            int size = input.Count;
            int outputCount = 0;
            for (int i = 0; i < size; i++)
            {
                int ib = i < size - 1 ? i + 1 : 0;
                Vector2 p0 = input[i];
                Vector2 p1 = input[ib];
                float distance = Vector2.Distance(p0, p1);
                outputCount += Mathf.FloorToInt(distance / minimumLength);
            }
            Vector2[] output = new Vector2[outputCount];
            int baseIndex = 0;
            for (int i = 0; i < size; i++)
            {
                int ib = i < size - 1 ? i + 1 : 0;
                Vector2 p0 = input[i];
                Vector2 p1 = input[ib];
                Vector2 vector = p1 - p0;
                Vector2 direction = vector.normalized;
                float distance = Vector2.Distance(p0, p1);
                int splitCount = Mathf.FloorToInt(distance / minimumLength);
                for (int j = 0; j < splitCount; j++)
                {
                    if (j == splitCount - 1)
                    {
                        output[baseIndex + j] = p1;
                    }
                    else
                    {
                        output[baseIndex + j] = p0 + direction * (j + 1) / splitCount;
                    }
                }

                baseIndex += splitCount;
            }

            return output;
        }
    }
}