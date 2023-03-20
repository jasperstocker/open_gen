using System;
using System.Collections.Generic;
using System.Linq;
using opengen.types;
using UnityEngine;

namespace opengen.maths.shapes
{
	public static partial class ShapeSplit
	{
		/// <summary>
		/// Split an input polygon into a series of rows defined by width and along specified angle
		/// Assumption that input is convex
		/// </summary>
		public static List<Vector2[]> RowDivision(IList<Vector2> input, float sliceWidth, float angle = 0)
		{
			// Debug.Log($"row div {input.Count} {sliceWidth} {angle}");
			List<Vector2[]> output = new ();
			OBBox obBox = OBBox.Fit(input, angle);
			float length = obBox.longSize;
			int sliceCount = Mathf.CeilToInt(length / sliceWidth);
			// Debug.Log($"sliceCount {sliceCount}");
			// Debug.Log($"obBox {obBox.size} {obBox.center}");
			// Debug.Log($"input Area {Area.Calculate(input)}");

			IList<Vector2> residualShape = input;
			int direction = 0;// default to zero until set
			for (int i = 0; i < sliceCount - 1; i++)
			{
				float slicePoint = sliceWidth * (i + 1);
				float sliceOffset = slicePoint / length;
				// Debug.Log($"slice sliceOffset {sliceOffset}");
				Vector2[][] splits = OBBSquareSlice(residualShape, obBox, sliceOffset);
				int shapeCount = splits.Length;
				// Debug.Log($"shapeCount {shapeCount}");
				// if (shapeCount == 1)
				// {
				// 	float area = Area.Calculate(splits[0]);
					// Debug.Log($"single area {area}");
				// }
				if (shapeCount > 1)
				{
					Vector2[] outputShape = splits[0];
					if (outputShape.Length == 0)
					{
						continue;
					}
					
					float area;
					int lastShapeIndex = shapeCount - 1;
					int outputShapeIndex = 0;
					int residualShapeIndex = 0;
					if (direction == 0)
					{
						float area0 = Area.Calculate(splits[0]);
						float area1 = Area.Calculate(splits[lastShapeIndex]);
						if (area0 < area1)
						{
							area = area0;
							outputShapeIndex = 0;
							residualShapeIndex = lastShapeIndex;
							direction = 1;
						}
						else
						{
							area = area1;
							outputShapeIndex = lastShapeIndex;
							residualShapeIndex = 0;
							direction = -1;
						}
					}
					else
					{
						outputShapeIndex = direction == 1 ? 0 : lastShapeIndex;
						residualShapeIndex = direction == 1 ? lastShapeIndex : 0;
						area = Area.Calculate(splits[outputShapeIndex]);
					}
					
					// Debug.Log($"output area {area}");
					if (area > 1)
					{
						output.Add(splits[outputShapeIndex]);
					}
					
					residualShape = splits[residualShapeIndex];
					// Debug.Log($"residualShape Area {Area.Calculate(splits[shapeCount-1])}");
				}
			}
			
			output.Add(residualShape.ToArray());

			// Debug.Log($"output count {output.Count}");

			return output;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input">Needs to be a convex poly</param>
		/// <param name="intersectionLine0">A point on one side of the polygon</param>
		/// <param name="intersectionLine1">A point on the other side</param>
		/// <param name="snapPoints">Merge points if they produce edges that are less than this value</param>
		/// <returns></returns>
		public static Vector2[][] Slice(IList<Vector2> input, Vector2 intersectionLine0, Vector2 intersectionLine1, float snapPoints = 0)
		{
			int polySize = input.Count;
			bool isPointSnapping = snapPoints > Numbers.Epsilon;

			int intersectionCount = 0;
			Vector2 int0 = Vector2.zero, int1 = Vector2.zero;//intersection points
			int index0 = 0, index1 = 0;
			bool int0Snap = false, int1Snap = false;
			bool shape0Snap = false, shape1Snap = false;
			//find the points where in the input intersection slice hits the shape sides
			for (int pl = 0; pl < polySize; pl++)
			{
				Vector2 pl0 = input[pl];
				int plb = pl < polySize - 1 ? pl + 1 : 0;
				Vector2 pl1 = input[plb];

				if (Segments.Intersects(intersectionLine0, intersectionLine1, pl0, pl1))
				{
					switch (intersectionCount)
					{
						case 0:
							int0 = Segments.FindIntersection(intersectionLine0, intersectionLine1, pl0, pl1);
							index0 = pl;
							intersectionCount++;

							if (isPointSnapping)
							{
								float length0 = Vector2.Distance(pl0, int0);
								if (length0 < snapPoints)
								{
									int0Snap = true;
									shape0Snap = true;
									int0 = pl0;
								}
								else
								{
									float length1 = Vector2.Distance(pl1, int0);
									if (length1 < snapPoints)
									{
										int0Snap = true;
										shape1Snap = true;
										int0 = pl1;
									}
								}
							}
							
							break;
						case 1:
							int1 = Segments.FindIntersection(intersectionLine0, intersectionLine1, pl0, pl1);
							index1 = pl;
							intersectionCount++;
							
							if (isPointSnapping)
							{
								float length0 = Vector2.Distance(pl0, int1);
								if (length0 < snapPoints)
								{
									int1Snap = true;
									shape0Snap = true;
									int1 = pl0;
								}
								else
								{
									float length1 = Vector2.Distance(pl1, int1);
									if (length1 < snapPoints)
									{
										int1Snap = true;
										shape1Snap = true;
										int1 = pl1;
									}
								}
							}
							
							break;
					}

					if (intersectionCount == 2)
					{
						break;
					}
				}
			}

			//dump the output if there is no intersection across the polygon
			if (intersectionCount != 2)
			{
				Vector2[][] dumpOutput = new Vector2[1][];
				dumpOutput[0] = input.ToArray();
				return dumpOutput;
			}
            
			//BUILD THE TWO POLY SHAPES
			List<Vector2> plotA = new ();
			List<Vector2> plotB = new ();
			int it = polySize;
			int index = index1;
			int terminateAtIndex = index0;
			while (true)
			{
				index = (index + 1) % polySize;
				Vector2 pl0 = input[index];
				plotA.Add(pl0);

				if (index == terminateAtIndex)
				{
					break;
				}

				it--;
				if (it == 0)
				{
					break;
				}
			}

			if (!int0Snap)
			{
				plotA.Add(int0);
			}
			else if (shape1Snap)
			{
				int insert = (index0 + 1) % polySize;
				plotA.Add(input[insert]);
			}

			if (!int1Snap)
			{
				plotA.Add(int1);
			}
			else if (shape0Snap)
			{
				plotA.Add(input[index1]);
			}
			//plot A complete
            
			//lets build the second shape
			if (!int1Snap)
			{
				plotB.Add(int1);
			}
			else if (shape1Snap)
			{
				int insert = (index1 + 1) % polySize;
				plotB.Add(input[insert]);
			}
			
			if (!int0Snap)
			{
				plotB.Add(int0);
			}
			else if (shape0Snap)
			{
				plotB.Add(input[index0]);
			}
			//add the shape points in
			it = polySize;
			index = index0;
			terminateAtIndex = (index1 + 1) % polySize;
			while (true)
			{
				index = (index + 1) % polySize;
				Vector2 pl0 = input[index];
                
				if (index == terminateAtIndex)
				{
					break;
				}

				plotB.Add(pl0);
                
				it--;
				if (it == 0)
				{
					break;
				}
			}
			//plot B complete

			Vector2[][] output = new Vector2[2][];
			output[0] = plotA.ToArray();
			output[1] = plotB.ToArray();

			return output;
		}

		public static Vector2[][] OBBSquareSlice(IList<Vector2> input, OBBox obBox, float sliceOffset, float snapPoints = 0)
		{
			float shortSize = obBox.shortSize;
			Vector2 shortDir = obBox.shortDir;
			float longSize = obBox.longSize;
			Vector2 longDir = obBox.longDir;
			Vector2 cutCenter = obBox.center;
			if (Math.Abs(sliceOffset - 0.5f) > Numbers.Epsilon)
			{
				Vector2 cenExt = 0.5f * longSize * longDir;
				cutCenter = Vector2.Lerp(obBox.center - cenExt, obBox.center + cenExt, sliceOffset);
			}
			Vector2 intExt = shortDir * shortSize;
			Vector2 intP0 = cutCenter - intExt;
			Vector2 intP1 = cutCenter + intExt;

			return Slice(input, intP0, intP1, snapPoints);
		}

		public static Vector2[][] OBBLongSlice(IList<Vector2> input, OBBox obBox, float sliceOffset, float snapPoints = 0)
		{
			float shortSize = obBox.shortSize;
			Vector2 shortDir = obBox.shortDir;
			float longSize = obBox.longSize;
			Vector2 longDir = obBox.longDir;
			Vector2 cutCenter = obBox.center;
			if (Math.Abs(sliceOffset - 0.5f) > Numbers.Epsilon)
			{
				Vector2 cenExt = 0.5f * shortSize * shortDir;
				cutCenter = Vector2.Lerp(obBox.center - cenExt, obBox.center + cenExt, sliceOffset);
			}
			Vector2 intExt = longDir * longSize;
			Vector2 intP0 = cutCenter - intExt;
			Vector2 intP1 = cutCenter + intExt;

			return Slice(input, intP0, intP1, snapPoints);
		}
	}
}