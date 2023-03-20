using System;
using System.Collections.Generic;
using opengen.types;
using UnityEngine;

namespace opengen.maths.shapes
{
	public static partial class ShapeSplit
	{
		public static Vector2[][] SegmentSubplotDivisionDecomposed(IList<Vector2> input, float maximumArea, uint seed = 58,
			float cutVariation = 0.0f)
		{
		;
			List<SegmentShape> segmentShapes = SegmentSubplotDivision(input, maximumArea, seed, cutVariation);
			int size = segmentShapes.Count;
			Vector2[][] output = new Vector2[size][];
			for (int i = 0; i < size; i++)
			{
				output[i] = segmentShapes[i].Decompose();
			}

			return output;
		}

		public static List<SegmentShape> SegmentSubplotDivision(
			IList<Vector2> input, 
			float maximumArea, 
			uint seed = 58,
			float cutVariation = 0.0f
			)
		{
			// Debug.Log("RUN SPLOIT");
			cutVariation = Numbers.Clamp01(cutVariation);
			RandomGenerator rgen = new(seed);

			SegmentShape initialShape = new(input);
			List<SegmentShape> output = new();
			List<SegmentShape> process = new();

			// Debug.Log($"initialShape area {initialShape.area}");
			if (initialShape.area < maximumArea)
			{
				// Debug.Log("init max area");
				output.Add(initialShape);
				return output;
			}
			
			//start with input shape
			process.Add(initialShape);
			
			int maxIt = Numbers.RoundToInt(Math.Max(1, initialShape.area / maximumArea)) * 100;
			while (process.Count > 0)
			{
				// Debug.Log(" - new process (rem:"+process.Count+") ");
				SegmentShape current = process[0];
				// Debug.Log("current area "+current.area);
				process.RemoveAt(0);
				if (current.bounds.area < Mathf.Epsilon)
				{
					//invalid cut generated
					// Debug.Log("invalid cut generated");
					output.Add(current);
					continue;
				}

				float sliceOffset = cutVariation < Mathf.Epsilon ? 0.5f : rgen.Range(-cutVariation * 0.5f, cutVariation * 0.5f) + 0.5f;
				SegmentShape[] splits = current.Slice(sliceOffset);
				int splitCount = splits.Length;

				if (splitCount == 0)
				{
					//no cut generated
					// Debug.Log("no cut generated");
					output.Add(current);
					continue;
				}
				
				// Debug.Log("split count "+splitCount);
				for (int s = 0; s < splitCount; s++)
				{
					SegmentShape splitShape = splits[s];
					if(splitShape.area < maximumArea)
					{
						// Debug.Log("comp");
						output.Add(splitShape);
					}
					else
					{
						// Debug.Log("split area "+splitShape.area);
						process.Add(splitShape);
					}
				}

				maxIt--;
				if (process.Count > 0 && maxIt == 0)
				{
					// Debug.Log("MAXIT");
					// Debug.Log(initialShape.area);
					// Debug.Log(maximumArea);
					// Debug.Log(process.Count);
					//Debug.Log(Numbers.RoundToInt(Math.Max(1, initialArea / maximumArea)) * 2);
					//output.AddRange(process); //dump the process stack onto the output
					break;
				}
			}

			// Debug.Log($"complete {output.Count}");

			return output;
		}

		/*private class SplitShape
		{
			public SplitShapeSegment[] segments;
			private float _area;
			private OBBox _bounds;
			private int _size = 0;

			public float area => _area;
			public OBBox bounds => _bounds;

			public int size => _size;

			public SplitShape(IList<Vector2> input)
			{
				_size = input.Count;
				segments = new SplitShapeSegment[size];
				for (int i = 0; i < size; i++)
				{
					int ix = i < size - 1 ? i + 1 : 0;

					Vector2 a, b;
					if (i % 2 == 0)
					{
						a = input[i];
						b = input[ix];
					}
					else
					{
						b = input[i];
						a = input[ix];
					}

					SplitShapeSegment newSegment = new (a, b, this);
					segments[i] = newSegment;
				}
				
				CalculateArea();
				CalculateBounds();
			}

			public SplitShape(List<SplitShapeSegment> input, SplitShape oldShape)
			{
				Debug.Log($"new split shape new{input.Count} old{oldShape.size}");
				segments = input.ToArray();
				foreach (SplitShapeSegment segment in segments)
				{
					segment.ReplaceShapeReference(oldShape, this);
				}
				_size = segments.Length;
				CalculateArea();
				CalculateBounds();
			}

			public Vector2[] Decompose()
			{
				if (_size == 0)
				{
					return new Vector2[0];
				}
				
				Vector2[] output = new Vector2[_size];
				SplitShapeSegment previous = segments[_size - 1];
				for (int i = 0; i < _size; i++)
				{
					SplitShapeSegment current = segments[i];
					// Debug.Log(i+" "+current+" "+previous);
					output[i] = current.NextPoint(previous);
					previous = current;
				}

				return output;
			}
			
			public SplitShape[] Slice(float sliceOffset)
			{
				// Debug.Log("Slice _size "+_size);
				// for (int i = 0; i < _size; i++)
				// {
				// 	Debug.Log($"{i} {segments[i]}");
				// }
				
				float shortSize = _bounds.shortSize;
				Vector2 shortDir = _bounds.shortDir;
				float longSize = _bounds.longSize;
				Vector2 longDir = _bounds.longDir;
				Vector2 cutCenter = _bounds.center;
				if (Math.Abs(sliceOffset - 0.5f) > Numbers.Epsilon)
				{
					Vector2 cenExt = 0.5f * longSize * longDir;
					cutCenter = Vector2.Lerp(_bounds.center - cenExt, _bounds.center + cenExt, sliceOffset);
				}
				Vector2 intExt = shortDir * shortSize;
				Vector2 intP0 = cutCenter - intExt;
				Vector2 intP1 = cutCenter + intExt;

				//Debug.DrawLine(intP0.Vector3Flat(), intP1.Vector3Flat(), Color.red, 5);
				
				int intersectionCount = 0;
				Vector2 int0 = Vector2.zero, int1 = Vector2.zero;//intersection points
				SplitShapeSegment[] split0 = new SplitShapeSegment[0], split1 = new SplitShapeSegment[0];
				int index0 = 0, index1 = 0;
				//find the points where in the input intersection slice hits the shape sides
				SplitShapeSegment previous = segments[_size - 1];
				for (int i = 0; i < _size; i++)
				{
					SplitShapeSegment current = segments[i];
					if (current.Intersects(intP0, intP1, out Vector2 intersectionPoint))
					{
						switch (intersectionCount)
						{
							case 0:
								int0 = intersectionPoint;
								index0 = i;
								split0 = current.Split(intersectionPoint, previous);
								intersectionCount++;
								break;
							case 1:
								int1 = intersectionPoint;
								index1 = i;
								split1 = current.Split(intersectionPoint, previous);
								intersectionCount++;
								break;
						}
					}

					if (intersectionCount == 2)
					{
						break;
					}
					
					previous = current;
				}

				bool isShapeSplit = intersectionCount == 2;
				bool isLineSplit0 = split0.Length == 2;
				bool isLineSplit1 = split1.Length == 2;

				//dump the output if there is no intersection across the polygon
				if (!isShapeSplit || !isLineSplit0 || !isLineSplit1)
				{
					SplitShape[] dumpOutput = new SplitShape[1];
					dumpOutput[0] = this;
					return dumpOutput;
				}

				// Debug.DrawLine(split0[0].p0.Vector3Flat(), split0[0].p1.Vector3Flat(), Color.magenta, 5);
				// Debug.DrawLine(split0[1].p0.Vector3Flat(), split0[1].p1.Vector3Flat(), Color.blue, 5);
				// Debug.DrawLine(split1[0].p0.Vector3Flat(), split1[0].p1.Vector3Flat(), Color.magenta, 5);
				// Debug.DrawLine(split1[1].p0.Vector3Flat(), split1[1].p1.Vector3Flat(), Color.blue, 5);
				
				//BUILD THE TWO POLY SHAPES
				SplitShapeSegment intersectionSegment = new (int0, int1);
				
				int index = index1;
				int iterations = _size;
				
				// start at the intersection, add the split line and then add the old lines in
				List<SplitShapeSegment> plotA = new ();
				// while loop this so we can pass through index 0 if we need to
				Debug.Log(_size);
				while (iterations > 0)
				{
					index = (index + 1) % _size;
					SplitShapeSegment segment = segments[index];
					//Debug.Log($"{index} segment {segment}");

					// if (segment != null)
					// {
						plotA.Add(segment);
					// }
					
					// if we hit the intersection, stop
					if (index == index0)
					{
						break;
					}
					
					iterations--;
				}
				plotA.Add(split0[0]);
				plotA.Add(intersectionSegment);
				plotA.Add(split1[1]);
				//plot A complete
            
				//lets build the second shape
				//we build this one "backwards" to maintain new shape winding
				List<SplitShapeSegment> plotB = new ();
				//add the shape points in
				iterations = _size;
				index = index0;
				while (iterations > 0)
				{
					index = (index + 1) % _size;
                
					// if we hit the intersection, stop
					if (index == index1)
					{
						break;
					}
					
					SplitShapeSegment segment = segments[index];
					//Debug.Log($"{index} segment {segment}");

					// if (segment != null)
					// {
						plotB.Add(segment);
					// }

					iterations--;
				}
				plotB.Add(split1[0]);
				plotB.Add(intersectionSegment);
				plotB.Add(split0[1]);
				//plot B complete

				// Debug.Log("sliced lists");
				// Debug.Log("plot a "+plotA.Count);
				// for (int i = 0; i < plotA.Count; i++)
				// {
				// 	Debug.Log($"{i} {plotA[i]}");
				// }
				// Debug.Log("plot b "+plotB.Count);
				// for (int i = 0; i < plotB.Count; i++)
				// {
				// 	Debug.Log($"{i} {plotB[i]}");
				// }

				SplitShape[] output = new SplitShape[2];
				output[0] = new SplitShape(plotA, this);
				output[1] = new SplitShape(plotB, this);
				intersectionSegment.s0 = output[0];
				intersectionSegment.s1 = output[1];

				// Debug.Log("shape 0 size "+output[0].size);
				// Debug.Log("shape 1 size "+output[1].size);

				return output;
			}

			public void ReplaceSplitSegment(SplitShapeSegment old, SplitShapeSegment new0, SplitShapeSegment new1)
			{
				Debug.Log($"replace split segment old size:{_size}");
				int newSize = _size + 1;
				SplitShapeSegment[] newSegments = new SplitShapeSegment[newSize];
				int insertionIndex = 0;
				bool segmentReplaced = false;
				for (int i = 0; i < _size; i++)
				{
					if (segments[i] != old)
					{
						newSegments[insertionIndex] = segments[i];
						Debug.Log($"{insertionIndex} {i}");
						insertionIndex++;
					}
					else
					{
						segmentReplaced = true;
						int nextSegmentIndex = (i + 1) % _size;
						SplitShapeSegment nextSegment = segments[nextSegmentIndex];
						float sqrMag0 = nextSegment.SquareMagnitude(new0);
						float sqrMag1 = nextSegment.SquareMagnitude(new1);

						if (sqrMag0 > sqrMag1)
						{
							newSegments[insertionIndex] = new0;
							Debug.Log($"{insertionIndex} 00");
							insertionIndex++;
							newSegments[insertionIndex] = new1;
							Debug.Log($"{insertionIndex} 11");
							insertionIndex++;
						}
						else
						{
							newSegments[insertionIndex] = new1;
							Debug.Log($"{insertionIndex} 11");
							insertionIndex++;
							newSegments[insertionIndex] = new0;
							Debug.Log($"{insertionIndex} 00");
							insertionIndex++;
						}
					}
				}

				if (!segmentReplaced)
				{
					Debug.Log("ISSUE no segment in this shape");
				}
				
				segments = newSegments;
				_size = newSize;
				Debug.Log($"new size {newSize}");
			}

			private void CalculateArea()
			{
				Vector2[] shape = Decompose();
				_area = 0;
				for (int s = 0; s < _size; s++)
				{
					int sx = s < _size - 1 ? s + 1 : 0;
					Vector2 a = shape[s];
					Vector2 b = shape[sx];
					_area += (b.x + a.x) * (b.y - a.y);
				}
				// Return absolute value
				_area = Mathf.Abs(_area / 2.0f);
			}

			private void CalculateBounds()
			{
				_bounds = OBBox.Fit(Decompose());
				//_bounds.DrawDebug(Color.red, 21);
			}
		}
		
		private class SplitShapeSegment
		{
			public Vector2 p0;
			public Vector2 p1;
			public SplitShape s0;
			public SplitShape s1;

			public SplitShapeSegment(Vector2 a, Vector2 b)
			{
				p0 = a;
				p1 = b;
				s0 = null;
				s1 = null;
			}

			public SplitShapeSegment(Vector2 a, Vector2 b, SplitShape initialShape)
			{
				p0 = a;
				p1 = b;
				s0 = initialShape;
				s1 = null;
			}

			public SplitShapeSegment(Vector2 a, Vector2 b, SplitShape shape0, SplitShape shape1)
			{
				p0 = a;
				p1 = b;
				s0 = shape0;
				s1 = shape1;
			}

			public bool Intersects(Vector2 intersectionLine0, Vector2 intersectionLine1, out Vector2 intersectionPoint)
			{
				if (Segments.Intersects(intersectionLine0, intersectionLine1, p0, p1))
				{
					intersectionPoint = Segments.FindIntersection(intersectionLine0, intersectionLine1, p0, p1);
					return true;
				}
				
				intersectionPoint = Vector2.zero;
				return false;
			}

			public SplitShapeSegment[] Split(Vector2 intersectionPoint)
			{
				SplitShapeSegment[] output = new SplitShapeSegment[2];
				output[0] = new SplitShapeSegment(p0, intersectionPoint, s0, s1);
				output[1] = new SplitShapeSegment(intersectionPoint, p1, s0, s1);

				if (s0 != null)
				{
					s0.ReplaceSplitSegment(this,output[0],output[1]);
				}

				if (s1 != null)
				{
					s1.ReplaceSplitSegment(this,output[0],output[1]);
				}
				
				return output;
			}

			public SplitShapeSegment[] Split(Vector2 intersectionPoint, SplitShapeSegment previous)
			{
				SplitShapeSegment[] output = Split(intersectionPoint);
				float s0sqmg = previous.SquareMagnitude(output[0]);
				float s1sqmg = previous.SquareMagnitude(output[1]);

				if (s1sqmg < s0sqmg)
				{
					SplitShapeSegment[] newOutput = new SplitShapeSegment[2];
					newOutput[0] = output[1];
					newOutput[1] = output[0];
					output = newOutput;
				}
				
				if (s0 != null)
				{
					s0.ReplaceSplitSegment(this,output[0],output[1]);
				}

				if (s1 != null)
				{
					s1.ReplaceSplitSegment(this,output[0],output[1]);
				}
				
				return output;
			}

			public float SquareMagnitude(SplitShapeSegment other)
			{
				float sqr0 = (p0 - other.p0).sqrMagnitude;
				float sqr1 = (p1 - other.p0).sqrMagnitude;
				float sqr2 = (p0 - other.p1).sqrMagnitude;
				float sqr3 = (p1 - other.p1).sqrMagnitude;
				return Mathf.Min(sqr0, sqr1, sqr2, sqr3);
			}

			public float SquareMagnitude(Vector2 point)
			{
				float sqr0 = (p0 - point).sqrMagnitude;
				float sqr1 = (p1 - point).sqrMagnitude;
				return Mathf.Min(sqr0, sqr1);
			}

			public Vector2 NextPoint(SplitShapeSegment previous)
			{
				float p0sqmg = previous.SquareMagnitude(p0);
				float p1sqmg = previous.SquareMagnitude(p1);

				if (p0sqmg < p1sqmg)
				{
					return p1;
				}

				return p0;
			}

			public void ReplaceShapeReference(SplitShape oldReference, SplitShape newReference)
			{
				if (s0 == oldReference)
				{
					s0 = newReference;
				}

				if (s1 == oldReference)
				{
					s1 = newReference;
				}
			}
		}*/
	}
}