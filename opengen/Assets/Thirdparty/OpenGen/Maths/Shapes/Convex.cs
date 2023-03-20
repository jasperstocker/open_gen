using System.Collections.Generic;
using opengen.types;
using UnityEngine;
using System;
using Enumerable = System.Linq.Enumerable;

namespace opengen.maths.shapes
{
	public class Convex
	{
		public static bool IsConvex(IList<Vector2> points)
		{
			int count = points.Count;
			float previous = 0;
			for (int i = 0; i < count; i++)
			{
				Vector2 p0 = points[i];
				Vector2 p1 = points[(i + 1) % count];
				Vector2 p2 = points[(i + 2) % count];

				float current = Vector2Extended.Cross(p0, p1, p2);
 
				// If curr is not equal to 0
				if (current != 0) {
 
					// If direction of cross product of
					// all adjacent edges are not same
					if (current * previous < 0) {
						return false;
					}
					
					// ELSE Update curr
					previous = current;
				}
			}
			return true;
		}
		
		private static Vector2 pointMinRect;
        private static Vector2 pointMaxRect;

        /// <summary>
        /// Decompose a defined input polygon shape into smaller shapes that are convex
        /// </summary>
        public static List<List<Vector2>> Decompose(IList<Vector2> points)
        {
            // Debug.Log("execute "+points.Length);
            List<List<Vector2>> output = new ();
            int pointCount = points.Count;
            
            if (pointCount == 3)
            {
                output.Add(new List<Vector2>()); //three points will always be convex
                output[0].AddRange(points);
            }

            if (pointCount <= 3) //at this point we can terminate - either it's a triangle or less
            {
                return output;
            }
            
            if (ShapeUtil.SelfIntersectionCheck(points))
            {
                return output;
                //TODO split the shape into shapes that do not intersect
                // (more complex than initially sounds)
            }

            if (!Clockwise.Check(points))
            {
                Array.Reverse(Enumerable.ToArray(points));
            }

            List<LinkedVector2> processShapes = new();
            processShapes.Add(LinkedVector2.LinkedList(points));

            int maximumIterations = pointCount-2;
            while (processShapes.Count > 0)
            {
                LinkedVector2 pointsLink = processShapes[0];
                processShapes.RemoveAt(0);
                
                List<LinkedVector2> pointsList = LinkedVector2.BuildFlatList(pointsLink);
                int processShapeSize = pointsList.Count;
                List<Vector2> vectorList = LinkedVector2.Flatten(pointsLink);
                
                List<int> convexIndexShape = CalculateConvexShape(vectorList);
                int convexShapeSize = convexIndexShape.Count;
                
                for (int i = 0; i < convexShapeSize; i++)
                {
                    int i1 = i < convexShapeSize - 1 ? i + 1 : 0;
                    int i2 = (i + 2) % convexShapeSize;
                    int ix1 = convexIndexShape[i1];
                    int ix2 = convexIndexShape[i2];
                    int diffB = Math.Abs(ix2 - ix1);
                    bool adjacentB = diffB == 1 || diffB == processShapeSize - 1;
                    if (!adjacentB)
                    {
                        LinkedVector2 lv0 = pointsList[ix1];
                        LinkedVector2 lv1 = pointsList[ix2];
                        LinkedVector2 newLinkedShape = LinkedVector2.SplitPolygon(lv0, lv1);
                        processShapes.Add(newLinkedShape);
                    }
                }

                List<Vector2> convexOutput = new (convexShapeSize);
                for (int i = 0; i < convexShapeSize; i++)
                {
                    convexOutput.Add(vectorList[convexIndexShape[i]]);
                }
                output.Add(convexOutput);

                maximumIterations--;
                if(maximumIterations==0)
                {
                    break;
                }
            }
            
            return output;
        }

        public static List<int> CalculateConvexShape(IList<Vector2> input)
        {
            int pointCount = input.Count;

            Vector2 baseVector = input[1] - input[0];
            Vector2 baseDir = baseVector.normalized;
            Vector2 baseNorm = baseDir.Rotate90Clockwise();
            
            List<int> output = new ();
            output.Add(0);
            output.Add(1);
            int outputSize = 2;
            
            for (int p = 2; p < pointCount; p++)
            {
                Vector2 x0 = input[output[outputSize - 2]];
                Vector2 x1 = input[output[outputSize - 1]];
                Vector2 x2 = input[p];

                Vector2 pVector = x2 - input[0];
                Vector2 pDir = pVector.normalized;
                if (Vector2.Dot(pDir, baseNorm) > 0)
                {
                    continue; //ignore points that fall behind the initial line
                }

                float cross = Vector2Extended.Cross(x0, x1, x2);
                if (cross < 0)
                {
                    continue; //ignore concave lines
                }

                int internalPointIndex = -1;
                output.Add(p);//add p and check it's okay
                outputSize++;
                for (int j = p + 1; j < pointCount; j++)
                {
                    if(output.Contains(j))
                    {
                        continue;
                    }

                    Vector2 shapePoint = input[j];
                    if (Shapes.PointInside(shapePoint, input, output))
                    {
                        internalPointIndex = j;
                        break;
                    }
                }

                if (internalPointIndex != -1)
                {
                    output.RemoveAt(outputSize-1);//remove it
                    outputSize--;
                }
                
                if (internalPointIndex > p)// a point would be within this shape - jump to this point and try to add it
                {
                    p = internalPointIndex - 1;
                }
            }

            return output;
        }
        public static List<Vector2> MakeHull(List<Vector2> points)
        {
            List<Point> input = new();
            for(int p = 0; p < points.Count; p++)
            {
                input.Add(new Point(points[p].x, points[p].y));
            }

            List<Point> outputHull = MakeHull(input);
            List<Vector2> output = new();
            for(int p = 0; p < outputHull.Count; p++)
            {
                output.Add(new Vector2(outputHull[p].x, outputHull[p].y));
            }

            return output;
        }

        public static Vector2[] MakeHull(Vector2[] points)
        {
            List<Point> input = new();
            for(int p = 0; p < points.Length; p++)
            {
                input.Add(new Point(points[p].x, points[p].y));
            }

            List<Point> outputHull = MakeHull(input);
            Vector2[] output = new Vector2[outputHull.Count];
            for(int p = 0; p < outputHull.Count; p++)
            {
                output[p] = new Vector2(outputHull[p].x, outputHull[p].y);
            }

            return output;
        }

        public static Vector2Fixed[] MakeHull(Vector2Fixed[] points)
        {
            List<Point> input = new();
            for(int p = 0; p < points.Length; p++)
            {
                input.Add(new Point(points[p].vx, points[p].vy));
            }

            List<Point> outputHull = MakeHull(input);
            Vector2Fixed[] output = new Vector2Fixed[outputHull.Count];
            for(int p = 0; p < outputHull.Count; p++)
            {
                output[p] = new Vector2Fixed(outputHull[p].x, outputHull[p].y);
            }

            return output;
        }

        // Returns a new list of points representing the convex hull of
        // the given set of points. The convex hull excludes collinear points.
        // This algorithm runs in O(n log n) time.
        public static List<Point> MakeHull(List<Point> points)
        {
            List<Point> newPoints = new(points);
            newPoints.Sort();
            return MakeHullPresorted(newPoints);
        }


        // Returns the convex hull, assuming that each points[i] <= points[i + 1]. Runs in O(n) time.
        public static List<Point> MakeHullPresorted(List<Point> points)
        {
            if(points.Count <= 1)
            {
                return new List<Point>(points);
            }

            // Andrew's monotone chain algorithm. Positive y coordinates correspond to "up"
            // as per the mathematical convention, instead of "down" as per the computer
            // graphics convention. This doesn't affect the correctness of the result.

            List<Point> upperHull = new();
            foreach(Point p in points)
            {
                while(upperHull.Count >= 2)
                {
                    Point q = upperHull[upperHull.Count - 1];
                    Point r = upperHull[upperHull.Count - 2];
                    if((q.x - r.x) * (p.y - r.y) >= (q.y - r.y) * (p.x - r.x))
                    {
                        upperHull.RemoveAt(upperHull.Count - 1);
                    }
                    else
                    {
                        break;
                    }
                }

                upperHull.Add(p);
            }

            upperHull.RemoveAt(upperHull.Count - 1);

            List<Point> lowerHull = new();
            for(int i = points.Count - 1; i >= 0; i--)
            {
                Point p = points[i];
                while(lowerHull.Count >= 2)
                {
                    Point q = lowerHull[lowerHull.Count - 1];
                    Point r = lowerHull[lowerHull.Count - 2];
                    if((q.x - r.x) * (p.y - r.y) >= (q.y - r.y) * (p.x - r.x))
                    {
                        lowerHull.RemoveAt(lowerHull.Count - 1);
                    }
                    else
                    {
                        break;
                    }
                }

                lowerHull.Add(p);
            }

            lowerHull.RemoveAt(lowerHull.Count - 1);

            if(!(upperHull.Count == 1 && Enumerable.SequenceEqual(upperHull, lowerHull)))
            {
                upperHull.AddRange(lowerHull);
            }

            return upperHull;
        }
        
        public struct Point : IComparable<Point>
        {

            public float x;
            public float y;


            public Point(float x, float y)
            {
                this.x = x;
                this.y = y;
            }


            public int CompareTo(Point other)
            {
                if(x < other.x)
                {
                    return -1;
                }
                else if(x > other.x)
                {
                    return +1;
                }
                else if(y < other.y)
                {
                    return -1;
                }
                else if(y > other.y)
                {
                    return +1;
                }
                else
                {
                    return 0;
                }
            }

        }
	}
}