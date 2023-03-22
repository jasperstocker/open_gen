using opengen.types;
using UnityEngine;

namespace opengen.unityeditor
{
	public class ShapeComponent : MonoBehaviour
	{
		public Shape shape;
		public Color colour = Color.white;
		public bool drawShape = true;
		public bool showDirection = true;
		
		public void GenerateTriangle()
		{
			shape = new Shape();
			shape.Add(new Vector2(0, 0));
			shape.Add(new Vector2(0, 1));
			shape.Add(new Vector2(1, 0));
		}
		
		public void GenerateSquare()
		{
			shape = new Shape();
			shape.Add(new Vector2(0, 0));
			shape.Add(new Vector2(0, 1));
			shape.Add(new Vector2(1, 1));
			shape.Add(new Vector2(1, 0));
		}
	}
}