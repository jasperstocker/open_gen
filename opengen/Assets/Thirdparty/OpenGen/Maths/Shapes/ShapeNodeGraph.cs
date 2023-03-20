using System;
using System.Collections.Generic;
using opengen.types;
using UnityEngine;

namespace opengen.maths.shapes
{
    public class ShapeNodeGraph
    {
        private readonly List<Node> _nodes;
        private readonly List<Edge> _edges;

        public List<Node> nodes => _nodes;
        public List<Edge> edges => _edges;
        
        public ShapeNodeGraph(Vector2[] shape)
        {
            int pointCount = shape.Length;
            Debug.Log(pointCount);
            _nodes = new List<Node>(pointCount);
            _edges = new List<Edge>(pointCount);
            for (int i = 0; i < pointCount; i++)
            {
                _nodes.Add(new Node(shape[i]));
            }

            for (int i = 0; i < pointCount; i++)
            {
                int ib = i < pointCount - 1 ? i + 1 : 0;

                Node n0 = _nodes[i];
                Node n1 = _nodes[ib];
                Edge newEdge = n0.Connect(n1);
                Debug.Log(newEdge);
                _edges.Add(newEdge);
            }
            
            //break down intersections
            int edgeCount = _edges.Count;
            for (int e = 0; e < edgeCount; e++)
            {
                Debug.Log(e);
                Edge e0 = _edges[e];
                Debug.Log(e0);
                Vector2 p0 = e0.a.position;
                Vector2 p1 = e0.b.position;

                for (int f = e + 2; f < edgeCount; f++)
                {
                    Edge e1 = _edges[f];
                    Vector2 p2 = e1.a.position;
                    Vector2 p3 = e1.b.position;
                    
                    if (Segments.FastIntersection(p0, p1, p2, p3))
                    {
                        Vector2 intersectionPoint = Segments.FindIntersection(p0, p1, p2, p3);

                        Node intersectionNode = new(intersectionPoint);
                        _nodes.Add(intersectionNode);
                        
                        Edge[] newEdges0 = e0.Split(intersectionNode);
                        Edge[] newEdges1 = e1.Split(intersectionNode);

                        edgeCount += 2;
                        _edges.Capacity = edgeCount;
                            
                        _edges.Remove(e0);
                        _edges.Remove(e1);
                        
                        _edges.AddRange(newEdges0);
                        _edges.AddRange(newEdges1);

                        e--;//go back to do the next edge after the current was deleted
                        break;
                    }
                }
            }

            //set indices for quick look up
            for (int i = 0; i < _nodes.Count; i++)
            {
                _nodes[i].index = i;
            }

            for (int i = 0; i < _edges.Count; i++)
            {
                _edges[i].index = i;
            }
        }

        public List<Vector2[]> GenerateShapes()
        {
            List<Vector2[]> output = new();
            int graphEdgeSize = _edges.Count;
            bool[] edgeTraversed = new bool[graphEdgeSize];
            
            Edge start = _edges[0];
            Edge current = start;
            edgeTraversed[0] = true;
            List<Vector2> newShape = new();
            do
            {
                newShape.Add(current.a.position);
                int currentIndex = current.index;
                edgeTraversed[currentIndex] = true;
                current = current.NextClockwiseEdge();
            } 
            while (start != current);
            
            output.Add(newShape.ToArray());

            return output;
        }
    }

    public class Node
    {
        public int index = -1;
        public Node(Vector2 position)
        {
            this.position = position;
        }

        public Vector2 position { get; }
        public List<Edge> connections { get; } = new();

        public Edge GetEdge(Node node)
        {
            if (node == this)
            {
                return null;
            }

            int size = connections.Count;
            for(int i = 0; i < size; i++)
            {
                Edge edge = connections[i];
                if (edge.Contains(node))
                {
                    return edge;
                }
            }
            return null;
        }

        public bool Contains(Edge edge)
        {
            Node other = edge.GetOther(this);//find the other node so we can check if this edge exists already
            if (other == null)//supplied edge not linked to this node
            {
                return false;
            }

            //check if the edge is contained in the node connections already
            int size = connections.Count;
            for(int i = 0; i < size; i++)
            {
                Edge connection = connections[i];
                if (connection.Contains(this) && connection.Contains(other))
                {
                    return true;
                }
            }

            return false;
        }

        public Edge Connect(Node node)
        {
            if (node == this)
            {
                Debug.Log("edge nodes the same?");
                return null;
            }
            Edge output = GetEdge(node);
            Debug.Log("get edge "+output+" "+(output==null));
            if (output == null)
            {
                output = new Edge(this, node);
                output = node.Connect(output);
                if(output != null)
                {
                    connections.Add(output);
                }
            }
            return output;
        }

        public Edge Connect(Edge edge)
        {
            if (Contains(edge))
            {
                return null;
            }

            connections.Add(edge);
            return edge;
        }

        public bool Disconnect(Node connection)
        { 
            return Disconnect(GetEdge(connection));
        }

        public bool Disconnect(Edge connection)
        {
            if (Contains(connection))
            {
                Node other = connection.GetOther(this);
                connections.Remove(connection);
                other.Disconnect(connection);
                return true;
            }
            return false;
        }

        public float CalculateAngle(Edge edge)
        {
            float output = 0;
            Node other = edge.GetOther(this);
            if (other != null)
            {
                Vector2 origin = position;
                Vector2 from = other.position;
                Vector2 dir = (from - origin).normalized;
                output = Vector2.Angle(Vector2.up, dir);
                Vector3 cross = Vector3.Cross(Vector2.up, dir);
                if (cross.z > 0)
                {
                    output = -output;
                }
            }
            return output;
        }

        public Edge NextClockwiseEdge(Edge from)
        {
            int connectionCount = connections.Count;
            Vector2 baseVector = from.GetVector();
            Node baseNode = from.b;
            float lowestAngle = 360;
            Edge output = from;
            for (int i = 0; i < connectionCount; i++)
            {
                Edge edge = connections[i];
                if(edge == from)
                {
                    continue;
                }

                Vector2 edgeVector = edge.GetVector(baseNode);
                float angle = Angles.SignAngleRelative(baseVector, edgeVector);
                if (angle < 0)
                {
                    angle = 360 + angle;//convert angles to 0-359.
                }

                if (angle < lowestAngle)
                {
                    lowestAngle = angle;
                    output = edge;
                }
            }

            return output;
        }
        
        public override bool Equals(object obj)
        {
            Node node = obj as Node;
            return node != null && Equals(node);
        }

        public bool Equals(Node b)
        {
            return position == b.position;
        }

        public override int GetHashCode()
        {
            return position.GetHashCode();
        }

        public static bool operator ==(Node a, Node b)
        {
            bool aIsNull = ReferenceEquals(null, a);
            bool bIsNull = ReferenceEquals(null, b);
            if(aIsNull && bIsNull)
            {
                return true;
            }

            if(aIsNull || bIsNull)
            {
                return false;
            }

            return a.position == b.position;
        }

        public static bool operator !=(Node a, Node b)
        {
            bool aIsNull = ReferenceEquals(null, a);
            bool bIsNull = ReferenceEquals(null, b);
            if(aIsNull && bIsNull)
            {
                return false;
            }

            if(aIsNull || bIsNull)
            {
                return true;
            }

            return a.position != b.position;
        }

        public static bool operator ==(Vector2 a, Node b)
        {
            bool bIsNull = ReferenceEquals(null, b);
            if(bIsNull)
            {
                return false;
            }

            return a == b.position;
        }

        public static bool operator !=(Vector2 a, Node b)
        {
            bool bIsNull = ReferenceEquals(null, b);
            if(bIsNull)
            {
                return true;
            }

            return a != b.position;
        }

        public static bool operator ==(Node a, Vector2 b)
        {
            bool aIsNull = ReferenceEquals(null, a);
            if(aIsNull)
            {
                return false;
            }

            return a.position == b;
        }

        public static bool operator !=(Node a, Vector2 b)
        {
            bool aIsNull = ReferenceEquals(null, a);
            if(aIsNull)
            {
                return true;
            }

            return a.position != b;
        }

        public override string ToString()
        {
            return string.Format("node({0})", position);
        }
    }
    
    public class Edge
    {
        public int index = -1;
        public Node a { get; }
        public Node b { get; }
        private AABBox _bounds;

        public Edge(Node a, Node b)
        {
            if(a==b)
            {
                throw new Exception("Line formed from equal nodes!");
            }

            this.a = a;
            this.b = b;
            _bounds = new AABBox(new []{a.position, b.position});
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

        public bool Contains(Edge edge)
        {
            if(a == edge.a)
            {
                return true;
            }

            if(a == edge.b)
            {
                return true;
            }

            if(b == edge.a)
            {
                return true;
            }

            if(b == edge.b)
            {
                return true;
            }

            return false;
        }

        public Node GetOther(Node node)
        {
            if(a == node)
            {
                return b;
            }

            if(b == node)
            {
                return a;
            }

            return null;
        }

        public Vector2 GetVector(Node from)
        {
            Node other = GetOther(from);
            Vector2 output = Vector2.zero;
            if (other != null)
            {
                output = other.position - from.position;
            }

            return output;
        }

        public Vector2 GetVector()
        {
            return b.position - a.position;
        }

        public bool OverlapBounds(Edge other)
        {
            return _bounds.Overlaps(other._bounds);
        }

        public bool Disconnect()
        {
            return a.Disconnect(b);
        }

        public Edge[] Split(Node withNode)
        {
            Edge[] output = new Edge[2];
            a.Disconnect(this);
            b.Disconnect(this);
            output[0] = a.Connect(withNode);
            output[1] = withNode.Connect(b);
            return output;
        }

        public Edge NextClockwiseEdge()
        {
            return this.b.NextClockwiseEdge(this);
        }
        
        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Edge && Equals((Edge)obj);
        }

        public bool Equals(Edge b)
        {
            bool xc = a == b.a || a == b.b;
            bool yc = this.b == b.a || this.b == b.b;
            return xc && yc;
        }

        public override int GetHashCode()
        {
            return (a.position.x * a.position.y * b.position.x * b.position.y).GetHashCode();
        }

        public static bool operator ==(Edge a, Edge b)
        {
            bool anul = a is null;
            bool bnul = b is null;
            if (anul && bnul)
            {
                return true;
            }

            if (anul)
            {
                return false;
            }

            if (bnul)
            {
                return false;
            }

            bool xc = a.a == b.a || a.a == b.b;
            bool yc = a.b == b.a || a.b == b.b;
            return xc && yc;
        }

        public static bool operator !=(Edge a, Edge b)
        {
            bool anul = a is null;
            bool bnul = b is null;
            if (anul && bnul)
            {
                return false;
            }

            if (anul)
            {
                return true;
            }

            if (bnul)
            {
                return true;
            }

            bool xc = a.a == b.a || a.a == b.b;
            bool yc = a.b == b.a || a.b == b.b;
            return !xc || !yc;
        }

        public override string ToString()
        {
            return string.Format("edge<{0} - {1}>", a, b);
        }
    }
}