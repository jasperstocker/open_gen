using System.Collections.Generic;
using opengen.types;
using UnityEngine;

namespace opengen.mesh
{
	public partial class DynamicMesh
	{
		private const int MinimumConvexPoly = 3;
		
		public void AddConvex(Vector3[] verts, int subMesh)
		{
			int newVertCount = verts.Length;
			
			if (newVertCount < MinimumConvexPoly)
			{
				return;
			}
			
			if (MeshOverflow(newVertCount))
			{
				_overflow.AddConvex(verts, subMesh);
				return;
			}

			Vector2[] uv = new Vector2[newVertCount];
			Vector3 p0 = verts[0];
			Vector3 p1 = verts[1];
			Vector3 p2 = verts[2];
			Vector3 normal = Triangle3D.CalculateNormal(p0, p2, p1);
			Vector4 tangent = Vector4Extensions.CalculateTangent((p1 - p0).normalized);

			AddConvex(verts, uv, normal, tangent, subMesh, null);
		}

		public void AddConvex(Vector3[] verts, Vector2[] uv, int subMesh)
		{
			int newVertCount = verts.Length;
			
			if (newVertCount < MinimumConvexPoly)
			{
				return;
			}
			
			if (MeshOverflow(newVertCount))
			{
				_overflow.AddConvex(verts, uv, subMesh);
				return;
			}
			
			Vector3 p0 = verts[0];
			Vector3 p1 = verts[1];
			Vector3 p2 = verts[2];
			Vector3 normal = Triangle3D.CalculateNormal(p0, p2, p1);
			Vector4 tangent = Vector4Extensions.CalculateTangent((p1 - p0).normalized);

			AddConvex(verts, uv, normal, tangent, subMesh, null);
		}
		
		public void AddConvex(Vector3[] verts, Vector2[] uv, Vector3 normal, Vector4 tangent, int subMesh, TextureData textureData = null)
		{
			int newVertCount = verts.Length;
			
			if (newVertCount < MinimumConvexPoly)
			{
				return;
			}
			
			if (MeshOverflow(newVertCount))
			{
				_overflow.AddConvex(verts, uv, normal, tangent, subMesh, textureData);
				return;
			}

			int indiceBase = vertices.Count;
			
			for (int i = 0; i < newVertCount; i++)
			{
				vertices.Add(verts[i]);
				uvs.Add(uv[i]);
				normals.Add(normal);
				tangents.Add(tangent);
				colours.Add(new Color32());
			}

			if (ignoreSubmeshAssignment)
			{
				subMesh = 0;
			}

			if (!subTriangles.ContainsKey(subMesh))
			{
				subTriangles.Add(subMesh, new List<int>());
			}

			int triCount = newVertCount - 2;
			for (int i = 0; i < triCount; i++)
			{
				int i0 = indiceBase;
				int i1 = indiceBase + i + 2;
				int i2 = indiceBase + i + 1;
				
				subTriangles[subMesh].Add(i0);
				subTriangles[subMesh].Add(i1);
				subTriangles[subMesh].Add(i2);
			}
		}
	}
}