using System;
using System.Collections.Generic;
using opengen.maths;
using UnityEngine;

namespace opengen.types
{
	public class SegmentShape
	{
			public Segment2D[] segments;
			private float _area;
			private OBBox _bounds;
			private int _size = 0;

			public float area => _area;
			public OBBox bounds => _bounds;

			public int size => _size;

			public SegmentShape(IList<Vector2> input)
			{
				_size = input.Count;
				segments = new Segment2D[size];
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

					Segment2D newSegment = new (a, b, this);
					segments[i] = newSegment;
				}
				
				CalculateArea();
				CalculateBounds();
			}

			public SegmentShape(List<Segment2D> input, SegmentShape oldShape)
			{
				// Debug.Log($"new split shape new{input.Count} old{oldShape.size}");
				segments = input.ToArray();
				foreach (Segment2D segment in segments)
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
				Segment2D previous = segments[_size - 1];
				for (int i = 0; i < _size; i++)
				{
					Segment2D current = segments[i];
					// Debug.Log(i+" "+current+" "+previous);
					output[i] = current.NextPoint(previous);
					previous = current;
				}

				return output;
			}

			/// <summary>
			/// Slice a shape segment into two parts
			/// At a given index and point
			/// </summary>
			public void SliceSegment(Vector2 point, int index)
			{
				Segment2D subjectSegment = segments[index];
				Segment2D newSegmentA = new (subjectSegment.p0, point, subjectSegment.s0, subjectSegment.s1);
				Segment2D newSegmentB = new (point, subjectSegment.p1, subjectSegment.s0, subjectSegment.s1);
				
				ReplaceSegment(subjectSegment, newSegmentA, newSegmentB);

				if (subjectSegment.s0 != null && subjectSegment.s0 != this)
				{
					// Debug.Log("replace seg x");
					subjectSegment.s0.ReplaceSegment(subjectSegment, newSegmentA, newSegmentB);
				}

				if (subjectSegment.s1 != null && subjectSegment.s1 != this)
				{
					// Debug.Log("replace seg x");
					subjectSegment.s1.ReplaceSegment(subjectSegment, newSegmentA, newSegmentB);
				}
			}

			private int SegmentIndex(Segment2D segment)
			{
				for (int i = 0; i < _size; i++)
				{
					if (segments[i] == segment)
					{
						return i;
					}
				}

				return -1;
			}

			private void ReplaceSegment(Segment2D old, Segment2D newSegmentA, Segment2D newSegmentB)
			{
				int index = SegmentIndex(old);
				// if (index == -1)
				// {
				// 	Debug.LogError("no index");
				// }
				int previousIndex = (index + _size - 1) % _size;
				Segment2D previous = segments[previousIndex];
				
				int newSize = _size + 1;
				Segment2D[] newSegments = new Segment2D[newSize];
				int insertionIndex = 0;
				bool segmentReplaced = false;
				for (int i = 0; i < _size; i++)
				{
					if (i == index)
					{
						segmentReplaced = true;
						float sqrMag0 = previous.SquareMagnitude(newSegmentA);
						float sqrMag1 = previous.SquareMagnitude(newSegmentB);

						if (sqrMag0 < sqrMag1)
						{
							newSegments[insertionIndex] = newSegmentA;
							insertionIndex++;
							newSegments[insertionIndex] = newSegmentB;
							insertionIndex++;
						}
						else
						{
							newSegments[insertionIndex] = newSegmentB;
							insertionIndex++;
							newSegments[insertionIndex] = newSegmentA;
							insertionIndex++;
						}

						// Debug.Log(i+" "+index+" "+_size);
					}
					else
					{
						newSegments[insertionIndex] = segments[i];
						insertionIndex++;
					}
				}

				// if (!segmentReplaced)
				// {
				// 	Debug.LogError("ISSUE no segment in this shape");
				// }
				
				segments = newSegments;
				_size = newSize;
			}

			public SegmentShape[] Slice(float sliceOffset)
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
				int index0 = 0, index1 = 0;
				//find the points where in the input intersection slice hits the shape sides
				for (int i = 0; i < _size; i++)
				{
					Segment2D current = segments[i];
					if (current.Intersects(intP0, intP1, out Vector2 intersectionPoint))
					{
						switch (intersectionCount)
						{
							case 0:
								int0 = intersectionPoint;
								SliceSegment(intersectionPoint, i);
								index0 = (i + 0) % _size;
								intersectionCount++;
								i++;
								break;
							case 1:
								int1 = intersectionPoint;
								SliceSegment(intersectionPoint, i);
								index1 = (i + 0) % _size;
								intersectionCount++;
								i++;
								break;
						}
					}

					if (intersectionCount == 2)
					{
						break;
					}
				}

				bool isShapeSplit = intersectionCount == 2;

				//dump the output if there is no intersection across the polygon
				if (!isShapeSplit)
				{
					// Debug.Log($"isShapeSplit {isShapeSplit}");
					SegmentShape[] dumpOutput = new SegmentShape[1];
					dumpOutput[0] = this;
					return dumpOutput;
				}

				// Debug.DrawLine(split0[0].p0.Vector3Flat(), split0[0].p1.Vector3Flat(), Color.magenta, 5);
				// Debug.DrawLine(split0[1].p0.Vector3Flat(), split0[1].p1.Vector3Flat(), Color.blue, 5);
				// Debug.DrawLine(split1[0].p0.Vector3Flat(), split1[0].p1.Vector3Flat(), Color.magenta, 5);
				// Debug.DrawLine(split1[1].p0.Vector3Flat(), split1[1].p1.Vector3Flat(), Color.blue, 5);
				
				//BUILD THE TWO POLY SHAPES
				Segment2D intersectionSegment = new (int0, int1);
				
				int index = index1;
				int iterations = _size;
				
				// start at the intersection, add the split line and then add the old lines in
				List<Segment2D> plotA = new ();
				// while loop this so we can pass through index 0 if we need to
				// Debug.Log(_size);
				while (iterations > 0)
				{
					index = (index + 1) % _size;
					Segment2D segment = segments[index];
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
				// plotA.Add(split0[0]);
				plotA.Add(intersectionSegment);
				// plotA.Add(split1[1]);
				//plot A complete
            
				//lets build the second shape
				//we build this one "backwards" to maintain new shape winding
				List<Segment2D> plotB = new ();
				//add the shape points in
				iterations = _size;
				index = index0;
				while (iterations > 0)
				{
					index = (index + 1) % _size;
					Segment2D segment = segments[index];
					plotB.Add(segment);
                
					// if we hit the intersection, stop
					if (index == index1)
					{
						break;
					}
					
					//Debug.Log($"{index} segment {segment}");

					// if (segment != null)
					// {
					// }

					iterations--;
				}
				// plotB.Add(split1[0]);
				plotB.Add(intersectionSegment);
				// plotB.Add(split0[1]);
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

				SegmentShape[] output = new SegmentShape[2];
				output[0] = new SegmentShape(plotA, this);
				output[1] = new SegmentShape(plotB, this);
				intersectionSegment.SetShape0(output[0]);
				intersectionSegment.SetShape1(output[1]);

				// Debug.Log("shape 0 size "+output[0].size);
				// Debug.Log("shape 1 size "+output[1].size);

				return output;
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
}