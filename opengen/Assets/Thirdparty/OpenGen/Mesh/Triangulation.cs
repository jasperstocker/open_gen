using System.Collections.Generic;
using opengen.maths.shapes;
using UnityEngine;

namespace opengen.mesh
{
	public static class Triangulation
	{
		public static List<int> EarCut(IList<Vector2> shape, IList<IList<Vector2>> holeIndices)
		{
			int shapeSize = shape.Count;
			if (shapeSize < 3)
			{
				return new List<int>();
			}
			if (shapeSize == 3)
			{
				return new List<int>{ 0, 1, 2 };
			}
			
			if (!Convex.IsConvex(shape))
			{
				Debug.LogError("Unsupported currently");
				//List<List<Vector2>> convexShapes = Convex.Decompose(shape);
				return new List<int>();
			}
			
			return EarCutUtil.Tessellate(shape, holeIndices, false);
		}
		
		public static int[] MaxAreaShape(IList<Vector2> shape, bool testForConvex = true)
		{
			int shapeSize = shape.Count;
			if (shapeSize < 3)
			{
				return new int[0];
			}
			if (shapeSize == 3)
			{
				return new [] { 0, 1, 2 };
			}
			
			if(testForConvex)
			{
				if (!Convex.IsConvex(shape))
				{
					Debug.LogError("Unsupported currently");
					//List<List<Vector2>> convexShapes = Convex.Decompose(shape);
					return new int[0];
				}
			}
			
			return MaxAreaShapeConvex(shape);
		}

		public static int[] MaxAreaShapeConvex(IList<Vector2> shape)
		{
			return new int[0];
		}

		public static int[] MaxAreaCircle(IList<Vector2> shape)
		{
			int shapeSize = shape.Count;
			if (shapeSize < 3)
			{
				return new int[0];
			}
			if (shapeSize == 3)
			{
				return new [] { 0, 1, 2 };
			}

			int triangleCount = shapeSize - 2;
			int outputSize = triangleCount * 3;
			int[] output = new int[outputSize];

			output[0] = 0;
			output[1] = Mathf.FloorToInt(outputSize/3f);
			output[2] = output[1] * 2;
			
			
			return new int[0];
		}

		private static void MaxAreaCircleTriangle()
		{
			
		}
	}
}