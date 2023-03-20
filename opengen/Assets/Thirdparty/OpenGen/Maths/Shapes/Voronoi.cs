using System.Collections.Generic;
using System.Linq;
using opengen.types;
using UnityEngine;

namespace opengen.maths.shapes
{
    public class Voronoi
    {
        public static DelaunayEdge[] GenerateEdgesFromDelaunay(DelaunayTriangle[] triangulation)
        {
            HashSet<DelaunayEdge> voronoiEdges = new();
            foreach (DelaunayTriangle triangle in triangulation)
            {
                foreach (DelaunayTriangle neighbor in triangle.TrianglesWithSharedEdge)
                {
                    DelaunayEdge edge = new(triangle.circumcenter, neighbor.circumcenter);
                    voronoiEdges.Add(edge);
                }
            }

            return voronoiEdges.ToArray();
        }

        public static Vector2[][] GenerateRegionsFromDelaunay(DelaunayPoint[] points)
        {
            int pointCount = points.Length;
            Vector2[][] output = new Vector2[pointCount][];
            for (int i = 0; i < pointCount; i++)
            {
                DelaunayPoint point = points[i];
                int shapeSize = point.AdjacentTriangles.Count;
                Vector2[] shape = new Vector2[shapeSize];
                int shapeCounter = 0;
                foreach (DelaunayTriangle triangle in point.AdjacentTriangles)
                {
                    shape[shapeCounter] = triangle.circumcenter.vector2;
                    shapeCounter++;
                }

                shape = Shapes.OrderPoints(shape);
                output[i] = shape;
            }

            return output;
        }
    }
}