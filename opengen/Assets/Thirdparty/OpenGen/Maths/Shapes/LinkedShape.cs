using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace opengen.maths.shapes
{
    public class LinkedShape
    {
        public static Node LinkedList(Vector2[] shape, int start, int end, bool clockwise)
        {
            var last = default(Node);

            if (clockwise == (SignedArea(shape, start, end) > 0f))
            {
                for (int i = start; i < end; i++)
                {
                    //Debug.Log(i+" "+last?.i);
                    last = InsertNode(i, shape[i], last);
                }
            }
            else
            {
                for (int i = end - 1; i >= start; i--)
                {
                    last = InsertNode(i, shape[i], last);
                }
            }

            if (last != null && Equals(last, last.next))
            {
                //Debug.Log("remove "+last.i);
                RemoveNode(last);
                last = last.next;
            }

            return last;
        }
        
        public static Node Find(Vector2 point, Node last)
        {
            var p = last;
            while (true)
            {
                //Debug.Log(p.point+" "+point+" "+Vector2.Distance(p.point,point).ToString("F8")+" - "+(Vector2.Distance(p.point,point) < 0.01f));
                if (Vector2.Distance(p.point,point) < 0.01f)
                {
                    return p;
                }

                p = p.next;
                if (p == last)
                {
                    break;
                }
            }
            return null;
        }
        
        public static Node FindOrigin(Vector2 point, Node last)
        {
            var p = last;
            while (true)
            {
                //Debug.Log(p.point+" "+point+" "+Vector2.Distance(p.point,point).ToString("F8")+" - "+(Vector2.Distance(p.point,point) < 0.01f));
                if (Vector2.Distance(p.point,point) < 0.01f)
                {
                    return p;
                }

                p = p.originNext;
                if (p == last)
                {
                    break;
                }
            }
            return null;
        }

        public static List<Node> GetAllNodes(Node last)
        {
            List<Node> output = new();
            var p = last;
            while (true)
            {
                output.Add(p);
                p = p.next;
                if (p == last)
                {
                    break;
                }
            }
            return output;
        }

        static float SignedArea(Vector2[] shape, int start, int end)
        {
            float sum = 0;

            if (end > shape.Length)
            {
                Debug.Log(shape.Length + " " + start + " " + end);//tut tut
                end = shape.Length;//let's just fix that for you bro...
            }

            for (int i = start, j = end - 1; i < end; i += 1)
            {
                sum += (shape[j].x - shape[i].x) * (shape[i].y + shape[j].y);
                j = i;
            }

            return sum;
        }
        
        
        // check if a diagonal between two polygon nodes is valid (lies in polygon interior)
        static bool IsValidDiagonal(Node a, Node b)
        {
            return a.next.i != b.i && a.prev.i != b.i && !IntersectsPolygon(a, b) &&
                   LocallyInside(a, b) && LocallyInside(b, a) && MiddleInside(a, b);
        }

        // signed area of a triangle
        static float Area(Node p, Node q, Node r)
        {
            return (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
        }

        // check if two points are equal
        static bool Equals(Node p1, Node p2)
        {
            return Math.Abs(p1.x - p2.x) < Mathf.Epsilon && Math.Abs(p1.y - p2.y) < Mathf.Epsilon;
        }

        // check if two segments intersect
        static bool Intersects(Node p1, Node q1, Node p2, Node q2)
        {
            if ((Equals(p1, q1) && Equals(p2, q2)) ||
                (Equals(p1, q2) && Equals(p2, q1)))
            {
                return true;
            }

            return Area(p1, q1, p2) > 0 != Area(p1, q1, q2) > 0 &&
                   Area(p2, q2, p1) > 0 != Area(p2, q2, q1) > 0;
        }

        // check if a polygon diagonal intersects any polygon segments
        static bool IntersectsPolygon(Node a, Node b)
        {
            Node p = a;
            do
            {
                if (p.i != a.i && p.next.i != a.i && p.i != b.i && p.next.i != b.i &&
                        Intersects(p, p.next, a, b))
                {
                    return true;
                }

                p = p.next;
            } while (p != a);

            return false;
        }

        // check if a polygon diagonal is locally inside the polygon
        static bool LocallyInside(Node a, Node b)
        {
            return Area(a.prev, a, a.next) < 0 ?
                Area(a, b, a.next) >= 0 && Area(a, a.prev, b) >= 0 :
                Area(a, b, a.prev) < 0 || Area(a, a.next, b) < 0;
        }

        // check if the middle point of a polygon diagonal is inside the polygon
        static bool MiddleInside(Node a, Node b)
        {
            var p = a;
            var inside = false;
            var px = (a.x + b.x) / 2f;
            var py = (a.y + b.y) / 2f;
            do
            {
                if (((p.y > py) != (p.next.y > py)) && p.next.y != p.y &&
                        (px < (p.next.x - p.x) * (py - p.y) / (p.next.y - p.y) + p.x))
                {
                    inside = !inside;
                }

                p = p.next;
            } while (p != a);

            return inside;
        }

        // link two polygon vertices with a bridge; if the vertices belong to the same ring, it splits polygon into two;
        // if one belongs to the outer ring and another to a hole, it merges it into a single ring
        static Node SplitPolygon(Node a, Node b)
        {
            var a2 = new Node(a);
            var b2 = new Node(b);
            var an = a.next;
            var bp = b.prev;

            a.next = b;
            b.prev = a;

            a2.next = an;
            an.prev = a2;

            b2.next = a2;
            a2.prev = b2;

            bp.next = b2;
            b2.prev = bp;

            return b2;
        }

        // create a node and optionally link it with previous one (in a circular doubly linked list)
        public static Node InsertNode(int i, Vector2 point, Node last)
        {
            var newNode = new Node(i, point);

            if (last == null)
            {
                newNode.prev = newNode;
                newNode.next = newNode;

            }
            else
            {
                newNode.next = last.next;
                newNode.prev = last;
                last.next.prev = newNode;
                last.next = newNode;
            }
            return newNode;
        }

        // create a node and optionally link it with previous one (in a circular doubly linked list)
        public static Node InsertNode(Node newNode, Node last)
        {
            if (last == null)
            {
                newNode.prev = newNode;
                newNode.next = newNode;
            }
            else
            {
                if (newNode.next == last)
                {
                    last.prev.next = newNode;
                    newNode.prev = last.prev;
                    last.prev = newNode;
                }
                else if (newNode.prev == last)
                {
                    last.next.prev = newNode;
                    newNode.next = last.next;
                    last.next = newNode;
                }
                else
                {
                    Debug.Log("Unhandled!");
                }
            }
            return newNode;
        }

        public static Node RemoveNode(Node p)
        {
            p.next.prev = p.prev;
            p.prev.next = p.next;
            return p.next;//return the next point so we know what exists!
        }
        
        public class Node
        {
            public int i;
            public Vector2 point;
            public Node prev;
            public Node next;
            public bool steiner;

            private Node _originPrev;
            private Node _originNext;

            public float x => point.x;
            public float y => point.y;
            public Node originPrev => _originPrev;
            public Node originNext => _originNext;
            public bool unmodified => prev == _originPrev && next == _originNext;

            public Node(int i, Vector2 point)
            {
                // vertex index in coordinates array
                this.i = i;

                // vertex coordinates
                this.point = point;

                // previous and next vertex nodes in a polygon ring
                this.prev = null;
                this.next = null;

                // indicates whether this is a steiner point
                this.steiner = false;
            }
            public Node(Node clone)
            {
                // vertex index in coordinates array
                this.i = clone.i;

                // vertex coordinates
                this.point = clone.point;

                // previous and next vertex nodes in a polygon ring
                this.prev = null;
                this.next = null;

                // indicates whether this is a steiner point
                this.steiner = false;
            }

            public void SetOrigin()
            {
                _originPrev = prev;
                _originNext = next;
            }
        }

        public static void SetOrigins(Node start)
        {
            var p = start;
            while (true)
            {
                p.SetOrigin();
                p = p.next;
                if (p == start)
                {
                    break;
                }
            }
        }
        
        // eliminate colinear or duplicate points
        static Node FilterPoints(Node start, Node end = null)
        {
            if (start == null)
            {
                return start;
            }

            if (end == null)
            {
                end = start;
            }

            var p = start;
            bool again;

            do
            {
                again = false;

                if (!p.steiner && (Equals(p, p.next) || Area(p.prev, p, p.next) == 0))
                {
                    RemoveNode(p);
                    p = end = p.prev;
                    if (p == p.next)
                    {
                        break;
                    }

                    again = true;

                }
                else
                {
                    p = p.next;
                }
            } while (again || p != end);

            return end;
        }
        
        //hole code
        static void EliminateHole(Node hole, Node outerNode)
        {
            outerNode = FindHoleBridge(hole, outerNode);
            if (outerNode != null)
            {
                var b = SplitPolygon(outerNode, hole);
                FilterPoints(b, b.next);
            }
        }

        // David Eberly's algorithm for finding a bridge between hole and outer polygon
        static Node FindHoleBridge(Node hole, Node outerNode)
        {
            var p = outerNode;
            var hx = hole.x;
            var hy = hole.y;
            var qx = float.NegativeInfinity;
            Node m = null;

            // find a segment intersected by a ray from the hole's leftmost point to the left;
            // segment's endpoint with lesser x will be potential connection point
            do
            {
                if (hy <= p.y && hy >= p.next.y && p.next.y != p.y)
                {
                    var x = p.x + (hy - p.y) * (p.next.x - p.x) / (p.next.y - p.y);
                    if (x <= hx && x > qx)
                    {
                        qx = x;
                        if (x == hx)
                        {
                            if (hy == p.y)
                            {
                                return p;
                            }

                            if (hy == p.next.y)
                            {
                                return p.next;
                            }
                        }
                        m = p.x < p.next.x ? p : p.next;
                    }
                }
                p = p.next;
            } while (p != outerNode);

            if (m == null)
            {
                return null;
            }

            if (hx == qx)
            {
                return m.prev; // hole touches outer segment; pick lower endpoint
            }

            // look for points inside the triangle of hole point, segment intersection and endpoint;
            // if there are no points found, we have a valid connection;
            // otherwise choose the point of the minimum angle with the ray as connection point

            var stop = m;
            var mx = m.x;
            var my = m.y;
            var tanMin = float.PositiveInfinity;
            float tan;

            p = m.next;

            while (p != stop)
            {
                if (hx >= p.x && p.x >= mx && hx != p.x && PointInTriangle(hy < my ? hx : qx, hy, mx, my, hy < my ? qx : hx, hy, p.x, p.y))
                {

                    tan = Math.Abs(hy - p.y) / (hx - p.x); // tangential

                    if ((tan < tanMin || (tan == tanMin && p.x > m.x)) && LocallyInside(p, hole))
                    {
                        m = p;
                        tanMin = tan;
                    }
                }

                p = p.next;
            }

            return m;
        }

        // check if a point lies within a convex triangle
        static bool PointInTriangle(float ax, float ay, float bx, float by, float cx, float cy, float px, float py)
        {
            return (cx - px) * (ay - py) - (ax - px) * (cy - py) >= 0 &&
                   (ax - px) * (by - py) - (bx - px) * (ay - py) >= 0 &&
                   (bx - px) * (cy - py) - (cx - px) * (by - py) >= 0;
        }
        

        // check whether a polygon node forms a valid ear with adjacent nodes
        public static bool IsEar(Node ear)
        {
            var a = ear.prev;
            var b = ear;
            var c = ear.next;

            if (Area(a, b, c) >= 0)
            {
                return false; // reflex, can't be an ear
            }

            // now make sure we don't have other points inside the potential ear
            var p = ear.next.next;

            while (p != ear.prev)
            {
                if (PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y) &&
                    Area(p.prev, p, p.next) >= 0)
                {
                    return false;
                }

                p = p.next;
            }

            return true;
        }

        public static bool IsReflex(Node node, int sign)
        {

            float cross = Cross(node);
            int pointSign = cross > 0 ? -1 : 1;
            return pointSign != sign;
        }

        private static float Cross(Node node)
        {
            Node prev = node.prev;
            Node next = node.next;
            
            Vector2 pa = prev.point;
            Vector2 pb = node.point;
            Vector2 pc = next.point;
            
            return Cross(pa, pb, pc);
        }

        private static float Cross(Vector2 a, Vector2 b, Vector2 c)
        {
            float x1 = b.x - a.x;
            float y1 = b.y - a.y;
            float x2 = c.x - b.x;
            float y2 = c.y - b.y;
            return x1 * y2 - x2 * y1;
        }

        public static int CalculateSign(Node outer)
        {
            int plus = 0;
            int minus = 0;
            Node node = outer;
            Node next = outer.next;
            while (next != outer)
            {
                float cross = Cross(node);
                bool sign = cross < 0;
                if (sign)
                {
                    plus++;
                }
                else
                {
                    minus++;
                }

                node = next;
                next = node.next;
            }

            return plus > minus ? 1 : -1;
        }

        public static void DebugLog(Node ear)
        {
            var p = ear;

            StringBuilder sb = new();
            while (true)
            {
                sb.Append($" >>> [{p.prev.i}(({p.i})){p.next.i}]");
                p = p.next;
                if (p == ear)
                {
                    break;
                }
            }
            Debug.Log(sb.ToString());
        }

        public static void DebugLog(Vector2[] shape)
        {
            int shapeSize = shape.Length;
            StringBuilder sb = new();
            for (int s = 0; s < shapeSize; s++)
            {
                sb.Append($" >>> [{shape[s].ToString("F3")}]");
            }

            Debug.Log(sb.ToString());
        }

        public static Vector2[][] RebuildShapes(List<Node> fromNodes)
        {
            List<Vector2[]> output = new();
            int nodeCount = fromNodes.Count;
            //if node list empty there is nothing to do
            //a single node may have two other connections to add to the residual (makes a triangle)
            if (nodeCount < 1)
            {
                return output.ToArray();
            }

            // //make a list of all the nodes that are to be used
            // //we will find the original connecting nodes to the ones supplied
            List<Node> useNodes = new(fromNodes);//todo
            // for (int i = 0; i < nodeCount; i++)
            // {
            //     Node node0 = fromNodes[i];
            //     useNodes.Add(node0);
            //     if (!fromNodes.Contains(node0.originPrev))
            //     {
            //         Node prev = node0.originPrev;
            //         node0.prev = prev;
            //         prev.next = node0;
            //         InsertNode(prev, node0);
            //         useNodes.Add(prev);
            //     }
            //     if (!fromNodes.Contains(node0.originNext))
            //     {
            //         Node next = node0.originNext;
            //         node0.next = next;
            //         next.prev = node0;
            //         InsertNode(next, node0);
            //         useNodes.Add(next);
            //     }
            // }
            // Debug.Log(useNodes.Count);

            //build the residual shapes from the linked lists
            //there may be multiple shapes to generate
            //at this point we will need at least a triangle to work with - otherwise there are no residual shapes...
            nodeCount = useNodes.Count;
            if (nodeCount < 3)
            {
                return output.ToArray();
            }

            int outputCount = 0;
            Node start = useNodes[0];
            Node current = start;
            List<Vector2> newShape = new();
            while (useNodes.Count > 0)
            {
                newShape.Add(current.point);
                Debug.Log("ADD>> "+current.point);
                useNodes.Remove(current);
                RemoveNode(current);

                if (start != current.next)
                {
                    current = current.next;
                }
                else
                {
                    output.Add(newShape.ToArray());
                    start = useNodes[0];
                    current = start;
                    newShape.Clear();
                }
                
                outputCount++;
                if (outputCount > nodeCount)
                {
                    break;
                }
            }
            output.Add(newShape.ToArray());//add the final generated shape


            return output.ToArray();
        }
    }
}