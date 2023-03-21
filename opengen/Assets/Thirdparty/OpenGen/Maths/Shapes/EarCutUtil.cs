using System;
using System.Collections.Generic;
using opengen.types;
using UnityEngine;


namespace opengen.maths.shapes
{
    public class EarCutUtil
    {
        public static List<int> Tessellate(Shape data)
        {
            return Tessellate(data.pointList, new Vector2[0][]);
        }
        
        public static List<int> Tessellate(IList<Vector2> data)
        {
            return Tessellate(data, new Vector2[0][]);
        }
        
        public static List<int> Tessellate(IList<Vector2> data, IList<IList<Vector2>> holes, bool flipTri = false)
        {
            int dataLength = data.Count;
            int holeCount = holes.Count;
            int flatDataLength = dataLength;
            for(int h = 0; h < holeCount; h++)
            {
                flatDataLength += holes[h].Count;
            }

            flatDataLength *= 2;
            
            List<float> flatData = new(flatDataLength);
            List<int> holeIndices = new(holeCount);
            for(int d = 0; d < dataLength; d++)
            {
                flatData.Add(data[d].x);
                flatData.Add(data[d].y);
            }
            for(int h = 0; h < holeCount; h++)
            {
                holeIndices.Add(flatData.Count / 2);
                int holeSize = holes[h].Count;
                for(int d = 0; d < holeSize; d++)
                {
                    flatData.Add(holes[h][d].x);
                    flatData.Add(holes[h][d].y);
                }
            }

            List<int> output = Earcut.Tessellate(flatData, holeIndices);
            
            if(flipTri)
            {
                FlipTri(output);
            }

            if(CheckData(output, flatDataLength - 1))
            {
                throw new Exception("Tessellate Exceeds Bounds");
            }

            return output;
        }
        
        public static List<int> Tessellate(Vector3[] data, Vector3[][] holes)
        {
            int dataLength = data.Length;
            int holeCount = holes.Length;
            int flatDataLength = dataLength;
            for(int h = 0; h < holeCount; h++)
            {
                flatDataLength += holes[h].Length;
            }

            List<float> flatData = new(flatDataLength);
            List<int> holeIndices = new(holeCount);
            for(int d = 0; d < dataLength; d++)
            {
                flatData.Add(data[d].x);
                flatData.Add(data[d].z);
            }

            for(int h = 0; h < holeCount; h++)
            {
                holeIndices.Add(flatData.Count / 2);
                int holeSize = holes[h].Length / 2;
                for(int d = 0; d < holeSize; d++)
                {
                    flatData.Add(holes[h][d].x);
                    flatData.Add(holes[h][d].z);
                }
            }
            
            List<int> output = Earcut.Tessellate(flatData, holeIndices);
            
            if(CheckData(output, flatDataLength - 1))
            {
                throw new Exception("Tessellate Exceeds Bounds");
            }

            return output;
        }

        public static void FlipTri(List<int> tri)
        {
            int triLength = tri.Count;
            for(int t = 0; t < triLength; t+=3)
            {
                int a = tri[t + 1];
                int b = tri[t + 2];
                tri[t + 1] = b;
                tri[t + 2] = a;
            }
        }

        public static void FlipTri(int[] tri)
        {
            int triLength = tri.Length;
            for(int t = 0; t < triLength; t+=3)
            {
                int a = tri[t + 1];
                int b = tri[t + 2];
                tri[t + 1] = b;
                tri[t + 2] = a;
            }
        }

        private static bool CheckData(List<int> indices, int max)
        {
            int size = indices.Count;
            bool output = false;
            for(int i = 0; i < size; i++)
            {
                if(indices[i] > max)
                {
                    output = true;
                    indices[i] = max;
                }
            }
            return output;
        }
    }
}