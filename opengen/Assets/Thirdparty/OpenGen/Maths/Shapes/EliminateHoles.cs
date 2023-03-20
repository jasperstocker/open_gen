using System;
using System.Collections.Generic;
using opengen.types;
using UnityEngine;

namespace opengen.maths.shapes
{
    public class EliminateHoles
    {
        // link every hole into the outer loop, producing a single-ring polygon without holes
        public static Vector2[] Execute(Vector2[] shape, Vector2[][] holes)
        {
            int holeNumber = holes.Length;
            bool[] used = new bool[holeNumber];
            for (int i = 0; i < holeNumber; i++)
            {
                int leftHole = 0;
                float leftValue = float.PositiveInfinity;
                for (int h = 0; h < holeNumber; h++)
                {
                    if(used[h])
                    {
                        continue;
                    }

                    Vector2[] hole = holes[h];
                    int leftIndex = GetLeftmost(hole);
                    Vector2 leftPoint = hole[leftIndex];
                    if (leftPoint.x < leftValue)
                    {
                        leftValue = leftPoint.x;
                        leftHole = h;
                    }
                }

                used[leftHole] = true;
                Vector2[] useHole = holes[leftHole];
                shape = EliminateHole(shape, useHole);
            }

            return shape;
        }

        // find a bridge between vertices that connects hole with an outer ring and and link it
        public static Vector2[] EliminateHole(Vector2[] shape, Vector2[] hole)
        {
            Vector2[] output = shape;
            int bridgePointIndex = FindHoleBridge(shape, hole);
            if (bridgePointIndex != -1)
            {
                output = MergePolygon(shape, hole, bridgePointIndex);
                output = Simplify.Execute2(output);
            }
            return output;
        }
        
        //return index of hold bridge in shape
        static int FindHoleBridge(Vector2[] shape, Vector2[] hole)
        {
            int holeLeftmostIndex = GetLeftmost(hole);
            Vector2 holeLeftPoint = hole[holeLeftmostIndex];
            float hx = holeLeftPoint.x;
            float hy = holeLeftPoint.y;
            float qx = float.NegativeInfinity;
            int mIndex = -1;
            int shapeSize = shape.Length;

            // find a segment intersected by a ray from the hole's leftmost point to the left;
            // segment's endpoint with lesser x will be potential connection point
            for (int i = 0; i < shapeSize; i++)
            {
                Vector2 pvector = shape[i];
                int ib = i < shapeSize - 1 ? i + 1 : 0;
                Vector2 pnext = shape[ib];
                
                if (hy <= pvector.y && hy >= pnext.y && pnext.y != pvector.y)
                {
                    var x = pvector.x + (hy - pvector.y) * (pnext.x - pvector.x) / (pnext.y - pvector.y);
                    if (x <= hx && x > qx)
                    {
                        qx = x;
                        if (x == hx)
                        {
                            if (hy == pvector.y)
                            {
                                return i;
                            }

                            if (hy == pnext.y)
                            {
                                return ib;
                            }
                        }
                        mIndex = pvector.x < pnext.x ? i : ib;
                    }
                }
            }

            if (mIndex == -1)
            {
                return -1;
            }

            if (hx == qx)
            {
                return Numbers.PreviousIndex(mIndex, shapeSize); // hole touches outer segment; pick lower endpoint
            }

            // look for points inside the triangle of hole point, segment intersection and endpoint;
            // if there are no points found, we have a valid connection;
            // otherwise choose the point of the minimum angle with the ray as connection point

            Vector2 m = shape[mIndex];
            float mx = m.x;
            float my = m.y;
            float tanMin = float.PositiveInfinity;

            int output = mIndex;
            for (int i = 0; i < shapeSize; i++)
            {
                int pIndex = (i + mIndex) % shapeSize;
                Vector2 p = shape[pIndex];
                
                if (hx >= p.x && p.x >= mx && hx != p.x && PointInTriangle(hy < my ? hx : qx, hy, mx, my, hy < my ? qx : hx, hy, p.x, p.y))
                {
                    float tan = Mathf.Abs(hy - p.y) / (hx - p.x);

                    if ((tan < tanMin || (tan == tanMin && p.x > m.x)) && LocallyInside(shape, pIndex, holeLeftPoint))
                    {
                        output = pIndex;
                        tanMin = tan;
                    }
                }
            }

            return output;
        }

        // link two polygon vertices with a bridge; if the vertices belong to the same ring, it splits polygon into two;
        // if one belongs to the outer ring and another to a hole, it merges it into a single ring
        static Vector2[] MergePolygon(Vector2[] a, Vector2[] b, int atIndexA)
        {
            int aSize = a.Length;
            if (atIndexA >= aSize)
            {
                throw new Exception("At Index A exceeds the size of Shape A");
            }

            int bSize = b.Length;
            int oSize = aSize + bSize + 2;
            Vector2[] output = new Vector2[oSize];

            int thresholdB = atIndexA + bSize + 1;
            for (int i = 0; i < oSize; i++)
            {
                Vector2 point;

                if (i <= atIndexA)
                {
                    point = a[i];
                }
                else if (i <= thresholdB)
                {
                    int indexB = i - atIndexA - 1;
                    if (indexB == bSize)//account for final point to loop it round
                    {
                        indexB += -bSize;
                    }

                    indexB = bSize - indexB - 1;//reverse it
                    point = b[indexB];
                }
                else
                {
                    point = a[i - bSize - 2];
                }

                output[i] = point;
            }
            
            return output;
        }
        
        static int CompareX(Vector2 a, Vector2 b)
        {
            return Mathf.RoundToInt(Mathf.Sign(a.x - b.x));
        }

        // find the leftmost node of a polygon ring
        static int GetLeftmost(Vector2[] shape)
        {
            int output = 0;
            int shapeSize = shape.Length;
            for (int i = 1; i < shapeSize; i++)
            {
                if (shape[i].x < shape[output].x)
                {
                    output = i;
                }
            }
            return output;
        }

        // check if a point lies within a convex triangle
        static bool PointInTriangle(float ax, float ay, float bx, float by, float cx, float cy, float px, float py)
        {
            return (cx - px) * (ay - py) - (ax - px) * (cy - py) >= 0 &&
                   (ax - px) * (by - py) - (bx - px) * (ay - py) >= 0 &&
                   (bx - px) * (cy - py) - (cx - px) * (by - py) >= 0;
        }

        // check if a polygon diagonal is locally inside the polygon
        static bool LocallyInside(Vector2[] aShape, int indexA, Vector2 point)
        {
            int aSize = aShape.Length;
            int indexAPrev = Numbers.PreviousIndex(indexA, aSize);
            int indexANext = Numbers.NextIndex(indexA, aSize);

            Vector2 aprev = aShape[indexAPrev];
            Vector2 acurr = aShape[indexA];
            Vector2 anext = aShape[indexANext];
            
            return Area(aprev, acurr, anext) < 0 ?
                Area(acurr, point, anext) >= 0 && Area(acurr, aprev, point) >= 0 :
                Area(acurr, point, aprev) < 0 || Area(acurr, anext, point) < 0;
        }

        // signed area of a triangle
        static float Area(Vector2 p, Vector2 q, Vector2 r)
        {
            return (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
        }
        
        
        // static LinkedVector2 Execute(List<Vector2> data, List<List<Vector2>> holes)
        // {
        //     var queue = new List<LinkedVector2>();
        //
        //     int holeCount = holes.Count;
        //     var len = holes.Count;
        //
        //     for (var i = 0; i < len; i++)
        //     {
        //         var start = holeIndices[i];
        //         var end = i < len - 1 ? holeIndices[i + 1] : data.Count;
        //         var list = LinkedList(data, start, end, false);
        //         if (list == list.next)
        //         {
        //             list.steiner = true;
        //         }
        //
        //         queue.Add(GetLeftmost(list));
        //     }
        //
        //     queue.Sort(CompareX);
        //
        //     // process holes from left to right
        //     for (var i = 0; i < queue.Count; i++)
        //     {
        //         EliminateHole(queue[i], outerNode);
        //         outerNode = FilterPoints(outerNode, outerNode.next);
        //     }
        //
        //     return outerNode;
        // }
        //
        // static int CompareX(Vector2 a, Vector2 b)
        // {
        //     return Mathf.RoundToInt(Mathf.Sign(a.x - b.x));
        // }
        //
        // // find a bridge between vertices that connects hole with an outer ring and and link it
        // static void EliminateHole(Vector2 hole, Vector2 outerNode)
        // {
        //     outerNode = FindHoleBridge(hole, outerNode);
        //     if (outerNode != null)
        //     {
        //         var b = SplitPolygon(outerNode, hole);
        //         FilterPoints(b, b.next);
        //     }
        // }
        //
        // static LinkedVector2 LinkedList(List<Vector2> data, int start, int end, bool clockwise)
        // {
        //     var last = default(LinkedVector2);
        //
        //     if (clockwise == (SignedArea(data, start, end) > 0))
        //     {
        //         for (int i = start; i < end; i += 2)
        //         {
        //             last = InsertNode(i, data[i], data[i + 1], last);
        //         }
        //     }
        //     else
        //     {
        //         for (int i = end - 2; i >= start; i -= 2)
        //         {
        //             last = InsertNode(i, data[i], data[i + 1], last);
        //         }
        //     }
        //
        //     if (last != null && Equals(last, last.next))
        //     {
        //         RemoveNode(last);
        //         last = last.next;
        //     }
        //
        //     return last;
        // }
        //
        // static float SignedArea(List<Vector2> data, int start, int end)
        // {
        //     var sum = default(float);
        //
        //     for (int i = start, j = end - 2; i < end; i += 2)
        //     {
        //         sum += (data[j].x - data[i].x) * (data[i].y + data[j + 1].y);
        //         j = i;
        //     }
        //
        //     return sum;
        // }
    }
}