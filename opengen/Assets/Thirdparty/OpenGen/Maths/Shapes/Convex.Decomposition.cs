using System.Collections.Generic;
using opengen.types;
using UnityEngine;

namespace opengen.maths.shapes
{
	public partial class Convex
	{
		public static List<List<Vector2>> Decompose2(IList<Vector2> points)
		{
			List<List<Vector2>> output = new ();
			int pointCount = points.Count;
            
			if (pointCount == 3)
			{
				output.Add(new List<Vector2>()); //three points will always be convex
				output[0].AddRange(points);
				return output;
			}

			if (pointCount <= 3) //at this point we can terminate - either it's a triangle or less
			{
				return output;
			}
            
			if (ShapeUtil.SelfIntersectionCheck(points))
			{
				Debug.LogWarning("Currently unable to decompose shapes that intersect themselves");
				return output;
				//TODO split the shape into shapes that do not intersect
				// (more complex than initially sounds)
			}

			List<Vector2> clockwisePoints = new List<Vector2>(points);
			if (!Clockwise.Check(clockwisePoints))
			{
				clockwisePoints.Reverse();
			}

			if (IsConvex(points))
			{
				output.Add(new List<Vector2>()); // return input if it's already convex
				output[0].AddRange(points);
				return output;
			}

			List<int> residualShapes = new List<int>();
			residualShapes.Add(0);
			residualShapes.Add(pointCount);

			int maxIt = pointCount;
			
			while (residualShapes.Count > 0)
			{
				int startIndex = residualShapes[0];
				int endIndex = residualShapes[1];
				Debug.Log(startIndex+" "+endIndex);
				int diff = endIndex - startIndex;
				residualShapes.RemoveRange(0, 2);

				if (diff < 2)
				{
					continue;
				}

				List<Vector2> newShape = new ();
				newShape.Add(clockwisePoints[startIndex]);
				newShape.Add(clockwisePoints[(startIndex + 1) % pointCount]);
				
				// if the residual shape is a triangle, we can just add it to the output
				if (diff == 2)
				{
					newShape.Add(clockwisePoints[endIndex % pointCount]);
					output.Add(newShape);
					continue;
				}
				
				int residualGap0 = -1;
				Vector2[] newTriangle = new Vector2[3];
				
				for (int i = startIndex + 2; i < pointCount; i++)
				{
					Vector2 p0 = newShape[newShape.Count - 2];
					Vector2 p1 = newShape[newShape.Count - 1];
					Vector2 p2 = clockwisePoints[i];
					Vector2 pFirst = newShape[0];
					Vector2 pSecond = newShape[1];
					float cross = Vector2Extended.Cross(p0, p1, p2);
					bool isConvex = cross <= 0;

					if (isConvex && newShape.Count > 2)
					{
						//check cross against start of shape
						float crossR1 = Vector2Extended.Cross(p1, p2, pFirst);
						float crossR2 = Vector2Extended.Cross(p2, pFirst, pSecond);
						isConvex = crossR1 <= 0 && crossR2 <= 0;
					}

					if (isConvex && newShape.Count > 2)
					{
						//check cross against start of shape
						float crossR = Vector2Extended.Cross(p1, p2, pFirst);
						isConvex = crossR <= 0;
					}

					//check if the additional point is encapsulating future points in the shape
					bool pointEncapsulated = false;
					if (isConvex)
					{
						newTriangle[0] = p1;
						newTriangle[1] = p2;
						newTriangle[2] = pFirst;
						for (int j = i + 1; j < pointCount; j++)
						{
							Vector2 pX = clockwisePoints[j];
							if (Shapes.PointInside(pX, newTriangle))
							{
								pointEncapsulated = true;
								break;
							}
						}
					}
					
					//need to check against self intersection
					bool selfIntersects = false;
					if (isConvex && !pointEncapsulated)
					{
						newTriangle[0] = p1;
						newTriangle[1] = p2;
						newTriangle[2] = pFirst;
						
						if(Shapes.Intersects(clockwisePoints, p1, p2))
						{
							selfIntersects = true;
						}
						else if(Shapes.Intersects(clockwisePoints, pFirst, p2))
						{
							selfIntersects = true;
						}
					}

					//Debug.Log(i+" "+isConvex+" "+pointEncapsulated);

					//Debug.Log(cross);
					if (isConvex && !pointEncapsulated && !selfIntersects)
					{
						newShape.Add(p2);

						if (residualGap0 != -1) // close residual shape
						{
							residualShapes.Add(residualGap0);
							residualShapes.Add(i + 1);
							//Debug.Log(residualGap0+" "+i);
						}
						residualGap0 = -1;
					}
					else
					{
						if (residualGap0 == -1)
						{
							residualGap0 = i - 1;
							//Debug.Log(residualGap0);
						}
					}
					
					if(i == endIndex - 1)
					{
						if (residualGap0 != -1) // close residual shape
						{
							residualShapes.Add(residualGap0);
							residualShapes.Add(endIndex);
							//Debug.Log(residualGap0+" "+endIndex);
						}
						break;
					}
				}
				
				output.Add(newShape);
				
				maxIt--;
				if (maxIt <= 0)
				{
					Debug.LogWarning("Max iterations reached");
					break;
				}
			}

			return output;
		}
	}
}