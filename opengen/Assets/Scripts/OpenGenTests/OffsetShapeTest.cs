using System.Collections.Generic;
using opengen;
using opengen.maths.shapes;
using opengen.unityeditor;
using UnityEngine;

namespace opengentests
{
	public class OffsetShapeTest : MonoBehaviour
	{
		[SerializeField] private ShapeComponent _shapeComponent = null;
		[SerializeField] private Color _inputColour = Color.green;
		[SerializeField] private Color _offsetColour = Color.red;
		[SerializeField] private float _offsetAmount = 0.25f;
        
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

			_shapeComponent.drawShape = false;
			GizmoDraw.DrawShape(_shapeComponent.shape, transform.localToWorldMatrix, _inputColour);
			List<Vector2[]> offsetShapes = Offset.Execute(_shapeComponent.shape, _offsetAmount);
			foreach (Vector2[] shape in offsetShapes)
			{
				GizmoDraw.DrawShape(shape, transform.localToWorldMatrix, _offsetColour);
			}
		}
	}
}