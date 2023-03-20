using System.Collections.Generic;
using opengen.maths.shapes;
using opengen.types;
using UnityEngine;


namespace opengen.maths
{
    public static class Area
    {

        //Shoelace formula.  
        public static float Calculate(IList<Vector2> shape)
        {
            float output = 0;
            int shapeSize = shape.Count;
            for (int s = 0; s < shapeSize; s++)
            {
                
                int sb = s < shapeSize - 1 ? s + 1 : 0;

                output += (shape[sb].x + shape[s].x) * (shape[sb].y - shape[s].y);
            }
            // Return absolute value
            return Mathf.Abs(output / 2.0f);
        }
        
        public static float Calculate(IList<Vector2> shape, Vector2[][] holes)
        {
            float output = Calculate(shape);

            int holeCount = holes.Length;
            for (int h = 0; h < holeCount; h++)
            {
                output -= Calculate(holes[h]);
            }

            return output;
        }
        
        public static float BoundsArea(Vector2[] points)
        {
            //TODO break down into min/max calc
            AABBox bounds = new(points);
            float output = bounds.Area();
            return output;
        }
    }
}