using System;
using opengen;
using opengen.types;
using UnityEngine;


namespace opengen.maths.shapes.shapefinder
{
    public struct NodeLine
    {
        public Node a;
        public Node b;
        public AABBox bounds;
        public bool ignore;

        public NodeLine(Node a, Node b)
        {
            if(a==b)
            {
                throw new Exception("Line formed from equal nodes!");
            }

            this.a = a;
            this.b = b;
            bounds = new AABBox(new []{a.position.vector2, b.position.vector2});
            ignore = false;
        }

        public bool Contains(Node node)
        {
            if(a == node)
            {
                return true;
            }

            if(b == node)
            {
                return true;
            }

            return false;
        }

        public bool Contains(NodeLine line)
        {
            if(a == line.a)
            {
                return true;
            }

            if(a == line.b)
            {
                return true;
            }

            if(b == line.a)
            {
                return true;
            }

            if(b == line.b)
            {
                return true;
            }

            return false;
        }

        public bool OverlapBounds(NodeLine other)
        {
            return bounds.Overlaps(other.bounds);
        }

        public bool Disconnect()
        {
            return a.Disconnect(b);
        }
        
        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is NodeLine && Equals((NodeLine)obj);
        }

        public bool Equals(NodeLine b)
        {
            bool xc = a == b.a || a == b.b;
            bool yc = this.b == b.a || this.b == b.b;
            return xc && yc;
        }

        public override int GetHashCode()
        {
            return (a.position.x * a.position.y * b.position.x * b.position.y).GetHashCode();
        }

        public static bool operator ==(NodeLine a, NodeLine b)
        {
            bool xc = a.a == b.a || a.a == b.b;
            bool yc = a.b == b.a || a.b == b.b;
            return xc && yc;
        }

        public static bool operator !=(NodeLine a, NodeLine b)
        {
            bool xc = a.a == b.a || a.a == b.b;
            bool yc = a.b == b.a || a.b == b.b;
            return !xc || !yc;
        }

        public override string ToString()
        {
            return string.Format("line<{0} - {1}>", a, b);
        }
    }
}