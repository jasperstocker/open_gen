using System.Collections.Generic;
using System.Linq;
using opengen.types;
using UnityEngine;

//https://github.com/RafaelKuebler/DelaunayVoronoi

namespace opengen.maths.shapes
{
	public class DelaunayTriangulator
	{
		private float MaxX { get; set; }
		private float MaxY { get; set; }
		private IEnumerable<DelaunayTriangle> border;

		public DelaunayPoint[] GenerateDelaunayPoints(uint newSeed, int amount, float maxX, float maxY)
		{
			DelaunayPoint[] output = new DelaunayPoint[amount];
			MaxX = maxX;
			MaxY = maxY;

			output[0] = new DelaunayPoint(0, 0);
			output[1] = new DelaunayPoint(0, MaxY);
			output[2] = new DelaunayPoint(MaxX, MaxY);
			output[3] = new DelaunayPoint(MaxX, 0);
			DelaunayTriangle tri1 = new (output[0], output[1], output[2]);
			DelaunayTriangle tri2 = new (output[0], output[2], output[3]);
			border = new List<DelaunayTriangle>() { tri1, tri2 };

			RandomGenerator random = new (newSeed);
			for (int i = 4; i < amount; i++)
			{
				float pointX = random.output * MaxX;
				float pointY = random.output * MaxY;
				output[i] = new DelaunayPoint(pointX, pointY);
			}

			return output;
		}
		
		public DelaunayPoint[] GenerateDelaunayPoints(Vector2[] newPoints)
		{
			int amount = newPoints.Length;
			DelaunayPoint[] output = new DelaunayPoint[amount + 4];
			AABBox mapBounds = new AABBox(newPoints);
			MaxX = mapBounds.width;
			MaxY = mapBounds.height;

			output[0] = new DelaunayPoint(0, 0);
			output[1] = new DelaunayPoint(0, MaxY);
			output[2] = new DelaunayPoint(MaxX, MaxY);
			output[3] = new DelaunayPoint(MaxX, 0);
			DelaunayTriangle tri1 = new (output[0], output[1], output[2]);
			DelaunayTriangle tri2 = new (output[0], output[2], output[3]);
			border = new List<DelaunayTriangle>() { tri1, tri2 };

			float xOffset = mapBounds.xMin;
			float yOffset = mapBounds.yMin;
			for (int i = 4; i < amount; i++)
			{
				float pointX = newPoints[i].x - xOffset;
				float pointY = newPoints[i].y - yOffset;
				output[i+4] = new DelaunayPoint(pointX, pointY);
			}

			return output;
		}

		public DelaunayTriangle[] BowyerWatson(DelaunayPoint[] points)
		{
			//var supraDelaunayTriangle = GenerateSupraDelaunayTriangle();
			HashSet<DelaunayTriangle> triangulation = new HashSet<DelaunayTriangle>(border);

			foreach (DelaunayPoint point in points)
			{
				//get (bad) triangles that this point is inside of
				DelaunayTriangle[] badDelaunayTriangles = FindBadDelaunayTriangles(point, triangulation);
				DelaunayEdge[] polygon = FindHoleBoundaries(badDelaunayTriangles);

				foreach (DelaunayTriangle triangle in badDelaunayTriangles)
				{
					for (int i = 0; i < 3; i++)
					{
						triangle[i].AdjacentTriangles.Remove(triangle);
					}
				}

				triangulation.RemoveWhere(o => badDelaunayTriangles.Contains(o));

				foreach (DelaunayEdge edge in polygon.Where(possibleEdge =>
					         possibleEdge.Point1 != point && possibleEdge.Point2 != point))
				{
					DelaunayTriangle triangle = new DelaunayTriangle(point, edge.Point1, edge.Point2);
					triangulation.Add(triangle);
				}
			}

			//triangulation.RemoveWhere(o => o.Vertices.Any(v => supraDelaunayTriangle.Vertices.Contains(v)));
			return triangulation.ToArray();
		}

		private DelaunayEdge[] FindHoleBoundaries(DelaunayTriangle[] badDelaunayTriangles)
		{
			List<DelaunayEdge> edges = new List<DelaunayEdge>();
			foreach (DelaunayTriangle triangle in badDelaunayTriangles)
			{
				edges.Add(new DelaunayEdge(triangle[0], triangle[1]));
				edges.Add(new DelaunayEdge(triangle[1], triangle[2]));
				edges.Add(new DelaunayEdge(triangle[2], triangle[0]));
			}

			//IEnumerable<IGrouping<DelaunayEdge, DelaunayEdge>> grouped = edges.GroupBy(o => o);
			IEnumerable<DelaunayEdge> boundaryEdges =
				edges.GroupBy(o => o).Where(o => o.Count() == 1).Select(o => o.First());
			return boundaryEdges.ToArray();

			int triCount = badDelaunayTriangles.Length;
			Debug.Log(triCount);
			int edgeCount = triCount * 3;
			DelaunayEdge[] allBadEdges = new DelaunayEdge[edgeCount];
			Debug.Log("edgeCount " + edgeCount);
			int[] badEdgeInstance = new int[edgeCount];

			for (int t = 0; t < triCount; t++)
			{
				DelaunayTriangle triangle = badDelaunayTriangles[t];
				allBadEdges[t * 3] = new DelaunayEdge(triangle[0], triangle[1]);
				allBadEdges[t * 3 + 1] = new DelaunayEdge(triangle[1], triangle[2]);
				allBadEdges[t * 3 + 2] = new DelaunayEdge(triangle[2], triangle[0]);
			}

			for (int e = 0; e < edgeCount; e++)
			{
				DelaunayEdge edge0 = allBadEdges[e];
				badEdgeInstance[e]++;
				for (int f = e + 1; f < edgeCount; f++)
				{
					DelaunayEdge edge1 = allBadEdges[f];
					if (DelaunayEdge.Equals(edge0, edge1))
					{
						badEdgeInstance[e]++;
						badEdgeInstance[f]++;
					}
				}
			}

			int outerEdges = 0;
			for (int e = 0; e < edgeCount; e++)
				if (badEdgeInstance[e] < 2)
					outerEdges++;
			Debug.Log("outerEdges " + outerEdges);

			DelaunayEdge[] output = new DelaunayEdge[outerEdges];
			int outputCounter = 0;
			for (int e = 0; e < edgeCount; e++)
			{
				if (badEdgeInstance[e] > 1)
					continue;
				output[outputCounter] = allBadEdges[e];
				outputCounter++;
			}

			return output;
		}

		private DelaunayTriangle GenerateSupraDelaunayTriangle()
		{
			//   1  -> maxX
			//  / \
			// 2---3
			// |
			// v maxY
			int margin = 500;
			DelaunayPoint point1 = new DelaunayPoint(0.5f * MaxX, -2 * MaxX - margin);
			DelaunayPoint point2 = new DelaunayPoint(-2 * MaxY - margin, 2 * MaxY + margin);
			DelaunayPoint point3 = new DelaunayPoint(2 * MaxX + MaxY + margin, 2 * MaxY + margin);
			return new DelaunayTriangle(point1, point2, point3);
		}

		// private ISet<DelaunayTriangle> FindBadDelaunayTriangles(DelaunayPoint point, HashSet<DelaunayTriangle> triangles)
		// {
		//     IEnumerable<DelaunayTriangle> badDelaunayTriangles = triangles.Where(o => o.IsPointInsideCircumcircle(point));
		//     return new HashSet<DelaunayTriangle>(badDelaunayTriangles);
		// }

		private DelaunayTriangle[] FindBadDelaunayTriangles(DelaunayPoint point, HashSet<DelaunayTriangle> triangles)
		{
			// return triangles.Where(o => o.IsPointInsideCircumcircle(point)).ToArray();

			var badTriangles = triangles.Where(o => o.IsPointInsideCircumcircle(point));
			return new HashSet<DelaunayTriangle>(badTriangles).ToArray();

			int badTriCount = 0;
			foreach (DelaunayTriangle delaunayTriangle in triangles)
			{
				if (delaunayTriangle.IsPointInsideCircumcircle(point))
					badTriCount++;
			}

			DelaunayTriangle[] output = new DelaunayTriangle[badTriCount];
			int counter = 0;
			foreach (DelaunayTriangle delaunayTriangle in triangles)
			{
				if (delaunayTriangle.IsPointInsideCircumcircle(point))
				{
					output[counter] = delaunayTriangle;
					counter++;
				}
			}

			return output;
		}

		public DelaunayPoint[] Relax(DelaunayPoint[] points, Rect bounds)
		{
			int pointCount = points.Length;
			//Debug.Log($"RELAX {pointCount}");
			DelaunayPoint[] output = new DelaunayPoint[pointCount];

			output[0] = new DelaunayPoint(points[0].vector2);
			output[1] = new DelaunayPoint(points[1].vector2);
			output[2] = new DelaunayPoint(points[2].vector2);
			output[3] = new DelaunayPoint(points[3].vector2);

			DelaunayTriangle tri1 = new(output[0], output[1], output[2]);
			DelaunayTriangle tri2 = new(output[0], output[2], output[3]);
			border = new List<DelaunayTriangle>() { tri1, tri2 };

			for (int i = 4; i < pointCount; i++)
			{
				DelaunayPoint delaunayPoint = points[i];
				Vector2[] shape = delaunayPoint.Shape();
				if (shape.Length == 0)
				{
					output[i] = delaunayPoint;
					continue;
				}

				List<Vector2> clipShape = SimpleClip.Execute(shape, bounds);
				Vector2 centroid = Shapes.OBBCenter(clipShape);

				DelaunayPoint newPoint = new DelaunayPoint(centroid);
				//Debug.DrawLine(dpoint.vector2.Vector3Flat(), centroid.Vector3Flat(), Color.red, 20);
				output[i] = newPoint;
				//output[i] = dpoint;//todo debug
			}

			//Debug.Log(count);

			return output;
		}
	}
}