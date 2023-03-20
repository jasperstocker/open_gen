using opengen.types;
using UnityEngine;

namespace opengen
{
	public static class DebugDraw
	{
		public static void DrawShape(Vector2[] shape, Color colour, float duration = 0.0f)
		{
			DrawShape(shape, Vector3.zero, colour, duration);
		}
		
		public static void DrawShape(Vector2[] shape, Vector3 offset, Color colour, float duration = 0.0f)
		{
			int shapeSize = shape.Length;
			for(int i = 0; i < shapeSize; i++)
			{
				int next = i + 1;
				if(next >= shapeSize)
				{
					next = 0;
				}

				Vector3 p0 = shape[i].Vector3Flat() + offset;
				Vector3 p1 = shape[next].Vector3Flat() + offset;

				Debug.DrawLine(p0, p1, colour, duration);
			}
		}
	}
}