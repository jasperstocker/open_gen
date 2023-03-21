using opengen;
using opengen.maths.shapes;
using opengen.unityeditor;
using UnityEngine;

namespace opengentests
{
	public class ConvexShapeTest : MonoBehaviour
	{
		
		[SerializeField] private ShapeComponent _shapeComponent = null;
		[SerializeField] private Color _clockwiseColour = Color.green;
		[SerializeField] private Color _antiClockwiseColour = Color.red;
        
		private void OnDrawGizmos()
		{
			if (_shapeComponent == null)
			{
				_shapeComponent = GetComponent<ShapeComponent>();
			}
            
			if (_shapeComponent == null)
			{
				return;
			}
            
			Color colour = Convex.IsConvex(_shapeComponent.shape) ? _clockwiseColour : _antiClockwiseColour;
			_shapeComponent.colour = colour;
			GizmoDraw.DrawShape(_shapeComponent.shape, transform.localToWorldMatrix, colour);
		}
	}
}