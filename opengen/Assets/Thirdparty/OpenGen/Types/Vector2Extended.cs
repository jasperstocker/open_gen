using UnityEngine;

namespace opengen.types
{
	public static class Vector2Extended
	{
		public static float Remap(this float value, float from1, float to1, float from2, float to2)
		{
			return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
		}

		public static Vector2 Rotate(this Vector2 value, float degrees)
		{
			float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
			float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

			float tx = value.x;
			float ty = value.y;

			Vector2 output = new Vector2
			{
				x = ((cos * tx) - (sin * ty)), y = ((sin * tx) + (cos * ty))
			};
			return output;
		}

		public static Vector2 Rotate90Clockwise(this Vector2 value)
		{
			return new Vector2(value.y, -value.x);
		}

		public static float Angle(Vector2 from, Vector2 to)
		{
			return Mathf.Atan2(to.y - from.y, to.x - from.x);
		}

		public static float SignAngle(Vector2 from, Vector2 to)
		{
			Vector2 vector = to - from;
			Vector2 dir = vector.normalized;
			float angle = Angle(Vector2.up, dir);
			Vector3 dirV3 = new Vector3(dir.x, 0, dir.y);
			Vector3 cross = Vector3.Cross(Vector3.up, dirV3);
			if (cross.z > 0)
			{
				angle = -angle;
			}

			return angle;
		}

		public static float SignAngle(Vector2 dir)
		{
			float angle = Angle(Vector2.up, dir);
			Vector3 dirV3 = new Vector3(dir.x, 0, dir.y);
			Vector3 cross = Vector3.Cross(Vector3.forward, dirV3);
			if (cross.z > 0)
			{
				angle = -angle;
			}

			return angle;
		}

		public static float SignAngleDirection(Vector2 dirForward, Vector2 dirAngle)
		{
			float angle = Angle(dirForward, dirAngle);
			Vector2 cross = Rotate(dirForward, 90);
			float crossDot = Vector2.Dot(cross, dirAngle);
			if (crossDot < 0)
			{
				angle = -angle;
			}

			return angle;
		}

		public static float Cross(Vector2 a, Vector2 b, Vector2 c)
		{
			float x1 = b.x - a.x;
			float y1 = b.y - a.y;
			float x2 = c.x - b.x;
			float y2 = c.y - b.y;
			return x1 * y2 - x2 * y1;
		}

		public static float Cross(this Vector2 value, Vector2 c)
		{
			return value.x * c.y - value.y * c.x;
		}

		public static Vector3 Vector3Up(this Vector2 value, float depth = 0)
		{
			return new Vector3(value.x, value.y, depth);
		}

		public static Vector3 Vector3Flat(this Vector2 value, float elevation = 0)
		{
			return new Vector3(value.x, elevation, value.y);
		}
	}
}