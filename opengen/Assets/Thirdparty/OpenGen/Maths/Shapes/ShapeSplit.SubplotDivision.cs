using System;
using System.Collections.Generic;
using System.Linq;
using opengen.types;
using UnityEngine;

namespace opengen.maths.shapes
{
    public static partial class ShapeSplit
    {
        public static List<Vector2[]> SubplotDivision(
            IList<Vector2> initialShape, 
            float maximumArea, 
            uint seed = 58, 
            float cutVariation = 0.0f,
            float minimumEdgeLength = 0.0f,
            bool snapPoints = false,
            bool fallbackToLength = false
            )
        {
            cutVariation = Numbers.Clamp01(cutVariation);
            RandomGenerator rgen = new (seed);
            float shortestEdgeLength = float.MaxValue;
            for (int i = 0; i < initialShape.Count; i++)
            {
                int next = i < initialShape.Count - 1 ? i + 1 : 0;
                float length = (initialShape[i] - initialShape[next]).magnitude;
                shortestEdgeLength = Mathf.Min(shortestEdgeLength, length);
            }
            minimumEdgeLength = Mathf.Clamp(minimumEdgeLength, Numbers.kEpsilon, shortestEdgeLength);
            cutVariation = Mathf.Clamp01(cutVariation);
            
            List<Vector2[]> output = new();
            Queue<Vector2[]> process = new();
            Queue<Vector2[]> fallbackProcess = new();
            process.Enqueue(initialShape.ToArray());
            float initialArea = Area.Calculate(initialShape);
            maximumArea = Mathf.Max(initialArea / 10000f, maximumArea);
            bool hasMinimumEdgeLength = minimumEdgeLength > Numbers.kEpsilon;
            
            if (initialArea < maximumArea)
            {
                return process.ToList();
            }

            int maxIt = Numbers.RoundToInt(Math.Max(1, initialArea / maximumArea)) * 2;
            while (process.Count > 0 || fallbackProcess.Count > 0)
            {
                Vector2[] current;
                bool isFallback = false;
                if (fallbackProcess.Count > 0)
                {
                    isFallback = true;
                    current = fallbackProcess.Dequeue();
                }
                else
                {
                    current = process.Dequeue();
                }
                
                OBBox obBox = OBBox.Fit(current);
                if (obBox.area < Mathf.Epsilon)
                {
                    //invalid shape was generated - ignore it
                    continue;
                }

                float sliceOffset = cutVariation < Mathf.Epsilon ? 0.5f : rgen.Range(-cutVariation * 0.5f, cutVariation * 0.5f) + 0.5f;
                float snapPointLength = 0;
                if (snapPoints && hasMinimumEdgeLength)
                {
                    snapPointLength = minimumEdgeLength;
                }
                
                // calculate the cut based on if this shape has had any cuts attempted and failed
                Vector2[][] splits = isFallback ?
                    OBBLongSlice(current, obBox, sliceOffset, snapPointLength):
                    OBBSquareSlice(current, obBox, sliceOffset, snapPointLength);
                
                int splitCount = splits.Length;
                bool validSplit = true;

                // split was not executed - dump the shape to output and continue
                if (splitCount != 2)
                {
                    if (fallbackToLength && !isFallback)
                    {
                        fallbackProcess.Enqueue(current);
                    }
                    else
                    {
                        output.Add(current);
                    }

                    continue;
                }
                
                float area0 = Area.Calculate(splits[0]);
                float area1 = Area.Calculate(splits[1]);

                if (area0 < Numbers.kEpsilon || area1 < Numbers.kEpsilon)
                {
                    if (fallbackToLength && !isFallback)
                    {
                        fallbackProcess.Enqueue(current);
                    }
                    else
                    {
                        output.Add(current);
                    }

                    continue;
                }

                // if we have a minimum length and we're not snapping points - we need to assess if
                // the output split is valid or illegal
                // if it is illegal we need to not use it
                if (hasMinimumEdgeLength && !snapPoints)
                {
                    for (int s = 0; s < splitCount; s++)
                    {
                        Vector2[] splitShape = splits[s];
                        int shapeCount = splitShape.Length;
                        for(int p = 0; p < shapeCount; p++)
                        {
                            int pb = p < shapeCount - 1 ? p + 1 : 0;
                            float distance = Vector2.Distance(splitShape[p], splitShape[pb]);
                            if(distance < minimumEdgeLength)
                            {
                                validSplit = false;
                                break;
                            }
                        }

                        if (!validSplit)
                        {
                            break;
                        }
                    }
                }

                if (!validSplit)
                {
                    if (fallbackToLength && !isFallback)
                    {
                        fallbackProcess.Enqueue(current);
                    }
                    else
                    {
                        output.Add(current);
                    }

                    continue;
                }
                
                // add the split shapes to the processing queue or 
                
                if (area0 < maximumArea)
                {
                    output.Add(splits[0]);
                }
                else
                {
                    process.Enqueue(splits[0]);
                }

                if (area1 < maximumArea)
                {
                    output.Add(splits[1]);
                }
                else
                {
                    process.Enqueue(splits[1]);
                }

                maxIt--;
                if (process.Count > 0 && maxIt == 0)
                {
                    Debug.Log("MAXIT");
                    Debug.Log(initialArea);
                    Debug.Log(maximumArea);
                    Debug.Log(process.Count);
                    Debug.Log(Numbers.RoundToInt(Math.Max(1, initialArea / maximumArea)) * 2);
                    output.AddRange(process); //dump the process stack onto the output
                    break;
                }
            }

            return output.ToList();
        }

        public static List<Vector2[]> SubplotDivisionConcave(
            IList<Vector2> initialShape,
            float maximumArea,
            uint seed = 58,
            float cutVariation = 0.0f,
            float minimumEdgeLength = 0.0f,
            bool snapPoints = false,
            bool fallbackToLength = false
        )
        {
            List<Vector2[]> output = new();
            List<List<Vector2>> convexShapes = Convex.Decompose2(initialShape);
            foreach (List<Vector2> convexShape in convexShapes)
            {
                List<Vector2[]> splitShapes = SubplotDivision(
                    convexShape,
                    maximumArea,
                    seed,
                    cutVariation,
                    minimumEdgeLength,
                    snapPoints,
                    fallbackToLength
                );
                output.AddRange(splitShapes);
            }

            return output;
        }


        public static List<Vector2[]> Executeodl(Vector2[] shape, float maximumArea)
        {
            List<Vector2[]> output = new();
            List<Vector2[]> processShapes = new() { shape };
            AABBox bounds = new(shape);
            float boundArea = bounds.Area();
            int roughMaxIt = Mathf.CeilToInt(boundArea / maximumArea);
            roughMaxIt *= roughMaxIt;//square it

            while (processShapes.Count > 0)
            {
                Vector2[] current = processShapes[0];
                processShapes.RemoveAt(0);
                
                OBBox obBox = OBBox.Fit(current);
                if (obBox.area < Mathf.Epsilon)
                {
                    output.Add(current);
                    continue;
                }
                
                float longSize = obBox.shortSize;
                Vector2 longDir = obBox.shortDir;
                Vector2 cutCenter = obBox.center;
                Vector2 intExt = longDir * longSize;
                Vector2 intP0 = cutCenter - intExt;
                Vector2 intP1 = cutCenter + intExt;
                    
                Vector2[][] splits = Slice(current, intP0, intP1);
                int splitCount = splits.Length;
                bool useSplit = true;
                bool[] processSplit = new bool[splitCount];//check the splits are still too big
                for (int s = 0; s < splitCount; s++)
                {
                    Vector2[] split = splits[s];
                    int splitSize = split.Length;
                    //check the splits are valid shapes
                    if (splitSize < 3)
                    {
                        useSplit = false;
                        break;
                    }
                    
                    float splitArea = Area.Calculate(split);
                    processSplit[s] = splitArea > maximumArea;
                }

                if (useSplit)
                {
                    for (int s = 0; s < splitCount; s++)
                    {
                        Vector2[] split = splits[s];
                        if(processSplit[s])
                        {
                            processShapes.Add(split);
                        }
                        else
                        {
                            output.Add(split);
                        }
                    }
                }
                else
                {
                    output.Add(current);
                }

                roughMaxIt--;
                if (roughMaxIt == 0)
                {
                    Debug.LogError("roughMaxIt");

                    string outputData = "";
                    for (int i = 0; i < shape.Length; i++)
                    {
                        Vector2 point = shape[i];
                        outputData += $"{i} {point.x} - {point.y}\n";
                    }

                    Debug.Log(outputData);
                    break;
                }
            }
            
            return output;
        }
    }
}