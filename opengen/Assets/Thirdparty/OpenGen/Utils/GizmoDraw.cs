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
		
		public static void DrawShapes(IList<Vector2[]> shapes, Matrix4x4 matrix4, Color colour)
		{
			foreach (Vector2[] shape in shapes)
			{
				DrawShape(shape, matrix4, colour);
			}
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
		
		public static void DrawShape(Shape shape, IList<int> indices, Matrix4x4 matrix4, Color colour)
		{
			DrawShape(shape.pointList, indices, matrix4, colour);
		}
		
		public static void DrawShape(IList<Vector2> shape, IList<int> indices, Matrix4x4 matrix4, Color colour)
		{
			int indexSize = indices.Count;
			Gizmos.color = colour;
			Gizmos.matrix = matrix4;
			for(int i = 0; i < indexSize - 2; i+=3)
			{
				int shapeIndex0 = indices[i];
				int shapeIndex1 = indices[i+1];
				int shapeIndex2 = indices[i+2];

				Vector3 p0 = shape[shapeIndex0].Vector3Flat();
				Vector3 p1 = shape[shapeIndex1].Vector3Flat();
				Vector3 p2 = shape[shapeIndex2].Vector3Flat();

				Gizmos.DrawLine(p0, p1);
				Gizmos.DrawLine(p1, p2);
				Gizmos.DrawLine(p2, p0);
			}
		}
	}
}