using System.Collections.Generic;
using UnityEngine;

namespace opengen.maths.shapes
{
	// TODO write summary
	
	public static class SimpleClip
	{
		public static List<Vector2> Execute(IList<Vector2> shape, Rect bounds)
		{
			//return new List<Vector2>(shape);
			List<Vector2> left = Left(shape, bounds.xMin);
			List<Vector2> right = Right(left, bounds.xMax);
			List<Vector2> top = Top(right, bounds.yMax);
			List<Vector2> bottom = Bottom(top, bounds.yMin);
			return bottom;
		}

		public static List<Vector2> Left(IList<Vector2> shape, float pos)
		{
			int size = shape.Count;
			List<Vector2> output = new List<Vector2>(size);
			for (int i = 0; i < size; i++)
			{
				int ib = i < size - 1 ? i + 1 : 0;

				Vector2 p0 = shape[i];
				Vector2 p1 = shape[ib];

				bool p0i = p0.x >= pos;
				bool p1i = p1.x >= pos;

				if (p0i && p1i)
				{
					//line fully within the plane
					output.Add(p0);
				}
				else if (!p0i && !p1i)
				{
					//line entirely outside the plane - ignore 
				}
				else
				{
					bool backwards = p1.x < p0.x;
					if (backwards)
					{
						(p0, p1) = (p1, p0);
					}

					float lerp = (pos - p0.x) / (p1.x - p0.x);
					Vector2 linePos = Vector2.Lerp(p0, p1, lerp);
					if (backwards) //capture the point inside the plane
					{
						output.Add(p1);
					}

					output.Add(linePos);
				}
			}

			return output;
		}

		public static List<Vector2> Right(IList<Vector2> shape, float pos)
		{
			int size = shape.Count;
			List<Vector2> output = new List<Vector2>(size);
			for (int i = 0; i < size; i++)
			{
				int ib = i < size - 1 ? i + 1 : 0;

				Vector2 p0 = shape[i];
				Vector2 p1 = shape[ib];

				bool p0i = p0.x <= pos;
				bool p1i = p1.x <= pos;

				if (p0i && p1i)
				{
					//line fully within the plane
					output.Add(p0);
				}
				else if (!p0i && !p1i)
				{
					//line entirely outside the plane - ignore 
				}
				else
				{
					bool backwards = p1.x < p0.x;
					if (backwards)
					{
						(p0, p1) = (p1, p0);
					}

					float lerp = (pos - p0.x) / (p1.x - p0.x);
					Vector2 linePos = Vector2.Lerp(p0, p1, lerp);
					if (!backwards) //capture the point inside the plane
					{
						output.Add(p0);
					}

					output.Add(linePos);
				}
			}

			return output;
		}

		public static List<Vector2> Bottom(IList<Vector2> shape, float pos)
		{
			int size = shape.Count;
			List<Vector2> output = new List<Vector2>(size);
			for (int i = 0; i < size; i++)
			{
				int ib = i < size - 1 ? i + 1 : 0;

				Vector2 p0 = shape[i];
				Vector2 p1 = shape[ib];

				bool p0i = p0.y >= pos;
				bool p1i = p1.y >= pos;

				if (p0i && p1i)
				{
					//line fully within the plane
					output.Add(p0);
				}
				else if (!p0i && !p1i)
				{
					//line entirely outside the plane - ignore 
				}
				else
				{
					bool backwards = p1.y < p0.y;
					if (backwards)
					{
						(p0, p1) = (p1, p0);
					}

					float lerp = (pos - p0.y) / (p1.y - p0.y);
					Vector2 linePos = Vector2.Lerp(p0, p1, lerp);
					if (backwards) //capture the point inside the plane
					{
						output.Add(p1);
					}

					output.Add(linePos);
				}
			}

			return output;
		}

		public static List<Vector2> Top(IList<Vector2> shape, float pos)
		{
			int size = shape.Count;
			List<Vector2> output = new List<Vector2>(size);
			for (int i = 0; i < size; i++)
			{
				int ib = i < size - 1 ? i + 1 : 0;

				Vector2 p0 = shape[i];
				Vector2 p1 = shape[ib];

				bool p0i = p0.y <= pos;
				bool p1i = p1.y <= pos;

				if (p0i && p1i)
				{
					//line fully within the plane
					output.Add(p0);
				}
				else if (!p0i && !p1i)
				{
					//line entirely outside the plane - ignore 
				}
				else
				{
					bool backwards = p1.y < p0.y;
					if (backwards)
					{
						(p0, p1) = (p1, p0);
					}

					float lerp = (pos - p0.y) / (p1.y - p0.y);
					Vector2 linePos = Vector2.Lerp(p0, p1, lerp);
					if (!backwards) //capture the point inside the plane
					{
						output.Add(p0);
					}

					output.Add(linePos);
				}
			}

			return output;
		}
	}
}