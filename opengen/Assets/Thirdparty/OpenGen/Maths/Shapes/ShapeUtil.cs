using System.Collections.Generic;
using UnityEngine;

namespace opengen.maths.shapes
{
	public class ShapeUtil
	{
		public static bool SelfIntersectionCheck(IList<Vector2> points, bool log = false)
		{
			int size = points.Count;
			for (int x = 0; x < size; x++)
			{
				int xb = x > 0 ? x - 1 : size - 1;
				for (int y = x; y < size; y++)
				{
					if (x == y || xb == y)
					{
						continue;
					}

					int yb = y > 0 ? y - 1 : size - 1;
					if (x == yb || xb == yb)
					{
						continue;
					}

					Vector2 x0 = points[x];
					Vector2 x1 = points[xb];
					Vector2 y0 = points[y];
					Vector2 y1 = points[yb];

					if (Segments.Intersects(x0, x1, y0, y1))
					{
						if (log)
						{
							OpenGenLog.Log($"SelfIntersectionCheck {x} . {xb} . {y} . {yb}");
						}
						return true;
					}
				}
			}

			return false;
		}
	}
}