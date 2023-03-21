using opengen.maths.shapes;
using UnityEngine;

namespace opengen.types
{
	public class DelaunayGraph
	{
		public DelaunayPoint[] points { get; }
		public DelaunayTriangle[] triangles { get; }

		public DelaunayGraph(uint seed, int pointCount, Vector2 size, int relaxIterations = 0)
		{
			DelaunayTriangulator triangulator = new ();
			Rect mapBounds = new (0, 0, size.x, size.y);

			points = triangulator.GenerateDelaunayPoints(seed, pointCount, size.x, size.y);
			triangles = triangulator.BowyerWatson(points);
			for (int i = 0; i < relaxIterations; i++)
			{
				points = triangulator.Relax(points, mapBounds);
				triangles = triangulator.BowyerWatson(points);
			}
		}
		
		public DelaunayGraph(Vector2[] newPoints, int relaxIterations = 0)
		{
			DelaunayTriangulator triangulator = new ();
			points = triangulator.GenerateDelaunayPoints(newPoints);
			AABBox mapBounds = new AABBox(newPoints);
			triangles = triangulator.BowyerWatson(points);
			for (int i = 0; i < relaxIterations; i++)
			{
				points = triangulator.Relax(points, mapBounds.rect);
				triangles = triangulator.BowyerWatson(points);
			}
		}
	}
}