using System.Collections.Generic;
using opengen.maths.shapes;
using UnityEngine;

namespace opengen.types
{
    public class LinkedVector2
    {
        public int i;
        public Vector2 position;

        public int? z;

        public LinkedVector2 prev;
        public LinkedVector2 next;

        public LinkedVector2 prevZ;
        public LinkedVector2 nextZ;

        public bool steiner;

        public float x => position.x;
        public float y => position.y;

        public LinkedVector2(int i, Vector2 position)
        {
            // vertex index in coordinates array
            this.i = i;

            this.position = position;

            // previous and next vertex nodes in a polygon ring
            prev = null;
            next = null;

            // z-order curve value
            z = null;

            // previous and next nodes in z-order
            prevZ = null;
            nextZ = null;

            // indicates whether this is a steiner point
            steiner = false;
        }

        public LinkedVector2(int i, LinkedVector2 linkedVector2)
        {
            // vertex index in coordinates array
            this.i = i;

            position = linkedVector2.position;

            // previous and next vertex nodes in a polygon ring
            prev = null;
            next = null;

            // z-order curve value
            z = null;

            // previous and next nodes in z-order
            prevZ = null;
            nextZ = null;

            // indicates whether this is a steiner point
            steiner = false;
        }

        public static LinkedVector2 LinkedList(IList<Vector2> shape)
        {
            LinkedVector2 last = default(LinkedVector2);
            bool clockwise = Clockwise.Check(shape);
            int start = 0;
            int end = shape.Count;

            if (clockwise)
            {
                for (int i = start; i < end; i++)
                {
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
                RemoveNode(last);
                last = last.next;
            }

            return last?.next;
        }

        public static List<LinkedVector2> BuildFlatList(LinkedVector2 start)
        {
            List<LinkedVector2> output = new List<LinkedVector2> {start};
            LinkedVector2 next = start.next;
            while (next != start)
            {
                output.Add(next);
                next = next.next;
            }
            return output;
        }

        public static List<Vector2> Flatten(LinkedVector2 start)
        {
            List<Vector2> output = new List<Vector2> {start.position};
            LinkedVector2 next = start.next;
            while (next != start)
            {
                output.Add(next.position);
                next = next.next;
            }
            return output;
        }
        
        // find the leftmost node of a polygon ring
        public static LinkedVector2 GetLeftmost(LinkedVector2 start)
        {
            LinkedVector2 p = start;
            LinkedVector2 leftmost = start;
            do
            {
                if (p.x < leftmost.x)
                {
                    leftmost = p;
                }

                p = p.next;
            } while (p != start);

            return leftmost;
        }

        // check if a point lies within a convex triangle
        public static bool PointInTriangle(float ax, float ay, float bx, float by, float cx, float cy, float px, float py)
        {
            return (cx - px) * (ay - py) - (ax - px) * (cy - py) >= 0 &&
                   (ax - px) * (by - py) - (bx - px) * (ay - py) >= 0 &&
                   (bx - px) * (cy - py) - (cx - px) * (by - py) >= 0;
        }

        // check if a diagonal between two polygon nodes is valid (lies in polygon interior)
        public static bool IsValidDiagonal(LinkedVector2 a, LinkedVector2 b)
        {
            return a.next.i != b.i && a.prev.i != b.i && !IntersectsPolygon(a, b) &&
                   LocallyInside(a, b) && LocallyInside(b, a) && MiddleInside(a, b);
        }

        // signed area of a triangle
        public static float Area(LinkedVector2 p, LinkedVector2 q, LinkedVector2 r)
        {
            return (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
        }

        // check if two points are equal
        public static bool Equals(LinkedVector2 p1, LinkedVector2 p2)
        {
            return p1.x == p2.x && p1.y == p2.y;
        }

        // check if two segments intersect
        public static bool Intersects(LinkedVector2 p1, LinkedVector2 q1, LinkedVector2 p2, LinkedVector2 q2)
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
        public static bool IntersectsPolygon(LinkedVector2 a, LinkedVector2 b)
        {
            LinkedVector2 p = a;
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
        public static bool LocallyInside(LinkedVector2 a, LinkedVector2 b)
        {
            return Area(a.prev, a, a.next) < 0 ?
                Area(a, b, a.next) >= 0 && Area(a, a.prev, b) >= 0 :
                Area(a, b, a.prev) < 0 || Area(a, a.next, b) < 0;
        }

        // check if the middle point of a polygon diagonal is inside the polygon
        public static bool MiddleInside(LinkedVector2 a, LinkedVector2 b)
        {
            var p = a;
            var inside = false;
            var px = (a.x + b.x) / 2;
            var py = (a.y + b.y) / 2;
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
        public static LinkedVector2 SplitPolygon(LinkedVector2 a, LinkedVector2 b)
        {
            LinkedVector2 a2 = new LinkedVector2(a.i, a);
            LinkedVector2 b2 = new LinkedVector2(b.i, b);
            LinkedVector2 an = a.next;
            LinkedVector2 bp = b.prev;
            
            // Debug.Log($"{a2.i} {b2.i} {an.i} {bp.i}");

            //cut old shape at bridge
            a.next = b;
            b.prev = a;

            //join duplicate point a to new shape
            a2.next = an;
            an.prev = a2;
            //join duplicate points to each other
            a2.prev = b2;
            b2.next = a2;
            //join duplicate point b to new shape
            bp.next = b2;
            b2.prev = bp;

            return a2;
        }

        // create a node and optionally link it with previous one (in a circular doubly linked list)
        public static LinkedVector2 InsertNode(int i, Vector2 position, LinkedVector2 last)
        {
            var p = new LinkedVector2(i, position);

            if (last == null)
            {
                p.prev = p;
                p.next = p;

            }
            else
            {
                p.next = last.next;
                p.prev = last;
                last.next.prev = p;
                last.next = p;
            }
            return p;
        }

        public static void RemoveNode(LinkedVector2 p)
        {
            p.next.prev = p.prev;
            p.prev.next = p.next;

            if (p.prevZ != null)
            {
                p.prevZ.nextZ = p.nextZ;
            }

            if (p.nextZ != null)
            {
                p.nextZ.prevZ = p.prevZ;
            }
        }

        public static LinkedVector2 GetNodeByIndex(LinkedVector2 start, int index)
        {
            LinkedVector2 next = start.next;
            while (next != start)
            {
                if (next.i == index)
                {
                    return next;
                }

                next = next.next;
            }
            return start;
        }

        public static void DebugDraw(LinkedVector2 start)
        {
            LinkedVector2 next = start.next;
            while (next != start)
            {
                Vector2 p0 = next.prev.position;
                Vector2 p1 = next.position;

                Vector3 v0 = new Vector3(p0.x, 0, p0.y);
                Vector3 v1 = new Vector3(p1.x, 0, p1.y);

                Debug.DrawLine(v0, v1, Color.red, 10);
                next = next.next;
            }
        }
    }
}