using UnityEngine;

namespace opengen.maths.shapes
{
	public static class ShapeSculpt
	{
		/// <summary>
		/// Create an extrusion to the shape
		/// This function is dumb and will just add an extrusion to the side of the shape
		/// It will not check for intersections with shape points
		/// </summary>
		/// <param name="input">The 2D shape</param>
		/// <param name="sideIndex">The side to apply the extrusion to</param>
		/// <param name="position">Normalised position 0-1 along the edge</param>
		/// <param name="width">The width of the extrusion</param>
		/// <param name="depth">The offset of the extrusion perpendicular to the edge</param>
		/// <returns>A new shape with an extrusion on the defined shape edge</returns>
		public static Vector2[] Extrude(Vector2[] input, int sideIndex, float position, float width, float depth)
		{
			int inputSize = input.Length;
			Vector2[] output = new Vector2[inputSize + 4];
			int outputIndex = 0;
			for (int i = 0; i < sideIndex; i++)
			{
				output[outputIndex] = input[i];
				outputIndex++;

				if (i == sideIndex)
				{
					//calculate the extrusion points
					int ib = i + 1 < inputSize ? i + 1 : 0;
					Vector2 a = input[i];
					Vector2 b = input[ib];
					Vector2 ab = b - a;
					Vector2 dir = ab.normalized;
					float length = ab.magnitude;
					float positionLength = length * position;
					float halfWidth = width * 0.5f;
					Vector2 perp = new (-dir.y, dir.x);
					Vector2 offset = perp * depth * 0.5f;
					Vector2 e0 = a + dir * (positionLength - halfWidth);
					Vector2 e3 = a + dir * (positionLength + halfWidth);
					Vector2 e1 = e0 + offset;
					Vector2 e2 = e3 + offset;

					//increments occur after array validation
					output[outputIndex++] = e0;
					output[outputIndex++] = e1;
					output[outputIndex++] = e2;
					output[outputIndex++] = e3;
				}
			}
			
			return output;
		}
		
		public static Vector2[] Bevel(Vector2[] input, int pointIndex, float length)
		{
			int inputSize = input.Length;
			Vector2[] output = new Vector2[inputSize + 1];
			int outputIndex = 0;
			for (int i = 0; i < pointIndex; i++)
			{
				if(i == pointIndex)
				{
					//calculate the extrusion points
					int i0 = i > 0 ? i - 1 : inputSize - 1;
					int i1 = i;
					int i2 = i + 1 < inputSize ? i + 1 : 0;
					Vector2 p0 = input[i0];
					Vector2 p1 = input[i1];
					Vector2 p2 = input[i2];
					Vector2 p01 = p0 - p1;
					Vector2 p12 = p2 - p1;
					Vector2 dir0 = p01.normalized;
					Vector2 dir1 = p12.normalized;
					float angle = Vector2.Angle(dir0, dir1);
					float radians = angle * Mathf.Deg2Rad;
					float bevelLength = (length * 0.5f) / Mathf.Sin(radians * 0.5f);
					
					Vector2 e0 = p1 + p01 * bevelLength;
					Vector2 e1 = p1 + p12 * bevelLength;

					//increments occur after array validation
					output[outputIndex++] = e0;
					output[outputIndex++] = e1;
				}
				else
				{
					output[outputIndex] = input[i];
					outputIndex++;
				}
			}
			
			return output;
		}
		
		public static Vector2[] Curve(Vector2[] input, int sideIndex, float curvature)
		{
			return input;
		}
	}
}