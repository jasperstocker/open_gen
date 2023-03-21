using System.Collections.Generic;
using opengen;
using opengen.maths.shapes;
using opengen.unityeditor;
using UnityEngine;

namespace opengentests
{
    public class EarCutShapeTest : MonoBehaviour
    {
        [SerializeField] private ShapeComponent _shapeComponent = null;
        [SerializeField] private Color _colour = Color.yellow;
        
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
            
            List<int> indices = EarCutUtil.Tessellate(_shapeComponent.shape);
            GizmoDraw.DrawShape(_shapeComponent.shape, indices, transform.localToWorldMatrix, _colour);
        }
    }
}
