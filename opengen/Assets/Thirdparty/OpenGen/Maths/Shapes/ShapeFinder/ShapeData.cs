using System.Collections.Generic;
using opengen;
using opengen.types;
using UnityEngine;


namespace opengen.maths.shapes.shapefinder
{
    public class ShapeData
    {
        public List<Node> nodes = new();
        public List<NodePlot> plots = new();
        public List<NodePlot> outPlots = new();

        public ShapeData(INode[] input)
        {
            int count = input.Length;
            Dictionary<Vector2Fixed, Node> nodeDic = new();
            for (int i = 0; i < count; i++)
            {
                INode inputNode = input[i];
                if (nodeDic.ContainsKey(inputNode.position))
                {
                    continue;
                }

                Node node = new(inputNode.position);
                nodes.Add(node);
                nodeDic.Add(inputNode.position, node);
            }

            //Debug.Log("ShapeData " + nodes.Count);

            for (int i = 0; i < count; i++)
            {
                INode inputNode = input[i];
                Node node = nodeDic[inputNode.position];

                int connectionCount = inputNode.connections.Length;
                for (int c = 0; c < connectionCount; c++)
                {
                    INode conection = inputNode.connections[c];
                    Node connectionNode = nodeDic[conection.position];
                    node.Connect(connectionNode);
                }
            }
        }

        public ShapeData(Node[] input)
        {
            nodes = new List<Node>(input);
        }

        public ShapeData(List<Node> input)
        {
            nodes = new List<Node>(input);
        }

        public void Clear()
        {
            nodes.Clear();
            plots.Clear();
            outPlots.Clear();
        }

        // public void DrawDebug()
        // {
        //     int nodeCount = nodes.Count;
        //     for(int n = 0; n < nodeCount; n++)
        //     {
        //         Node node = nodes[n];
        //         int connectionCount = node.connections.Count;
        //         for(int c = 0; c < connectionCount; c++)
        //         {
        //             Node connection = node.connections[c];
        //             Debug.DrawLine(node.position.vector3XZ, connection.position.vector3XZ, Color.red, 2);
        //         }
        //     }
        // }
    }

    public interface INode
    {
        Vector2Fixed position {get;}
        INode[] connections {get;}
    }
}