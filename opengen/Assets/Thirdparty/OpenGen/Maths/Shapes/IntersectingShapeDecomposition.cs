using System.Collections.Generic;
using UnityEngine;

namespace opengen.maths.shapes
{
    /// <summary>
    /// Break down a shape into an array of non self intersecting shapes
    /// </summary>
    public class IntersectingShapeDecomposition
    {
        public static Vector2[][] Execute(Vector2[] points)
        {
            int pointCount = points.Length;
            List<Vector2[]> output = new();

            //we'll go through the shape from the first point (arbitrary)
            //if we find an intersection - we'll split the shape there to create two shapes
            //send those shapes recursively until there are no intersections
            //we aim to generate a set of non intersecting shapes
            //the winding of these shapes might be important to gauge their validity
            bool selfIntersects = false;
            for (int x = 0; x < pointCount; x++)
            {
                int xb = x < pointCount - 1 ? x + 1 : 0;
                var a1 = points[x];
                var a2 = points[xb];
                for (int y = x; y < pointCount; y++)
                {
                    if (x == y || xb == y)
                    {
                        continue;
                    }

                    int yb = y < pointCount - 1 ? y + 1 : 0;
                    if (x == yb || xb == yb)
                    {
                        continue;
                    }

                    var b1 = points[y];
                    var b2 = points[yb];
                    
                    if (FindSegmentIntersection(a1, a2, b1, b2, out Vector2 intersection))
                    {
                        selfIntersects = true;
                        Vector2[][] splitShapes = SplitShape(points, intersection, x, y);
                        //in order to understand recursion, we must first understand recursion
                        output.AddRange(Execute(splitShapes[0]));
                        output.AddRange(Execute(splitShapes[1]));
                        // output.AddRange(splitShapes);
                        break;    
                    }
                }

                //if we found our intersection - we need to kill further processing
                if (selfIntersects)
                {
                    break;
                }
            }
            
            //if there are no intersections - we need to just send back the input shape
            //this might be because the shape was fully decomposed and no longer self intersects
            //or a non self intersecting shape was sent to the initial function
            if(!selfIntersects)
            {
                output.Add(points);
            }

            return output.ToArray();
        }

        private static Vector2[][] SplitShape(Vector2[] input, Vector2 intersection, int index0, int index1)
        {
            int inputSize = input.Length;
            Vector2[][] output = new Vector2[2][];
            int size0 = index0 + (inputSize - index1) + 1;
            output[0] = new Vector2[size0];
            int size1 = inputSize - size0 + 2;
            output[1] = new Vector2[size1];

            // Debug.Log(inputSize+" "+size0+" "+size1);
            // Debug.Log(index0+" "+index1);

            int o0index = 0;
            int o1index = 0;
            for (int i = 0; i < inputSize; i++)
            {
                Vector2 inputPoint = input[i];
                if (i <= index0)
                {
                    output[0][o0index] = inputPoint;
                    o0index++;
                    //i = index1 - 1;
                }

                if (i == index0)
                {
                    output[0][o0index] = intersection;
                    o0index++;
                }

                if (i > index1)
                {
                    output[0][o0index] = inputPoint;
                    o0index++;
                }

                if (o0index == size0)
                {
                    break;
                }
            }

            output[1][o1index] = intersection;
            o1index++;
            for (int i = index1; i > index0; i--)
            {
                Vector2 inputPoint = input[i];
                output[1][o1index] = inputPoint;
                o1index++;
            }

            return output;
        }
        
        private static bool FindSegmentIntersection(Vector2 s1, Vector2 e1, Vector2 s2, Vector2 e2, out Vector2 intersection)
        {
            Vector2 b = e1 - s1;
            Vector2 d = e2 - s2;
            float bDotDPerp = b.x * d.y - b.y * d.x;

            intersection = Vector2.zero;
            
            // if b dot d == 0, it means the lines are parallel so have infinite intersection points
            if (Mathf.Abs(bDotDPerp) < Numbers.Epsilon)
            {
                return false;
            }

            //check the intersection occurs within the two segments
            Vector2 c = s2 - s1;
            float t = (c.x * d.y - c.y * d.x) / bDotDPerp;
            if (t < 0 || t > 1)
            {
                return false;
            }

            float u = (c.x * b.y - c.y * b.x) / bDotDPerp;
            if (u < 0 || u > 1)
            {
                return false;
            }

            float a1 = e1.y - s1.y;
            float b1 = s1.x - e1.x;
            float c1 = a1 * s1.x + b1 * s1.y;
            float a2 = e2.y - s2.y;
            float b2 = s2.x - e2.x;
            float c2 = a2 * s2.x + b2 * s2.y;
            float delta = a1 * b2 - a2 * b1;

            //check the output is not zero
            if (Mathf.Abs(delta) < Numbers.Epsilon)
            {
                return false;
            }

            intersection = new Vector2((b2 * c1 - b1 * c2) / delta, (a1 * c2 - a2 * c1) / delta);
            
            return true; 
        }
    }
}