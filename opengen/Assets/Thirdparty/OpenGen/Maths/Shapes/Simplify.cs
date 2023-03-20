using System;
using System.Collections.Generic;
using opengen.types;
using UnityEngine;


namespace opengen.maths.shapes
{
    public class Simplify
    {
        public static Vector2[] Execute(Vector2[] input, float dot_threashold = 0.99f)
        {
            int inputSize = input.Length;
            if(inputSize < 4)
            {
                return input;
            }

            List<Vector2> output = new(inputSize);

            for(int i = 0; i < inputSize; i++)
            {
                Vector2 newPoint = input[i];
                if(output.Count < 2)
                {
                    output.Add( newPoint);
                }
                else
                {
                    Vector2 lastA = output[output.Count - 2];
                    Vector2 lastB = output[output.Count - 1];

                    Vector2 dirAB = (lastB - lastA).normalized;
                    Vector2 dirNB = (newPoint - lastB).normalized;
                    float dot = Vector2.Dot(dirAB, dirNB);
                    if(dot > dot_threashold)
                    {
                        output.RemoveAt(output.Count - 1);
                    }

                    output.Add(newPoint);
                }
            }

            //final loop check
            Vector2 a = output[output.Count - 1];
            Vector2 b = output[0];
            Vector2 c = output[1];
            
            Vector2 dirFAB = (b - a).normalized;
            Vector2 dirFBC = (c - b).normalized;  
            float dotF = Vector2.Dot(dirFAB, dirFBC);                  
            if(dotF > dot_threashold)
            {
                output.RemoveAt(0);
            }

            return output.ToArray();
        }
        
        public static Vector2[] Execute2(Vector2[] input, float epsilon = 0.001f)
        {
            int inputSize = input.Length;
            if(inputSize < 4)
            {
                return input;//cannot simplify triangles!
            }

            List<Vector2> output = new(inputSize);

            output.Add(input[0]);
            output.Add(input[1]);
            
            for(int i = 2; i < inputSize; i++)
            {
                Vector2 newPoint = input[i];
                bool parallel = isParallel(output[output.Count - 2],output[output.Count - 1],output[output.Count - 1],newPoint, epsilon);
                if(parallel)
                {
                    output.RemoveAt(output.Count - 1);
                }

                output.Add(newPoint);
            }

            //final loop check
            if(isParallel(output[output.Count - 2],output[output.Count - 1],output[output.Count - 1],output[0], epsilon))
            {
                output.RemoveAt(output.Count - 1);
            }

            if(isParallel(output[output.Count - 1],output[0],output[0],output[1], epsilon))
            {
                output.RemoveAt(0);
            }

            return output.ToArray();
        }

        private static bool isParallel(Vector2 s1, Vector2 e1, Vector2 s2, Vector2 e2, float epsilon)
        {
            float yd1 = e1.y - s1.y;
            float xd1 = s1.x - e1.x;
            float yd2 = e2.y - s2.y;
            float xd2 = s2.x - e2.x;
            float delta = yd1 * xd2 - yd2 * xd1;
            return Mathf.Abs(delta) < epsilon;
        }
    }
}