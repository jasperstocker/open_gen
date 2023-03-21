using opengen;
using opengen.maths.shapes;
using opengen.types;
using opengen.unityeditor;
using UnityEngine;

namespace opengentests
{
	public class OBBFitTest : MonoBehaviour
	{
		[SerializeField] private ShapeComponent _shapeComponent = null;
		[SerializeField] private Color _inputShape = Color.green;
		[SerializeField] private Color _obbShape = Color.red;
        
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
			
			OBBox obb = OBBox.Fit(_shapeComponent.shape);
			GizmoDraw.DrawShape(_shapeComponent.shape, transform.localToWorldMatrix, _inputShape);
			GizmoDraw.DrawShape(obb.Points, transform.localToWorldMatrix, _obbShape);
		}
	}
}