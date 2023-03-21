using opengen;
using opengen.maths.shapes;
using opengen.types;
using UnityEngine;

namespace opengentests
{
	public class DelaunayTest : MonoBehaviour
	{
		[SerializeField] private int _seed = 438750;
		[SerializeField] private int _pointCount = 12;
		[SerializeField] private Vector2 _size = new (10, 10);
		[SerializeField] private Color _inputColour = Color.green;
		[SerializeField] private Color _outputColour = Color.blue;
        
		private void OnDrawGizmos()
		{
			DelaunayGraph graph = new ((uint)_seed, _pointCount, _size, 1);

			foreach (DelaunayPoint delaunayPoint in graph.points)
			{
				GizmoDraw.DrawPoint(delaunayPoint.vector2, transform.localToWorldMatrix, _inputColour);
			}
			
			foreach (DelaunayTriangle delaunayTriangle in graph.triangles)
			{
				GizmoDraw.DrawShape(delaunayTriangle.Points(), transform.localToWorldMatrix, _inputColour);
			}
			
			
			Vector2[][] voronoiRegions = Voronoi.GenerateRegionsFromDelaunay(graph.points);

			foreach (Vector2[] region in voronoiRegions)
			{
				GizmoDraw.DrawShape(region, transform.localToWorldMatrix, _outputColour);
			}
		}
	}
}