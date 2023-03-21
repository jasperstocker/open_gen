using System.Collections.Generic;
using opengen.types;
using UnityEngine;

namespace opengen
{
	public static class GizmoDraw
	{
		public static void DrawShape(Shape shape, Color colour)
		{
			DrawShape(shape.pointList, Matrix4x4.identity, colour);
		}
		
		public static void DrawShape(Shape shape, Matrix4x4 matrix4, Color colour)
		{
			DrawShape(shape.pointList, matrix4, colour);
		}
		
		public static void DrawShape(IList<Vector2> shape, Color colour)
		{
			DrawShape(shape, Matrix4x4.identity, colour);
		}
		
		public static void DrawShape(IList<Vector2> shape, Matrix4x4 matrix4, Color colour)
		{
			int shapeSize = shape.Count;
			Gizmos.color = colour;
			Gizmos.matrix = matrix4;
			for(int i = 0; i < shapeSize; i++)
			{
				int next = i + 1;
				if(next >= shapeSize)
				{
					next = 0;
				}

				Vector3 p0 = shape[i].Vector3Flat();
				Vector3 p1 = shape[next].Vector3Flat();

				Gizmos.DrawLine(p0, p1);
			}
		}
	}
}