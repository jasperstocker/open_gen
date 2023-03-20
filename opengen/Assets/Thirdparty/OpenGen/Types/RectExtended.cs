using System.Collections.Generic;
using UnityEngine;

namespace opengen.types
{
    public static class RectExtended
    {
        public static void Encapsulate(ref this Rect value, Vector2 point)
        {
            if(value.Contains(point))
            {
                return;
            }

            if(value.xMin > point.x)
            {
                value.xMin = point.x;
            }
            else if(value.xMax < point.x)
            {
                value.xMax = point.x;
            }

            if(value.yMin > point.y)
            {
                value.yMin = point.y;
            }
            else if(value.yMax < point.y)
            {
                value.yMax = point.y;
            }
        }
        public static void Encapsulate(ref this Rect value, float x, float y)
        {
            Encapsulate(ref value, new Vector2(x, y));
        }

        public static void Encapsulate(ref this Rect value, IList<Vector2> points)
        {
            int pointSize = points.Count;
            for(int p = 0; p < pointSize; p++)
            {
                Encapsulate(ref value, points[p]);
            }
        }

        public static void Encapsulate(ref this Rect value, Rect rectangle)
        {
            Encapsulate(ref value, rectangle.min);
            Encapsulate(ref value, rectangle.max);
        }

        public static void Encapsulate(ref this Rect value, AABBox bounds)
        {
            Encapsulate(ref value, bounds.min);
            Encapsulate(ref value, bounds.max);
        }

        public static float Area(this Rect value)
        {
            return value.height * value.width;
        }

        public static void DebugDraw(this Rect value)
        {
            Vector3[] points = GetPointsV3(value);
            for (int i = 0; i < 4; i++)
            {
                int ib = i < 3 ? i + 1 : 0;
                Debug.DrawLine(points[i], points[ib]);
            }
        }

        public static Vector2[] GetPoints(this Rect value)
        {
            Vector2[] points = new Vector2[4];
            points[0] = value.min;
            points[1] = new Vector2(value.xMax, value.yMin);
            points[2] = value.max;
            points[3] = new Vector2(value.xMin, value.yMax);
            return points;
        }

        public static Vector3[] GetPointsV3(this Rect value)
        {
            Vector2[] points = GetPoints(value);
            Vector3[] output = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                output[i] = new Vector3(points[i].x, 0, points[i].y);
            }

            return output;
        }
    }
}