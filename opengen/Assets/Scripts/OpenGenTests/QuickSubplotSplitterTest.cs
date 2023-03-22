using System.Collections.Generic;
using opengen;
using opengen.maths.shapes;
using opengen.unityeditor;
using UnityEngine;

namespace opengentests
{
	public class QuickSubplotSplitterTest : MonoBehaviour
	{
		[SerializeField] private ShapeComponent _shapeComponent = null;
		[SerializeField] private Color _inputColour = Color.green;
		[SerializeField] private Color _outputColour = Color.red;
		[SerializeField] private uint _seed = 1;
		[SerializeField] private float _maxArea = 1;
		[SerializeField] private float _cutVariation = 1;
		
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
            
			_shapeComponent.colour = _inputColour;
			
			List<Vector2[]> subplots = ShapeSplit.SubplotDivisionConcave(_shapeComponent.shape.pointList, _maxArea, _seed, _cutVariation);

			foreach (Vector2[] subplot in subplots)
			{
				GizmoDraw.DrawShape(subplot, transform.localToWorldMatrix, _outputColour);
			}
		}
	}
}