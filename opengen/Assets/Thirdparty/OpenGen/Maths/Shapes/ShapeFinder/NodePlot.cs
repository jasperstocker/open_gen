using System.Collections.Generic;
using opengen;
using opengen.types;
using UnityEngine;



namespace opengen.maths.shapes.shapefinder
{
    public class NodePlot
    {
        public List<Node> nodes = new();
        public Vector2 center;

        public NodePlot()
        {
        }

        public NodePlot(List<Node> nodes)
        {
            this.nodes.AddRange(nodes);
            CalculateCenter();
        }

        public Node this[int index] {get {return nodes[index];}}

        public void CalculateCenter()
        {
            center = Vector2.zero;
            int plotPointSize = nodes.Count;
            if (plotPointSize == 0)
            {
                return;
            }

            AABBox bounds = new();
            for (int p = 0; p < plotPointSize; p++)
            {
                bounds.Encapsulate(nodes[p].position.vector2);
            }

            center = bounds.center;
        }

        public Vector2[] GetShape()
        {
            Vector2[] output = new Vector2[nodes.Count];
            for(int n = 0; n < nodes.Count; n++)
            {
                output[n] = nodes[n].position.vector2;
            }

            return output;
        }

        public Vector2Fixed[] GetShapeFixed()
        {
            Vector2Fixed[] output = new Vector2Fixed[nodes.Count];
            for(int n = 0; n < nodes.Count; n++)
            {
                output[n] = nodes[n].position;
            }

            return output;
        }

        // public void DrawDebugOutline(Color col)
        // {
        //     int plotSize = nodes.Count;
        //     for (int n = 0; n < plotSize; n++)
        //     {
        //         Node nodeA = nodes[n];
        //         Node nodeB = nodes[(n + 1) % plotSize];
        //         Debug.DrawLine(nodeA.position.vector3XZ, nodeB.position.vector3XZ, col, 30);
        //     }
        // }
        //
        // public void DrawDebugStar(Color col)
        // {
        //     int plotSize = nodes.Count;
        //     Vector3 v3Center = MathUtils.ToV3(center);
        //     for (int n = 0; n < plotSize; n++)
        //     {
        //         Node nodeA = nodes[n];
        //         Debug.DrawLine(v3Center, nodeA.position.vector3XZ, col);
        //     }
        // }
    }
}
