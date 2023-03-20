﻿using UnityEngine;

namespace opengen.maths
{
	public static class RayIntersection
	{
		public static bool TriangleIntersection(Vector3 v0, Vector3 v1, Vector3 v2, Ray ray, out float distance, bool backFaceCull = true)
		{
			distance = 0;
			Vector3 v0v1 = v1 - v0;
			Vector3 v0v2 = v2 - v0;
			Vector3 pvec = Vector3.Cross(ray.direction, v0v2);
			float det = Vector3.Dot(v0v1, pvec);
			if (det < Vector3.kEpsilon && backFaceCull)
			{
				return false; //backfacing
			}

			if (Mathf.Abs(det) < Vector3.kEpsilon)
			{
				return false; //parallel
			}

			float invDet = 1 / det;
			Vector3 tvec = ray.origin - v0;
			float u = Vector3.Dot(tvec, pvec) * invDet;
			if (u < 0 || u > 1)
			{
				return false;
			}

			Vector3 qvec = Vector3.Cross(tvec, v0v1);
			float v = Vector3.Dot(ray.direction, qvec) * invDet;
			if (v < 0 || u + v > 1)
			{
				return false;
			}

			distance = Vector3.Dot(v0v2, qvec) * invDet;
			return true;
		}

		public static bool QuadIntersection(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Ray ray, out float distance,
			bool backFaceCull = true)
		{
			if (TriangleIntersection(v0, v1, v2, ray, out distance, backFaceCull))
			{
				return true;
			}

			if (TriangleIntersection(v2, v1, v3, ray, out distance, backFaceCull))
			{
				return true;
			}

			return false;
		}
	}
}