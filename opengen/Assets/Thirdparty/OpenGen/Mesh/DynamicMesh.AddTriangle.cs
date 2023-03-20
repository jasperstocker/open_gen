using System.Collections.Generic;
using opengen.types;
using UnityEngine;


namespace opengen.mesh
{
    public partial class DynamicMesh
    {
        public void AddTri(Vector3 p0, Vector3 p1, Vector3 p2, int subMesh)
        {
            AddTri(p0, p1, p2, Vector2.right, subMesh);
        }
        
        public void AddTri(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 right, int subMesh)
        {
            if (MeshOverflow(3))
            {
                _overflow.AddTri(p0, p1, p2, right, subMesh);
                return;
            }

            int indiceBase = vertices.Count;
            vertices.Add(p0);
            vertices.Add(p1);
            vertices.Add(p2);

            Vector3 normal = Triangle3D.CalculateNormal(p0, p1, p2);
            Vector3 up = Vector3.Cross(right, normal);
            Vector4 tangent = Vector4Extensions.CalculateTangent(right);

            uvs.Add(Vector2.zero);
            uvs.Add(new Vector2(Vector3.Dot(p1 - p0, right), Vector3.Dot(p1 - p0, up)));
            uvs.Add(new Vector2(Vector3.Dot(p2 - p0, right), Vector3.Dot(p2 - p0, up)));

            if (ignoreSubmeshAssignment)
            {
                subMesh = 0;
            }

            if (!subTriangles.ContainsKey(subMesh))
            {
                subTriangles.Add(subMesh, new List<int>());
            }

            subTriangles[subMesh].Add(indiceBase);
            subTriangles[subMesh].Add(indiceBase + 1);
            subTriangles[subMesh].Add(indiceBase + 2);

            normals.AddRange(new[] {normal, normal, normal});
            tangents.AddRange(new[] {tangent, tangent, tangent});
            colours.AddRange(new Color32[3]);
        }

        public void AddTri(Vector3 p0, Vector3 p1, Vector3 p2, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector3 normal,
            Vector4 tangent, int subMesh, TextureData textureData = null)
        {
            if (MeshOverflow(3))
            {
                _overflow.AddTri(p0, p1, p2, uv0, uv1, uv2, normal, tangent, subMesh, textureData);
                return;
            }

            int indiceBase = vertices.Count;
            vertices.Add(p0);
            vertices.Add(p1);
            vertices.Add(p2);

            uvs.Add(CalculateUv(uv0, textureData));
            uvs.Add(CalculateUv(uv1, textureData));
            uvs.Add(CalculateUv(uv2, textureData));

            if (ignoreSubmeshAssignment)
            {
                subMesh = 0;
            }

            if (!subTriangles.ContainsKey(subMesh))
            {
                subTriangles.Add(subMesh, new List<int>());
            }

            subTriangles[subMesh].Add(indiceBase);
            subTriangles[subMesh].Add(indiceBase + 1);
            subTriangles[subMesh].Add(indiceBase + 2);

            normals.AddRange(new[] {normal, normal, normal});
            tangents.AddRange(new[] {tangent, tangent, tangent});
            colours.AddRange(new Color32[3]);
        }
    }
}