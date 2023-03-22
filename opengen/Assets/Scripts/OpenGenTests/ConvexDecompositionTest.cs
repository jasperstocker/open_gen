using System.Collections.Generic;
using opengen;
using opengen.maths.shapes;
using opengen.unityeditor;
using UnityEngine;

namespace opengentests
{
    public class ConvexDecompositionTest : MonoBehaviour
    {
        [SerializeField] private ShapeComponent _shapeComponent = null;
        [SerializeField] private Color _inputColour = Color.green;
        [SerializeField] private Color _outputColour = Color.red;
		
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
			
            List<List<Vector2>> concaveShapes = Convex.Decompose2(_shapeComponent.shape.pointList);

            foreach (List<Vector2> concaveShape in concaveShapes)
            {
                GizmoDraw.DrawShape(concaveShape, transform.localToWorldMatrix, _outputColour);
            }
        }
    }
}