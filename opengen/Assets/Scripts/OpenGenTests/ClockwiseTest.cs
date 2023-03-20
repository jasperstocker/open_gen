using opengen;
using opengen.maths.shapes;
using UnityEngine;

namespace opengentests
{
    public class ClockwiseTest : MonoBehaviour
    {

        // clockwise
        Vector2[] shape1 = {
            new(0, 0),
            new(0, 1),
            new(1, 1),
            new(1, 0)
        };
        
        // anti-clockwise
        Vector2[] shape2 = {
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(0, 1)
        };

        private void OnDrawGizmosSelected()
        {
            bool isShape1Clockwise = Clockwise.Check(shape1);
            Color shape1Color = isShape1Clockwise ? Color.green : Color.red;
            DebugDraw.DrawShape(shape1, shape1Color, 1.0f);
            
            bool isShape2Clockwise = Clockwise.Check(shape2);
            Color shape2Color = isShape2Clockwise ? Color.green : Color.red;
            DebugDraw.DrawShape(shape2, new Vector3(2, 0, 0), shape2Color, 1.0f);
        }
    }
}