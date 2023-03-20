using System.Collections.Generic;
using UnityEngine;

namespace opengen.mesh
{
    public class RawMeshData
    {
        public Vector3[] vertices;
        public Vector2[] uvs;
        public int[] triangles;
        public Dictionary<int, List<int>> subTriangles;
        public Vector3[] normals;
        public Vector4[] tangents;
        public Color32[] colours;
        public int vertCount;
        public int submeshCount;

        public RawMeshData()
        {

        }

        public RawMeshData(int vertCount, int triCount)
        {
            vertices = new Vector3[vertCount];
            uvs = new Vector2[vertCount];
            triangles = new int[triCount];
            subTriangles = new Dictionary<int, List<int>>();
            normals = new Vector3[vertCount];
            tangents = new Vector4[vertCount];
            colours = new Color32[vertCount];
            submeshCount = 1;
            this.vertCount = vertCount;
        }

        public void Copy(DynamicMesh data)
        {
            vertices = data.vertices.ToArray();
            uvs = data.uvs.ToArray();
            triangles = data.triangles.ToArray();
            normals = data.normals.ToArray();
            tangents = data.tangents.ToArray();
            vertCount = data.vertexCount;
            colours = new Color32[vertCount];
            subTriangles = new Dictionary<int, List<int>>(data.subTriangles);
            submeshCount = subTriangles.Count;
        }

        // public void Copy(Mesh data)
        // {
        //     vertices = data.vertices;
        //     uvs = data.uv;
        //     triangles = data.triangles;
        //     normals = data.normals;
        //     tangents = data.tangents;
        //     colours = data.colors32;
        //
        //     subTriangles = new Dictionary<int, List<int>>();
        //     for (int s = 0; s < data.subMeshCount; s++)
        //         subTriangles.Add(s, new List<int>(data.GetTriangles(s)));
        //
        //     vertCount = data.vertexCount;
        //     submeshCount = subTriangles.Count;
        // }

        public static RawMeshData CopyDynamicMesh(DynamicMesh data)
        {
            RawMeshData output = new(data.vertices.Count, data.triangles.Count);
            output.Copy(data);
            return output;
        }

        // public static RawMeshData CopMesh(Mesh data)
        // {
        //     RawMeshData output = new RawMeshData(data.vertices.Length, data.triangles.Length);
        //     output.Copy(data);
        //     return output;
        // }

        public override string ToString()
        {
            return string.Format("vert count {0} tri count {1} sm count {2} ", vertices.Length, triangles.Length, subTriangles.Count);
        }
    }
}