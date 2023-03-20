using System;
using System.Collections.Generic;
using opengen.types;
using UnityEngine;


namespace opengen.mesh
{
    public partial class DynamicMesh
    {
        public void AddPlaneBasic(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int subMesh)
        {
            if (MeshOverflow(4))
            {
                _overflow.AddPlaneBasic(p0, p1, p2, p3, subMesh);
                return;
            }

            int indiceBase = vertices.Count;
            vertices.Add(p0);
            vertices.Add(p1);
            vertices.Add(p2);
            vertices.Add(p3);

            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);

            normals.Add(Vector3.zero);
            normals.Add(Vector3.zero);
            normals.Add(Vector3.zero);
            normals.Add(Vector3.zero);

            tangents.Add(Vector4.zero);
            tangents.Add(Vector4.zero);
            tangents.Add(Vector4.zero);
            tangents.Add(Vector4.zero);

            if (ignoreSubmeshAssignment)
            {
                subMesh = 0;
            }

            if (!subTriangles.ContainsKey(subMesh))
            {
                subTriangles.Add(subMesh, new List<int>());
            }

            subTriangles[subMesh].Add(indiceBase);
            subTriangles[subMesh].Add(indiceBase + 2);
            subTriangles[subMesh].Add(indiceBase + 1);

            subTriangles[subMesh].Add(indiceBase + 1);
            subTriangles[subMesh].Add(indiceBase + 2);
            subTriangles[subMesh].Add(indiceBase + 3);
        }

        public void AddPlane(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int subMesh)
        {
            if (MeshOverflow(4))
            {
                _overflow.AddPlane(p0, p1, p2, p3, subMesh);
                return;
            }

            AddPlane(p0, p1, p2, p3, Vector2.zero, Vector2.one, Triangle3D.CalculateNormal(p0, p2, p1),
                Vector4Extensions.CalculateTangent((p1 - p0).normalized), subMesh, null);
        }

        public void AddPlane(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal, Vector4 tangent,
            int subMesh)
        {
            if (MeshOverflow(4))
            {
                _overflow.AddPlane(p0, p1, p2, p3, normal, tangent, subMesh);
                return;
            }

            AddPlane(p0, p1, p2, p3, Vector2.zero, Vector2.one, normal, tangent, subMesh, null);
        }

        public void AddPlane(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector2 minUv, Vector2 maxUv,
            Vector3 normal, Vector4 tangent, int subMesh, TextureData textureData = null)
        {
            if (MeshOverflow(4))
            {
                _overflow.AddPlane(p0, p1, p2, p3, minUv, maxUv, normal, tangent, subMesh, textureData);
                return;
            }

            int indiceBase = vertices.Count;
            vertices.Add(p0);
            vertices.Add(p1);
            vertices.Add(p2);
            vertices.Add(p3);

            if (textureData != null)
            {
                minUv = textureData.CalculateUv(minUv);
                maxUv = textureData.CalculateUv(maxUv);
            }

            uvs.Add(new Vector2(minUv.x, minUv.y));
            uvs.Add(new Vector2(maxUv.x, minUv.y));
            uvs.Add(new Vector2(minUv.x, maxUv.y));
            uvs.Add(new Vector2(maxUv.x, maxUv.y));

            if (ignoreSubmeshAssignment)
            {
                subMesh = 0;
            }

            if (!subTriangles.ContainsKey(subMesh))
            {
                subTriangles.Add(subMesh, new List<int>());
            }

            subTriangles[subMesh].Add(indiceBase);
            subTriangles[subMesh].Add(indiceBase + 2);
            subTriangles[subMesh].Add(indiceBase + 1);

            subTriangles[subMesh].Add(indiceBase + 1);
            subTriangles[subMesh].Add(indiceBase + 2);
            subTriangles[subMesh].Add(indiceBase + 3);

            normals.AddRange(new[] {normal, normal, normal, normal});
            tangents.AddRange(new[] {tangent, tangent, tangent, tangent});
            colours.AddRange(new Color32[4]);
        }

        public void AddPlane(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector2 minUv, Vector2 maxUv,
            Vector3 normal, Vector4 tangent, Color32 colour, int subMesh, TextureData textureData = null)
        {
            if (MeshOverflow(4))
            {
                _overflow.AddPlane(p0, p1, p2, p3, minUv, maxUv, normal, tangent, colour, subMesh, textureData);
                return;
            }

            int indiceBase = vertices.Count;
            vertices.Add(p0);
            vertices.Add(p1);
            vertices.Add(p2);
            vertices.Add(p3);

            if (textureData != null)
            {
                minUv = textureData.CalculateUv(minUv);
                maxUv = textureData.CalculateUv(maxUv);
            }

            uvs.Add(new Vector2(minUv.x, minUv.y));
            uvs.Add(new Vector2(maxUv.x, minUv.y));
            uvs.Add(new Vector2(minUv.x, maxUv.y));
            uvs.Add(new Vector2(maxUv.x, maxUv.y));

            if (ignoreSubmeshAssignment)
            {
                subMesh = 0;
            }

            if (!subTriangles.ContainsKey(subMesh))
            {
                subTriangles.Add(subMesh, new List<int>());
            }

            subTriangles[subMesh].Add(indiceBase);
            subTriangles[subMesh].Add(indiceBase + 2);
            subTriangles[subMesh].Add(indiceBase + 1);

            subTriangles[subMesh].Add(indiceBase + 1);
            subTriangles[subMesh].Add(indiceBase + 2);
            subTriangles[subMesh].Add(indiceBase + 3);

            normals.AddRange(new[] {normal, normal, normal, normal});
            tangents.AddRange(new[] {tangent, tangent, tangent, tangent});
            colours.AddRange(new[] {colour, colour, colour, colour});
        }

        public void AddPlaneNoUvCalc(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector2 minUv, Vector2 maxUv,
            Vector3 normal, Vector4 tangent, int subMesh, TextureData textureData)
        {
            if (MeshOverflow(4))
            {
                _overflow.AddPlaneNoUvCalc(p0, p1, p2, p3, minUv, maxUv, normal, tangent, subMesh, textureData);
                return;
            }

            int indiceBase = vertices.Count;
            vertices.Add(p0);
            vertices.Add(p1);
            vertices.Add(p2);
            vertices.Add(p3);

            uvs.Add(new Vector2(minUv.x, minUv.y));
            uvs.Add(new Vector2(maxUv.x, minUv.y));
            uvs.Add(new Vector2(minUv.x, maxUv.y));
            uvs.Add(new Vector2(maxUv.x, maxUv.y));

            if (ignoreSubmeshAssignment)
            {
                subMesh = 0;
            }

            if (!subTriangles.ContainsKey(subMesh))
            {
                subTriangles.Add(subMesh, new List<int>());
            }

            subTriangles[subMesh].Add(indiceBase);
            subTriangles[subMesh].Add(indiceBase + 2);
            subTriangles[subMesh].Add(indiceBase + 1);

            subTriangles[subMesh].Add(indiceBase + 1);
            subTriangles[subMesh].Add(indiceBase + 2);
            subTriangles[subMesh].Add(indiceBase + 3);

            normals.AddRange(new[] {normal, normal, normal, normal});
            tangents.AddRange(new[] {tangent, tangent, tangent, tangent});
            colours.AddRange(new Color32[4]);
        }

        public void AddPlaneComplex(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal, Vector4 tangent,
            int subMesh, TextureData textureData)
        {
            if (MeshOverflow(4))
            {
                _overflow.AddPlaneComplex(p0, p1, p2, p3, normal, tangent, subMesh, textureData);
                return;
            }

            Vector2 uv0 = new(p0.x, p0.z);
            Vector2 uv1 = new(p1.x, p1.z);
            Vector2 uv2 = new(p2.x, p2.z);
            Vector2 uv3 = new(p3.x, p3.z);
            AddPlaneComplex(p0, p1, p2, p3, uv0, uv1, uv2, uv3, normal, tangent, subMesh, textureData);
        }

        public void AddPlaneComplexUp(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float uvAngle, Vector3 normal,
            Vector4 tangent, int subMesh, TextureData textureData)
        {
            if (MeshOverflow(4))
            {
                _overflow.AddPlaneComplexUp(p0, p1, p2, p3, uvAngle, normal, tangent, subMesh, textureData);
                return;
            }

            Vector2 uv0 = Rotate(new Vector2(p0.x, p0.z), uvAngle);
            Vector2 uv1 = Rotate(new Vector2(p1.x, p1.z), uvAngle);
            Vector2 uv2 = Rotate(new Vector2(p2.x, p2.z), uvAngle);
            Vector2 uv3 = Rotate(new Vector2(p3.x, p3.z), uvAngle);
            AddPlaneComplex(p0, p1, p2, p3, uv0, uv1, uv2, uv3, normal, tangent, subMesh, textureData);
        }

        public void AddPlaneComplex(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector2 uv0, Vector2 uv1,
            Vector2 uv2, Vector2 uv3, Vector3 normal, Vector4 tangent, int subMesh, TextureData textureData)
        {
            if (MeshOverflow(4))
            {
                _overflow.AddPlaneComplex(p0, p1, p2, p3, uv0, uv1, uv2, uv3, normal, tangent, subMesh, textureData);
                return;
            }

            int indiceBase = vertices.Count;
            vertices.Add(p0);
            vertices.Add(p1);
            vertices.Add(p2);
            vertices.Add(p3);

            uvs.Add(CalculateUv(uv0, textureData));
            uvs.Add(CalculateUv(uv1, textureData));
            uvs.Add(CalculateUv(uv2, textureData));
            uvs.Add(CalculateUv(uv3, textureData));

            if (ignoreSubmeshAssignment)
            {
                subMesh = 0;
            }

            if (!subTriangles.ContainsKey(subMesh))
            {
                subTriangles.Add(subMesh, new List<int>());
            }

            subTriangles[subMesh].Add(indiceBase);
            subTriangles[subMesh].Add(indiceBase + 2);
            subTriangles[subMesh].Add(indiceBase + 1);

            subTriangles[subMesh].Add(indiceBase + 1);
            subTriangles[subMesh].Add(indiceBase + 2);
            subTriangles[subMesh].Add(indiceBase + 3);

            normals.AddRange(new[] {normal, normal, normal, normal});
            tangents.AddRange(new[] {tangent, tangent, tangent, tangent});
            colours.AddRange(new Color32[4]);
        }

        public void AddPlaneComplex(Vector3[] v, Vector2[] uv, Vector3 normal, Vector4 tangent, int subMesh,
            TextureData textureData)
        {
            if (v.Length > 4)
            {
                throw new Exception("Expecting complex plane data - 4 verts");
            }

            if (MeshOverflow(4))
            {
                _overflow.AddPlaneComplex(v, uv, normal, tangent, subMesh, textureData);
                return;
            }

            int indiceBase = vertices.Count;
            vertices.Add(v[0]);
            vertices.Add(v[1]);
            vertices.Add(v[2]);
            vertices.Add(v[3]);

            uvs.Add(CalculateUv(uv[0], textureData));
            uvs.Add(CalculateUv(uv[1], textureData));
            uvs.Add(CalculateUv(uv[2], textureData));
            uvs.Add(CalculateUv(uv[3], textureData));

            if (ignoreSubmeshAssignment)
            {
                subMesh = 0;
            }

            if (!subTriangles.ContainsKey(subMesh))
            {
                subTriangles.Add(subMesh, new List<int>());
            }

            subTriangles[subMesh].Add(indiceBase);
            subTriangles[subMesh].Add(indiceBase + 2);
            subTriangles[subMesh].Add(indiceBase + 1);

            subTriangles[subMesh].Add(indiceBase + 1);
            subTriangles[subMesh].Add(indiceBase + 2);
            subTriangles[subMesh].Add(indiceBase + 3);

            normals.AddRange(new[] {normal, normal, normal, normal});
            tangents.AddRange(new[] {tangent, tangent, tangent, tangent});
            colours.AddRange(new Color32[4]);
        }

        public void AddPlaneComplex(Vector3[] v, Vector2[] uv, Vector3[] normals, Vector4[] tangents, int subMesh,
            TextureData textureData)
        {
            if (v.Length > 4)
            {
                throw new Exception("Expecting complex plane data - 4 verts");
            }

            if (MeshOverflow(4))
            {
                _overflow.AddPlaneComplex(v, uv, normals, tangents, subMesh, textureData);
                return;
            }

            int indiceBase = vertices.Count;
            vertices.Add(v[0]);
            vertices.Add(v[1]);
            vertices.Add(v[2]);
            vertices.Add(v[3]);

            uvs.Add(CalculateUv(uv[0], textureData));
            uvs.Add(CalculateUv(uv[1], textureData));
            uvs.Add(CalculateUv(uv[2], textureData));
            uvs.Add(CalculateUv(uv[3], textureData));

            if (ignoreSubmeshAssignment)
            {
                subMesh = 0;
            }

            if (!subTriangles.ContainsKey(subMesh))
            {
                subTriangles.Add(subMesh, new List<int>());
            }

            subTriangles[subMesh].Add(indiceBase);
            subTriangles[subMesh].Add(indiceBase + 2);
            subTriangles[subMesh].Add(indiceBase + 1);

            subTriangles[subMesh].Add(indiceBase + 1);
            subTriangles[subMesh].Add(indiceBase + 2);
            subTriangles[subMesh].Add(indiceBase + 3);

            this.normals.AddRange(normals);
            this.tangents.AddRange(tangents);
            colours.AddRange(new Color32[4]);
        }
    }
}