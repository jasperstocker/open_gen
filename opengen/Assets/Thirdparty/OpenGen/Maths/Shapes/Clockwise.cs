using System.Collections.Generic;
using opengen.types;
using UnityEngine;


namespace opengen.maths.shapes
{
	public class Clockwise
	{
		public static bool Check(IList<Vector2Fixed> points)
		{
			int numberOfPoints = points.Count;
			if (numberOfPoints < 3)
			{
				return (false);
			}

			float signedArea = 0;
			for (int i = 0; i < numberOfPoints; i++)
			{
				int j = (i + 1) % numberOfPoints;
				
				Vector2 pointA = points[i].vector2;
				Vector2 pointB = points[j].vector2;

				signedArea += pointA.x * pointB.y - pointB.x * pointA.y;
			}
			return signedArea / 2 < 0;
		}

		public static bool Check(IList<Vector2> points)
		{
			int numberOfPoints = points.Count;
			if (numberOfPoints < 3)
			{
				return (false);
			}

			float signedArea = 0;
			for (int i = 0; i < numberOfPoints; i++)
			{
				int j = (i + 1) % numberOfPoints;
				
				Vector2 pointA = points[i];
				Vector2 pointB = points[j];

				signedArea += pointA.x * pointB.y - pointB.x * pointA.y;
			}
			return signedArea / 2 < 0;
		}
	}
}