using System;
using System.Collections.Generic;
using opengen;
using opengen.types;
using UnityEngine;



namespace opengen.maths.shapes.shapefinder
{
    public class Node : INode
    {
//        private static float LENGTH = 25;

        private Vector2Fixed _position;
        private List<Node> _connections = new();
        private List<Node> _ordereredConnections = new();
        private Bitmask _plotted = new();//used by the plot generation
        private int _connectionCountSqaure;
//        public bool bypass = false;
        
        public Node(Vector2Fixed position)
        {
            _position = position;
        }

        public Vector2Fixed position { get { return _position; } }
        public List<Node> connections { get { return _connections; } }

        INode[] INode.connections
        {
            get {return _connections.ToArray();}
        }

        public bool Contains(Node other)
        {
            int size = _connections.Count;
            for(int i = 0; i < size; i++)
            {
                if(other == _connections[i])
                {
                    return true;
                }
            }
            return false;
        }

        public bool Connect(Node connection)
        {
            if (this != connection && !Contains(connection))
            {
                _connections.Add(connection);
                connection.Connect(this);
                return true;
            }
            return false;
        }

        public bool Disconnect(Node connection)
        {
            if (Contains(connection))
            {
                _connections.Remove(connection);
                connection.Disconnect(this);
                return true;
            }
            return false;
        }

        public void OrderConnections()
        {
            _connectionCountSqaure = (int)Mathf.Pow(2, _connections.Count) - 1;
            List<float> _angles = new();
            List<Node> _unordered = new();
            foreach (Node connection in _connections)
            {
                Vector2 dir = (connection.position - position).vector2.normalized;
                float angle = Vector2.Angle(Vector2.up, dir);
                Vector3 dirV3 = new(dir.x, dir.y, 0);
                Vector3 cross = Vector3.Cross(Vector3.up, dirV3);
                if (cross.z > 0)
                {
                    angle = -angle;
                }

                _angles.Add(angle);
                _unordered.Add(connection);
            }

            _ordereredConnections.Clear();
            while (_unordered.Count > 0)
            {
                int index = 0;
                float minANgle = _angles[0];
                for (int i = 1; i < _unordered.Count; i++)
                {
                    float angle = _angles[i];
                    if (angle < minANgle)
                    {
                        index = i;
                        minANgle = angle;
                    }
                }
                Node smallestNode = _unordered[index];
                _unordered.RemoveAt(index);
                _angles.RemoveAt(index);
                _ordereredConnections.Add(smallestNode);
            }
        }

        public Node NextNode(Node lastNode = null)
        {
            int orderCount = _ordereredConnections.Count;
            if (orderCount == 0)
            {
                return _connections[0];
            }

            int lastIndex = -1;
            if (lastNode != null)
            {
                lastIndex = _ordereredConnections.IndexOf(lastNode);
            }

            //            Debug.Log(lastIndex);
            return _ordereredConnections[(lastIndex + 1) % orderCount];
        }

        public Node LastNode(Node node = null)
        {
            int orderCount = _ordereredConnections.Count;
            if (orderCount == 0)
            {
                return _connections[0];
            }

            int lastIndex = -1;
            if (node != null)
            {
                lastIndex = _ordereredConnections.IndexOf(node);
            }

            //            lastIndex = (lastIndex - 1 + orderCount) % orderCount;
            Node output = null;//_ordereredConnections[];
            int it = orderCount;
            while(it>0)
            {
                lastIndex = (lastIndex - 1 + orderCount) % orderCount;
                output = _ordereredConnections[lastIndex];
                if(output == node)
                {
                    return null;
                }

                if(output.connections != null && output.connections.Count > 1)
                {
                    return output;
                }

                it--;
            }
            return null;
        }

        public Node Intersect(Vector2Fixed point, Node connection)
        {
            Disconnect(connection);
            Node output = new(point);
            Connect(output);
            connection.Connect(output);
            return output;
        }

        public bool FullyPlotted()
        {
            return _connectionCountSqaure == _plotted.value;
        }

        public void Plot(int connectionIndex)
        {
            _plotted[connectionIndex] = true;
        }

        public void Plot(Node connection)
        {
            Plot(_connections.IndexOf(connection));
        }

        public bool IsPlotted(int connectionIndex)
        {
            return _plotted[connectionIndex];
        }

        public Node NextUnplotted()
        {
            if (FullyPlotted())
            {
                return null;
            }

            int firstFalse = _plotted.FirstFalse();
            return _connections[firstFalse];
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

        public static bool operator ==(Vector2Fixed a, Node b)
        {
            bool bIsNull = ReferenceEquals(null, b);
            if(bIsNull)
            {
                return false;
            }

            return a == b.position;
        }

        public static bool operator !=(Vector2Fixed a, Node b)
        {
            bool bIsNull = ReferenceEquals(null, b);
            if(bIsNull)
            {
                return true;
            }

            return a != b.position;
        }

        public static bool operator ==(Node a, Vector2Fixed b)
        {
            bool aIsNull = ReferenceEquals(null, a);
            if(aIsNull)
            {
                return false;
            }

            return a.position == b;
        }

        public static bool operator !=(Node a, Vector2Fixed b)
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
}