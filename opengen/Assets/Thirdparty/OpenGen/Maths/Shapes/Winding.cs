using UnityEngine;

namespace opengen.maths.shapes
{
    public static class Winding
    {
        public static float Calculate(Vector2[] shape)
        {
            int shapeSize = shape.Length;
            float calculation = 0;
            for (int i = 0; i < shapeSize; i++)
            {
                int ib = i < shapeSize - 1 ? i + 1 : 0;

                Vector2 p0 = shape[i];
                Vector2 p1 = shape[ib];
                
                calculation += (p1.x - p0.x) * (p1.y - p0.y);
            }

            return Numbers.Sign(calculation);
        }
    }
}