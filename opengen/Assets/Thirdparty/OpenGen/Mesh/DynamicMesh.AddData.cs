using System.Collections.Generic;
using opengen.types;
using UnityEngine;


namespace opengen.mesh
{
    public partial class DynamicMesh
    {
        public void AddDataRaw(Vector3[] verts, Vector2[] uvs, Vector3[] norms, Vector4[] tan,
            Dictionary<int, List<int>> subTriangles)
        {
            int vertLength = verts.Length;
            if (MeshOverflow(vertLength))
            {
                _overflow.AddDataRaw(verts, uvs, norms, tan, subTriangles);
                return;
            }

            vertices.AddRange(verts);
            this.uvs.AddRange(uvs);
            normals.AddRange(norms);
            tangents.AddRange(tan);
            colours.AddRange(new Color32[vertLength]);
            foreach (KeyValuePair<int, List<int>> kv in subTriangles)
            {
                int submesh = kv.Key;
                if (ignoreSubmeshAssignment)
                {
                    submesh = 0;
                }

                if (!this.subTriangles.ContainsKey(submesh))
                {
                    this.subTriangles.Add(submesh, new List<int>());
                }

                this.subTriangles[submesh].AddRange(kv.Value);
            }
        }

        public void AddDataRaw(List<Vector3> verts, List<Vector2> uvs, List<Vector3> norms, List<Vector4> tan,
            Dictionary<int, List<int>> subTriangles)
        {
            int vertLength = verts.Count;
            if (MeshOverflow(vertLength))
            {
                _overflow.AddDataRaw(verts, uvs, norms, tan, subTriangles);
                return;
            }

            vertices.AddRange(verts);
            this.uvs.AddRange(uvs);
            normals.AddRange(norms);
            tangents.AddRange(tan);
            colours.AddRange(new Color32[vertLength]);
            foreach (KeyValuePair<int, List<int>> kv in subTriangles)
            {
                int submesh = kv.Key;
                if (ignoreSubmeshAssignment)
                {
                    submesh = 0;
                }

                if (!this.subTriangles.ContainsKey(submesh))
                {
                    this.subTriangles.Add(submesh, new List<int>());
                }

                this.subTriangles[submesh].AddRange(kv.Value);
            }
        }
        
        
		// public void AddData(Mesh mesh, int[] mappedSubmeshes, Vector3 translate, Quaternion rotate, Vector3 scale, Vector2 uvOffset, bool[] uvTransform)
  //       {
  //           if (MeshOverflow(mesh.vertexCount))
  //           {
  //               _overflow.AddData(mesh, mappedSubmeshes, translate, rotate, scale, uvOffset, uvTransform);
  //               return;
  //           }
  //
  //           int indiceBase = vertices.Count;
  //           Vector3[] meshvertices = mesh.vertices;
  //           Vector3[] meshnormals = mesh.normals;
  //           Vector4[] meshtangents = mesh.tangents;
  //           int vertCount = meshvertices.Length;
  //           Vector3 rotateTangent = new Vector3();
  //           for (int v = 0; v < vertCount; v++)
  //           {
  //               vertices.Add(rotate * (meshvertices[v] * scale) + translate);
  //               normals.Add(rotate * meshnormals[v]);
  //
  //               //rotate tangents here
  //               rotateTangent.x = meshtangents[v].x;
  //               rotateTangent.y = meshtangents[v].y;
  //               rotateTangent.z = meshtangents[v].z;
  //               rotateTangent = rotate * rotateTangent;
  //               meshtangents[v].x = rotateTangent.x;
  //               meshtangents[v].y = rotateTangent.y;
  //               meshtangents[v].z = rotateTangent.z;
  //               tangents.Add(meshtangents[v]);
  //           }
  //
  //           int submeshCount = mappedSubmeshes.Length;
  //           int meshSubmesh = 0;
  //           if (submeshCount == 0)
  //           {
  //               mappedSubmeshes = new[] { 0 };
  //               submeshCount = 1;
  //           }
  //
  //           Vector2[] meshUvs = mesh.uv;
  //           bool[] set = new bool[vertCount];
  //           for (int sm = 0; sm < submeshCount; sm++)
  //           {
  //               int submesh = mappedSubmeshes[sm];
  //               if (ignoreSubmeshAssignment) submesh = 0;
  //               if (submesh == -1)
  //                   continue;
  //
  //               if (mesh.subMeshCount <= meshSubmesh)
  //                   continue;
  //
  //               if (!subTriangles.ContainsKey(submesh))
  //                   subTriangles.Add(submesh, new List<int>());
  //
  //               int[] tris = mesh.GetTriangles(meshSubmesh);
  //               int newTriCount = tris.Length;
  //
  //               if (uvTransform[sm])
  //               {
  //                   for (int t = 0; t < newTriCount; t++)
  //                   {
  //                       int uvIndex = tris[t];
  //                       if (set[uvIndex]) continue;
  //
  //                       meshUvs[uvIndex] += uvOffset;
  //                       set[uvIndex] = true;
  //                   }
  //               }
  //
  //               for (int t = 0; t < newTriCount; t++)
  //                   tris[t] += indiceBase;
  //               subTriangles[submesh].AddRange(tris);
  //               meshSubmesh++;
  //           }
  //           uvs.AddRange(meshUvs);
  //           colours.AddRange(new Color32[vertCount]);
  //       }

        public void AddData(RawMeshData mesh, Vector3 translate, Quaternion rotate, Vector3 scale, Vector2 uvOffset)
        {
            int vertCount = mesh.vertCount;
            if (MeshOverflow(vertCount))
            {
                _overflow.AddData(mesh, translate, rotate, scale, uvOffset);
                return;
            }

            int indiceBase = vertices.Count;
//            Debug.Log(mesh.vertices.Length);
            Vector3[] meshvertices = mesh.vertices;
            Vector3[] meshnormals = mesh.normals;
            Vector4[] meshtangents = mesh.tangents;
            Vector3 rotateTangent = new();
            for (int v = 0; v < vertCount; v++)
            {
                Vector3 vert = meshvertices[v];
                Vector3 scaledVert = Vector3.Scale(vert, scale);
                vertices.Add(rotate * scaledVert + translate);
                normals.Add(rotate * meshnormals[v]);

                //rotate tangents here
                rotateTangent.x = meshtangents[v].x;
                rotateTangent.y = meshtangents[v].y;
                rotateTangent.z = meshtangents[v].z;
                rotateTangent = rotate * rotateTangent;
                meshtangents[v].x = rotateTangent.x;
                meshtangents[v].y = rotateTangent.y;
                meshtangents[v].z = rotateTangent.z;
                tangents.Add(meshtangents[v]);
            }

            Vector2[] meshuvs = new Vector2[vertCount];
            System.Array.Copy(mesh.uvs, meshuvs, vertCount);
            int submeshIt = 0;
            foreach(KeyValuePair<int, List<int>> var in mesh.subTriangles)
            {
                int submesh = var.Key;
                if (ignoreSubmeshAssignment)
                {
                    submesh = 0;
                }

                if (submesh == -1)
                {
                    continue;
                }

                if (!subTriangles.ContainsKey(submesh))
                {
                    subTriangles.Add(submesh, new List<int>(LIST_SIZE_TRI));
                }

                List<int> tris = var.Value;
//                List<int> tris = new List<int>(var.Value);
                int newTriCount = tris.Count;
                
                submeshIt++;

//                for (int t = 0; t < newTriCount; t++)
//                    tris[t] += indiceBase;
//                subTriangles[submesh].AddRange(tris);
                for(int t = 0; t < newTriCount; t++)
                {
                    subTriangles[submesh].Add(tris[t] + indiceBase);
                }
            }
            uvs.AddRange(meshuvs);
            colours.AddRange(new Color32[vertCount]);
        }

        public void AddData(Vector3[] verts, Vector2[] uvs, int[] tris, Vector3[] norms, Vector4[] tan, int subMesh)
        {
            int vertLength = verts.Length;
            if (MeshOverflow(vertLength))
            {
                _overflow.AddData(verts, uvs, tris, norms, tan, subMesh);
                return;
            }

            int indiceBase = vertices.Count;
            vertices.AddRange(verts);
            this.uvs.AddRange(uvs);
            normals.AddRange(norms);
            tangents.AddRange(tan);
            colours.AddRange(new Color32[vertLength]);
            if (ignoreSubmeshAssignment)
            {
                subMesh = 0;
            }

            if (!subTriangles.ContainsKey(subMesh))
            {
                subTriangles.Add(subMesh, new List<int>());
            }

            int newTriCount = tris.Length;
            for (int t = 0; t < newTriCount; t++)
            {
                int newTri = (indiceBase + tris[t]);
                subTriangles[subMesh].Add(newTri);
            }
        }

        // public void AddData(Mesh mesh, int[] submeshes, Vector3 translate, Quaternion rotate, Vector3 scale)
        // {
        //     if (MeshOverflow(mesh.vertexCount))
        //     {
        //         _overflow.AddData(mesh, submeshes, translate, rotate, scale);
        //         return;
        //     }
        //
        //     int indiceBase = vertices.Count;
        //     Vector3[] meshvertices = mesh.vertices;
        //     Vector3[] meshNormals = mesh.normals;
        //     Vector4[] meshTangents = mesh.tangents;
        //     int vertCount = meshvertices.Length;
        //
        //     Vector3 rotateTangent = new Vector3();
        //     for (int v = 0; v < vertCount; v++)
        //     {
        //         vertices.Add(rotate * (meshvertices[v] * scale) + translate);
        //         normals.Add(rotate * meshNormals[v]);
        //
        //         rotateTangent.x = meshTangents[v].x;
        //         rotateTangent.y = meshTangents[v].y;
        //         rotateTangent.z = meshTangents[v].z;
        //         rotateTangent = rotate * rotateTangent;
        //         meshTangents[v].x = rotateTangent.x;
        //         meshTangents[v].y = rotateTangent.y;
        //         meshTangents[v].z = rotateTangent.z;
        //         tangents.Add(meshTangents[v]);
        //     }
        //
        //     uvs.AddRange(mesh.uv);
        //     colours.AddRange(new Color32[vertCount]);
        //
        //     int submeshCount = submeshes.Length;
        //     if (submeshCount == 0)
        //     {
        //         int submesh = 0;
        //         for (int s = 0;;)
        //         {
        //             int[] submeshTris = mesh.GetTriangles(submesh);
        //             int triCount = submeshTris.Length;
        //             if (triCount > 0)
        //             {
        //                 if (!subTriangles.ContainsKey(submesh))
        //                     subTriangles.Add(submesh, new List<int>());
        //
        //                 for (int t = 0; t < triCount; t++)
        //                 {
        //                     int newTri = indiceBase + submeshTris[t];
        //                     subTriangles[submesh].Add(newTri);
        //                 }
        //
        //                 s++;
        //
        //                 if (s == submeshCount)
        //                     break;
        //             }
        //
        //             submesh++;
        //
        //             if (submesh > 100)
        //                 break;
        //         }
        //     }
        //     else
        //     {
        //         for (int sm = 0; sm < submeshCount; sm++)
        //         {
        //             if (mesh.subMeshCount <= sm) continue;
        //             int submesh = submeshes[sm];
        //             if (ignoreSubmeshAssignment) submesh = 0;
        //             if (submesh == -1)
        //                 continue;
        //             if (!subTriangles.ContainsKey(submesh))
        //                 subTriangles.Add(submesh, new List<int>());
        //             int[] smTris = mesh.GetTriangles(sm);
        //             int smTriCount = smTris.Length;
        //             for (int t = 0; t < smTriCount; t++)
        //             {
        //                 int newTri = indiceBase + smTris[t];
        //                 subTriangles[submesh].Add(newTri);
        //             }
        //         }
        //     }
        // }

        public void AddData(RawMeshData mesh, int[] mappedSubmeshes, Vector3 translate, Quaternion rotate,
            Vector3 scale)
        {
            int meshvertexCount = mesh.vertices.Length;
            if (MeshOverflow(meshvertexCount))
            {
                _overflow.AddData(mesh, mappedSubmeshes, translate, rotate, scale);
                return;
            }

            int indiceBase = vertices.Count;
            Vector3[] meshvertices = mesh.vertices;
            Vector3[] meshNormals = mesh.normals;
            Vector4[] meshTangents = mesh.tangents;
            int vertCount = meshvertices.Length;
            colours.AddRange(new Color32[vertCount]);

            Vector3 rotateTangent = new();
            for (int v = 0; v < vertCount; v++)
            {
                Vector3 vert = meshvertices[v];
                Vector3 scaledVert = Vector3.Scale(vert, scale);
                vertices.Add(rotate * scaledVert + translate);
                normals.Add(rotate * meshNormals[v]);

                rotateTangent.x = meshTangents[v].x;
                rotateTangent.y = meshTangents[v].y;
                rotateTangent.z = meshTangents[v].z;
                rotateTangent = rotate * rotateTangent;
                meshTangents[v].x = rotateTangent.x;
                meshTangents[v].y = rotateTangent.y;
                meshTangents[v].z = rotateTangent.z;
                tangents.Add(meshTangents[v]);
            }

            uvs.AddRange(mesh.uvs);
            int submeshCount = mappedSubmeshes.Length;
            int meshsubMeshCount = mesh.subTriangles.Count;
            if (submeshCount == 0)
            {
                submeshCount = meshsubMeshCount;
                mappedSubmeshes = new int[submeshCount];
                for (int s = 0; s < meshsubMeshCount; s++)
                {
                    mappedSubmeshes[s] = 0;
                }
            }

            int meshSubmesh = 0;
            for (int sm = 0; sm < submeshCount; sm++)
            {
                int submesh = mappedSubmeshes[sm];
                if (ignoreSubmeshAssignment)
                {
                    submesh = 0;
                }

                if (submesh == -1)
                {
                    continue;
                }

                if (!mesh.subTriangles.ContainsKey(meshSubmesh))
                {
                    meshSubmesh++;
                    continue;
                }

                if (!subTriangles.ContainsKey(submesh))
                {
                    subTriangles.Add(submesh, new List<int>());
                }

                int newTriCount = mesh.subTriangles[meshSubmesh].Count;
                for (int t = 0; t < newTriCount; t++)
                {
                    int newTri = (indiceBase + mesh.subTriangles[meshSubmesh][t]);
                    subTriangles[submesh].Add(newTri);
                }

                meshSubmesh++;
            }
        }

        // public void AddData(Mesh mesh, Vector3 translate, Quaternion rotate, Vector3 scale)
        // {
        //     if (MeshOverflow(mesh.vertexCount))
        //     {
        //         _overflow.AddData(mesh, translate, rotate, scale);
        //         return;
        //     }
        //
        //     int indiceBase = vertices.Count;
        //     Vector3[] meshvertices = mesh.vertices;
        //     Vector3[] meshNormals = mesh.normals;
        //     Vector4[] meshTangents = mesh.tangents;
        //     int vertCount = meshvertices.Length;
        //
        //     Vector3 rotateTangent = new Vector3();
        //     for (int v = 0; v < vertCount; v++)
        //     {
        //         vertices.Add(rotate * (meshvertices[v] * scale) + translate);
        //         normals.Add(rotate * meshNormals[v]);
        //
        //         rotateTangent.x = meshTangents[v].x;
        //         rotateTangent.y = meshTangents[v].y;
        //         rotateTangent.z = meshTangents[v].z;
        //         rotateTangent = rotate * rotateTangent;
        //         meshTangents[v].x = rotateTangent.x;
        //         meshTangents[v].y = rotateTangent.y;
        //         meshTangents[v].z = rotateTangent.z;
        //         tangents.Add(meshTangents[v]);
        //     }
        //
        //     uvs.AddRange(mesh.uv);
        //     colours.AddRange(new Color32[vertCount]);
        //
        //     int[] tris = mesh.triangles;
        //     int newTriCount = tris.Length;
        //     if (!subTriangles.ContainsKey(0))
        //         subTriangles.Add(0, new List<int>());
        //     for (int t = 0; t < newTriCount; t++)
        //     {
        //         int newTri = (indiceBase + tris[t]);
        //         subTriangles[0].Add(newTri);
        //     }
        // }

        public void AddData(RawMeshData mesh, int submesh = 0)
        {
            int meshvertexCount = mesh.vertices.Length;
            if (MeshOverflow(meshvertexCount))
            {
                _overflow.AddData(mesh, submesh);
                return;
            }

            int indiceBase = vertices.Count;
            Vector3[] meshvertices = mesh.vertices;
            Vector3[] meshNormals = mesh.normals;
            Vector4[] meshTangents = mesh.tangents;
            Color32[] meshColours = mesh.colours;
            int vertCount = meshvertices.Length;

            // for (int v = 0; v < vertCount; v++)
            // {
            //    vertices.Add(meshvertices[v]);
            //     normals.Add(meshNormals[v]);
            //        tangents.Add(meshTangents[v]);
            //        tangents.Add(meshTangents[v]);
            // }
            vertices.AddRange(meshvertices);
            uvs.AddRange(mesh.uvs);
            normals.AddRange(meshNormals);
            tangents.AddRange(meshTangents);
            colours.AddRange(meshColours);

            int[] tris = mesh.triangles;
            int newTriCount = tris.Length;
            if (ignoreSubmeshAssignment)
            {
                submesh = 0;
            }

            if (!subTriangles.ContainsKey(submesh))
            {
                subTriangles.Add(submesh, new List<int>());
            }

            for (int t = 0; t < newTriCount; t++)
            {
                int newTri = (indiceBase + tris[t]);
                subTriangles[submesh].Add(newTri);
            }
        }

        public void AddData(RawMeshData mesh, Vector3 translate, Quaternion rotate, Vector3 scale)
        {
            int meshvertexCount = mesh.vertices.Length;
            if (MeshOverflow(meshvertexCount))
            {
                _overflow.AddData(mesh, translate, rotate, scale);
                return;
            }

            int indiceBase = vertices.Count;
            Vector3[] meshvertices = mesh.vertices;
            Vector3[] meshNormals = mesh.normals;
            Vector4[] meshTangents = mesh.tangents;
            int vertCount = meshvertices.Length;

            Vector3 rotateTangent = new();
            for (int v = 0; v < vertCount; v++)
            {
                Vector3 vert = meshvertices[v];
                Vector3 scaledVert = Vector3.Scale(vert, scale);
                vertices.Add(rotate * scaledVert + translate);
                normals.Add(rotate * meshNormals[v]);

                rotateTangent.x = meshTangents[v].x;
                rotateTangent.y = meshTangents[v].y;
                rotateTangent.z = meshTangents[v].z;
                rotateTangent = rotate * rotateTangent;
                meshTangents[v].x = rotateTangent.x;
                meshTangents[v].y = rotateTangent.y;
                meshTangents[v].z = rotateTangent.z;
                tangents.Add(meshTangents[v]);
            }

            uvs.AddRange(mesh.uvs);
            colours.AddRange(new Color32[vertCount]);

            int[] tris = mesh.triangles;
            int newTriCount = tris.Length;
            if (!subTriangles.ContainsKey(0))
            {
                subTriangles.Add(0, new List<int>());
            }

            for (int t = 0; t < newTriCount; t++)
            {
                int newTri = (indiceBase + tris[t]);
                subTriangles[0].Add(newTri);
            }
        }

        public void AddDataKeepSubmeshStructure(RawMeshData mesh, Vector3 translate, Quaternion rotate, Vector3 scale)
        {
            int meshvertexCount = mesh.vertices.Length;
            if (MeshOverflow(meshvertexCount))
            {
                _overflow.AddData(mesh, translate, rotate, scale);
                return;
            }

            int indiceBase = vertices.Count;
            Vector3[] meshvertices = mesh.vertices;
            Vector3[] meshNormals = mesh.normals;
            Vector4[] meshTangents = mesh.tangents;
            int vertCount = meshvertices.Length;

            Vector3 rotateTangent = new();
            for (int v = 0; v < vertCount; v++)
            {
                Vector3 vert = meshvertices[v];
                Vector3 scaledVert = Vector3.Scale(vert, scale);
                vertices.Add(rotate * scaledVert + translate);
                normals.Add(rotate * meshNormals[v]);

                rotateTangent.x = meshTangents[v].x;
                rotateTangent.y = meshTangents[v].y;
                rotateTangent.z = meshTangents[v].z;
                rotateTangent = rotate * rotateTangent;
                meshTangents[v].x = rotateTangent.x;
                meshTangents[v].y = rotateTangent.y;
                meshTangents[v].z = rotateTangent.z;
                tangents.Add(meshTangents[v]);
            }

            uvs.AddRange(mesh.uvs);
            colours.AddRange(new Color32[vertCount]);

            foreach (KeyValuePair<int, List<int>> var in mesh.subTriangles)
            {
                int submeshKey = var.Key;
                if (!subTriangles.ContainsKey(submeshKey))
                {
                    subTriangles.Add(submeshKey, new List<int>());
                }

                List<int> tris = var.Value;
                int triCount = var.Value.Count;
                for (int t = 0; t < triCount; t++)
                {
                    int newTri = (indiceBase + tris[t]);
                    subTriangles[submeshKey].Add(newTri);
                }
            }
        }

        // public void AddData(Mesh mesh, Matrix4 m4, int[] submeshes = null)
        // {
        //     if (MeshOverflow(mesh.vertexCount))
        //     {
        //         _overflow.AddData(mesh, m4, submeshes);
        //         return;
        //     }
        //
        //     int indiceBase = vertices.Count;
        //     Vector3[] meshvertices = mesh.vertices;
        //     Vector3[] meshNormals = mesh.normals;
        //     Vector4[] meshTangents = mesh.tangents;
        //     int vertCount = meshvertices.Length;
        //
        //     Vector3 rotateTangent = new Vector3();
        //     Vector3 m4Col1 = m4.GetColumn(1).Vector3;
        //     Vector3 m4Col2 = m4.GetColumn(2).Vector3;
        //     Quaternion rotate = Quaternion.LookRotation(m4Col2, m4Col1);
        //     for (int v = 0; v < vertCount; v++)
        //     {
        //         vertices.Add(m4.MultiplyPoint3x4(meshvertices[v]));
        //         normals.Add(rotate * meshNormals[v]);
        //
        //         rotateTangent.x = meshTangents[v].x;
        //         rotateTangent.y = meshTangents[v].y;
        //         rotateTangent.z = meshTangents[v].z;
        //         rotateTangent = rotate * rotateTangent;
        //         meshTangents[v].x = rotateTangent.x;
        //         meshTangents[v].y = rotateTangent.y;
        //         meshTangents[v].z = rotateTangent.z;
        //         tangents.Add(meshTangents[v]);
        //     }
        //
        //     if (mesh.uv.Length == vertCount)
        //         uvs.AddRange(mesh.uv);
        //     else
        //         uvs.AddRange(new Vector2[vertCount]);
        //
        //     if (submeshes == null)
        //     {
        //         int submeshCount = mesh.subMeshCount;
        //         int submesh = 0;
        //         for (int s = 0;;)
        //         {
        //             int[] submeshTris = mesh.GetTriangles(submesh);
        //             int triCount = submeshTris.Length;
        //             if (triCount > 0)
        //             {
        //                 if (!subTriangles.ContainsKey(submesh))
        //                     subTriangles.Add(submesh, new List<int>());
        //
        //                 for (int t = 0; t < triCount; t++)
        //                 {
        //                     int newTri = indiceBase + submeshTris[t];
        //                     subTriangles[submesh].Add(newTri);
        //                 }
        //
        //                 s++;
        //
        //                 if (s == submeshCount)
        //                     break;
        //             }
        //
        //             submesh++;
        //
        //             if (submesh > 100)
        //                 break;
        //         }
        //     }
        //     else
        //     {
        //         int submeshCount = submeshes.Length;
        //         for (int sm = 0; sm < submeshCount; sm++)
        //         {
        //             if (mesh.subMeshCount <= sm) continue;
        //             int submesh = submeshes[sm];
        //             if (!subTriangles.ContainsKey(submesh))
        //                 subTriangles.Add(submesh, new List<int>());
        //             int[] smTris = mesh.GetTriangles(sm);
        //             int smTriCount = smTris.Length;
        //             for (int t = 0; t < smTriCount; t++)
        //             {
        //                 int newTri = indiceBase + smTris[t];
        //                 subTriangles[submesh].Add(newTri);
        //             }
        //         }
        //     }
        //
        //     colours.AddRange(new Color32[vertCount]);
        // }

        /// <summary>
        /// Assumption is that the vert data is flat. Y is constant
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="tris"></param>
        /// <param name="submesh"></param>
        public void AddFlatMeshData(Vector3[] verts, int[] tris, int submesh)
        {
            if (tris.Length < 3)
            {
                return;
            }

            Vector3 v0 = verts[tris[0]];
            Vector3 v1 = verts[tris[1]];
            Vector3 v2 = verts[tris[2]];
            Vector3 normal = Triangle3D.CalculateNormal(v0, v1, v2);
            Vector3 tangentV3 = Vector3.Cross(normal, Vector3.forward);
            Vector4 tangent = Vector4Extensions.CalculateTangent(tangentV3);
            int vertCount = verts.Length;
            Vector2[] generatedUvs = new Vector2[vertCount];
            Vector3[] norms = new Vector3[vertCount];
            Vector4[] tans = new Vector4[vertCount];
            for (int v = 0; v < vertCount; v++)
            {
                generatedUvs[v] = new Vector2(verts[v].x, verts[v].z);
                norms[v] = normal;
                tans[v] = tangent;
            }

            AddData(verts, generatedUvs, tris, norms, tans, submesh);
        }

        // public void AddData(Mesh mesh, int submesh)
        // {
        //     if (mesh.subMeshCount > 1) Debug.LogError("Mesh contains more than one submesh, use AddData(Mesh mesh, int[] mappedSubmeshes)");
        //     if (MeshOverflow(mesh.vertexCount))
        //     {
        //         _overflow.AddData(mesh, submesh);
        //         return;
        //     }
        //
        //     AddData(mesh.vertices, mesh.uv, mesh.triangles, mesh.normals, mesh.tangents, submesh);
        // }
    }
}