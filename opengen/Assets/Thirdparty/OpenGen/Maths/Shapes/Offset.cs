using System.Collections.Generic;
using opengen.types;
using UnityEngine;

namespace opengen.maths.shapes
{
    /// <summary>
    /// Generate an offset of the provided shape
    ///
    /// TODO support holes
    /// </summary>
    
    public class Offset
    {
        public static List<Vector2[]> Execute(Shape input, float offset, int signCheck = 0)
        {
            return Execute(input.pointList, offset, signCheck);
        }
        
        public static List<Vector2[]> Execute(IList<Vector2> input, float offset, int signCheck = 0)
        {
            List<Vector2[]> output = new ();
            
            Vector2[][] intersectionCheck = SelfIntersectCheck(input);
            Vector2[] nonIntersectingShape = intersectionCheck[0];

            int offsetSign = 1;
            
            if (signCheck == 0)
            {
                signCheck = ShapeSign(nonIntersectingShape);
            }
            else if (signCheck != ShapeSign(nonIntersectingShape))
            {
                offsetSign = -1;
            }

            Vector2[] offsetShape = ExecuteIntersection(nonIntersectingShape, offsetSign * offset);
            Vector2[][] offsetIntersectionCheck = SelfIntersectCheck(offsetShape);
            
            output.Add(offsetIntersectionCheck[0]);

            //pass on residual intersecting shapes to be recused...
            if (intersectionCheck[1].Length > 0)
            {
                output.AddRange(Execute(intersectionCheck[1], offset, signCheck));
            }

            //offset shapes that have intersected - recurse them!
            //shapes are offset already = so set offset to zero!
            if (offsetIntersectionCheck[1].Length > 0)
            {
                output.AddRange(Execute(offsetIntersectionCheck[1], 0, signCheck));
            }

            return output;
        }
        
        public static Vector2[] ExecuteIntersection(IList<Vector2> points, float offset)
        {
            int pointCount = points.Count;
            Vector2[] offsetLines = new Vector2[pointCount * 2];
            //calculate the new offset lines
            float sqrMag = 0;
            for (int p = 0; p < pointCount; p++)
            {
                int pb = p < pointCount - 1 ? p + 1 : 0;
                Vector2 a = points[p];
                Vector2 b = points[pb];
                Vector2 vector = b - a;
                sqrMag = vector.sqrMagnitude;
                if (sqrMag > Mathf.Epsilon)
                {
                    Vector2 direction = vector.normalized;
                    Vector2 normal = direction.Rotate90Clockwise();
                    offsetLines[p * 2] = a + normal * offset;
                    offsetLines[p * 2 + 1] = b + normal * offset;
                }
                else
                {
                    offsetLines[p * 2] = a;
                    offsetLines[p * 2 + 1] = b;
                }
            }

            //calculate the intersection points of the offset lines
            Vector2[] offsetShape = new Vector2[pointCount];
            for (int p = 0; p < pointCount; p++)
            {
                //offsetShape[p] = points[p];
                int pb = p < pointCount - 1 ? p + 1 : 0;
                Vector2 a1 = offsetLines[p * 2];
                Vector2 a2 = offsetLines[p * 2 + 1];
                Vector2 b1 = offsetLines[pb * 2];
                Vector2 b2 = offsetLines[pb * 2 + 1];

                if (Lines.Intersects(a1, a2, b1, b2, out Vector2 intersection))
                {
                    offsetShape[pb] = intersection;
                }
            }

            return offsetShape;
        }
        
        public static Vector2[] ExecuteIntersectionSingleSide(IList<Vector2> points, int side, float offset)
        {
            int pointCount = points.Count;
            Vector2[] offsetShape = new Vector2[pointCount];
            
            int i0 = side > 0 ? side - 1 : pointCount - 1;
            int i1 = side;
            int i2 = side < pointCount - 1 ? side + 1 : 0;
            int i3 = side < pointCount - 2 ? side + 2 : side + 2 - pointCount;
            
            Vector2 p0 = points[i0];
            Vector2 p1 = points[i1];
            Vector2 p2 = points[i2];
            Vector2 p3 = points[i3];
            
            Vector2 vector01 = p1 - p0;
            Vector2 directionP01 = vector01.normalized;
            Vector2 normal01 = directionP01.Rotate90Clockwise();
            
            Vector2 l01p0 = p0 + normal01 * offset;
            Vector2 l01p1 = p1 + normal01 * offset;
            
            Vector2 vector12 = p2 - p1;
            Vector2 direction12 = vector12.normalized;
            Vector2 normal12 = direction12.Rotate90Clockwise();
            
            Vector2 l12p1 = p1 + normal12 * offset;
            Vector2 l12p2 = p2 + normal12 * offset;
            
            Vector2 vector23 = p3 - p2;
            Vector2 direction23 = vector23.normalized;
            Vector2 normal23 = direction23.Rotate90Clockwise();
            
            Vector2 l23p2 = p2 + normal23 * offset;
            Vector2 l23p3 = p3 + normal23 * offset;
            
            if (Lines.Intersects(l01p0, l01p1, l12p1, l12p2, out Vector2 intersection01))
            {
                offsetShape[i1] = intersection01;
            }
            
            if (Lines.Intersects(l12p1, l12p2, l23p2, l23p3, out Vector2 intersection12))
            {
                offsetShape[i2] = intersection12;
            }

            return offsetShape;
        }

        private static Vector2[][] SelfIntersectCheck(IList<Vector2> input)
        {
            int pointCount = input.Count;
            List<Vector2> newShape = new(pointCount);
            bool[] used = new bool[pointCount];
            int outputSize = 0;

            for (int x = 0; x < pointCount; x++)
            {
                int xb = x < pointCount - 1 ? x + 1 : 0;
                Vector2 a1 = input[x];
                Vector2 a2 = input[xb];
                newShape.Add(a1);
                used[x] = true;
                outputSize++;

                int intersectionIndex = -1;
                Vector2 intersectionPoint = Vector2.zero;
                float sqrMag = float.MaxValue;
                for (int y = x; y < pointCount; y++)
                {
                    if (x == y || xb == y)
                    {
                        continue;
                    }

                    int yb = y < pointCount - 1 ? y + 1 : 0;
                    if (x == yb || xb == yb)
                    {
                        continue;
                    }

                    Vector2 b1 = input[y];
                    Vector2 b2 = input[yb];

                    if (Segments.FastIntersection(a1, a2, b1, b2))
                    {
                        Vector2 intersection = Segments.IntersectionPoint(a1, a2, b1, b2);
                        float intersectionSqrMag = (intersection - a1).sqrMagnitude;
                        if (intersectionSqrMag < sqrMag)
                        {
                            sqrMag = intersectionSqrMag;
                            intersectionIndex = y;
                            intersectionPoint = intersection;
                        }
                    }
                }

                //if there was an intersection
                if (intersectionIndex != -1)
                {
                    newShape.Add(intersectionPoint);
                    // Vector3 v0 = new Vector3(intersectionPoint.x, 0, intersectionPoint.y);
                    // Debug.DrawLine(v0, v0+Vector3.up*100, Color.red, 1);
                    x = intersectionIndex;//skip ahead to the next index
                    // Debug.Log(intersectionIndex);
                }
            }

            int residualSize = pointCount - outputSize;
            Vector2[][] output = new Vector2[2][];
            output[0] = newShape.ToArray();
            output[1] = new Vector2[residualSize];
            int residualCount = 0;
            for (int i = 0; i < pointCount; i++)
            {
                if(used[i])
                {
                    continue;
                }

                output[1][residualCount] = input[i];
                residualCount++;
            }

            return output;
        }

        private static int ShapeSign(Vector2[] points)
        {
            if (Clockwise.Check(points))
            {
                return 1;
            }

            return -1;
        }
    }
}