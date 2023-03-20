using System;
using opengen.types;
using UnityEngine;


namespace opengen.maths
{
    public static class Segments
    {
        public static bool Parallel(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            Vector2 dirA = a2 - a1;
            Vector2 dirB = b2 - b1;
            return Mathf.Abs((dirA.y * dirB.x - dirB.y * dirA.x)) < Numbers.Epsilon;
        }

        public static bool Intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection)
        {
            intersection = Vector2.zero;
            Vector2 b = a2 - a1;
            Vector2 d = b2 - b1;
            float bDotDPerp = b.x * d.y - b.y * d.x;

            // if b dot d == 0, it means the lines are parallel so have infinite intersection points
            if (Mathf.Abs(bDotDPerp) < Numbers.Epsilon)
            {
                return false;
            }

            Vector2 c = b1 - a1;
            float t = (c.x * d.y - c.y * d.x) / bDotDPerp;
            if (t < 0 || t > 1)
            {
                return false;
            }

            float u = (c.x * b.y - c.y * b.x) / bDotDPerp;
            if (u < 0 || u > 1)
            {
                return false;
            }

            intersection = a1 + t * b;
            return true;
        }

        public static bool Intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            Vector2 ax = a2 - a1;
            Vector2 bx = b2 - b1;
            float bDotDPerp = ax.x * bx.y - ax.y * bx.x;

            // if b dot d == 0, it means the lines are parallel so have no intersection points
            if (Mathf.Abs(bDotDPerp) < Numbers.Epsilon)
            {
                return false;
            }

            Vector2 c = b1 - a1;
            float t = (c.x * bx.y - c.y * bx.x) / bDotDPerp;
            if (t < 0 || t > 1)
            {
                return false;
            }

            float u = (c.x * ax.y - c.y * ax.x) / bDotDPerp;
            if (u < 0 || u > 1)
            {
                return false;
            }

            return true;
        }

        public static Vector2 ClosestPoint(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 aToP = p - a;
            Vector2 aToB = b - a;
            float aToB2 = aToB.x * aToB.x + aToB.y * aToB.y;
            float aToPDotaToB = aToP.x * aToB.x + aToP.y * aToB.y;
            float t = aToPDotaToB / aToB2;
            
            Vector2 o = new(a.x + aToB.x * t, a.y + aToB.y * t);
            Vector2 aToO = o - a;
            float aToODotaToB = aToO.x * aToB.x + aToO.y * aToB.y;
            if (aToODotaToB <= Numbers.Epsilon)
            {
                return a;
            }

            float aToO2 = aToO.x * aToO.x + aToO.y * aToO.y;
            if (aToB2 < aToO2)
            {
                return b;
            }

            return o;
        }

        public static Vector3 ClosestPoint(Vector3 a, Vector3 b, Vector3 p)
        {
            Vector3 aToP = p - a;
            Vector3 aToB = b - a;
            float aToB2 = aToB.x * aToB.x + aToB.y * aToB.y + aToB.z * aToB.z;
            float aToPDotaToB = aToP.x * aToB.x + aToP.y * aToB.y;
            float t = aToPDotaToB / aToB2;
            
            Vector3 o = new(a.x + aToB.x * t, a.y + aToB.y * t, a.z + aToB.z * t);
            Vector3 aToO = o - a;
            float aToODotaToB = aToO.x * aToB.x + aToO.y * aToB.y;
            if (aToODotaToB <= Numbers.Epsilon)
            {
                return a;
            }

            float aToO2 = aToO.x * aToO.x + aToO.y * aToO.y;
            if (aToB2 < aToO2)
            {
                return b;
            }

            return o;
        }
        
        public static bool PointOnSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            float cross = (p.y - a.y) * (b.x - a.x) - (p.x - a.x) * (b.y - a.y);
            if (Mathf.Abs(cross) > Numbers.Epsilon)
            {
                return false;
            }

            float dot = (p.x - a.x) * (b.x - a.x) + (p.y - a.y) * (b.y - a.y);
            if (dot < 0)
            {
                return false;
            }

            float squaredlengthba = (b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y);
            if (dot > squaredlengthba)
            {
                return false;
            }

            return true;
        }
        
        public static int PointOnPolySegment(Vector2[] polygon, Vector2 point)
        {
            int polySize = polygon.Length;
            for (int p = 0; p < polySize; p++)
            {
                int px = p < polySize - 1 ? p + 1 : 0;
                Vector2 a = polygon[p];
                Vector2 b = polygon[px];

                if (PointOnSegment(point, a, b))
                {
                    return p;
                }
            }

            return -1;
        }

        public static bool FastIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            if (a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2)
            {
                return false;
            }

            return (Ccw(a1, b1, b2) != Ccw(a2, b1, b2)) && (Ccw(a1, a2, b1) != Ccw(a1, a2, b2));
        }

        public static bool FastIntersection(Vector2Fixed a1, Vector2Fixed a2, Vector2Fixed b1, Vector2Fixed b2)
        {
            if (a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2)
            {
                return false;
            }

            return (Ccw(a1, b1, b2) != Ccw(a2, b1, b2)) && (Ccw(a1, a2, b1) != Ccw(a1, a2, b2));
        }

        private static bool Ccw(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return ((p2.x - p1.x) * (p3.y - p1.y) > (p2.y - p1.y) * (p3.x - p1.x));
        }

        private static bool Ccw(Vector2Fixed p1, Vector2Fixed p2, Vector2Fixed p3)
        {
            return ((p2.x - p1.x) * (p3.y - p1.y) > (p2.y - p1.y) * (p3.x - p1.x));
        }

        public static Vector2 FindIntersectionQuick(Vector2 s1, Vector2 e1, Vector2 s2, Vector2 e2)
        {
            float a1 = e1.y - s1.y;
            float b1 = s1.x - e1.x;
            float c1 = a1 * s1.x + b1 * s1.y;
            float a2 = e2.y - s2.y;
            float b2 = s2.x - e2.x;
            float c2 = a2 * s2.x + b2 * s2.y;
            float delta = a1 * b2 - a2 * b1;
            //If lines are parallel, the result will be (NaN, NaN).
            return delta == 0 ? Vector2.zero : new Vector2((b2 * c1 - b1 * c2) / delta, (a1 * c2 - a2 * c1) / delta);
        }
        
        public static Vector2 FindIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            if (Mathf.Abs(Mathf.Abs(Vector2.Dot(a2, b2)) - 1.0f) < Numbers.Epsilon)
            {
                return Vector2.zero;
            }

            Vector2 intersectionPoint = IntersectionPoint(a2, a1, b1, b2);
            if (float.IsNaN(intersectionPoint.x) || float.IsNaN(intersectionPoint.y))
            {
                //flip the second line to find the intersection point
                intersectionPoint = IntersectionPoint(a2, a1, b1, b2);
            }

            if (float.IsNaN(intersectionPoint.x) || float.IsNaN(intersectionPoint.y))
            {
                //            Debug.Log(intersectionPoint.x+" "+intersectionPoint.y);
                intersectionPoint = a1 + a2;
            }

            return intersectionPoint;
        }

        public static Vector2 FindIntersection(Vector2 a0, Vector2 a1, Vector2[] poly)
        {
            int polySize = poly.Length;
            for (int p = 0; p < polySize; p++)
            {
                int px = p < polySize - 1 ? p + 1 : 0;
                Vector2 p0 = poly[p];
                Vector2 p1 = poly[px];
                if (FastIntersection(a0, a1, p0, p1))
                {
                    return FindIntersection(a0, a1, p0, p1);
                }
            }

            return Vector2.zero;
        }
        
        public static Vector2 IntersectionPoint(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            Vector2 intersection = new();
            float Ax, Bx, Cx, Ay, By, Cy, d, e, f, num /*,offset*/;
            float x1lo, x1hi, y1lo, y1hi;
            Ax = p2.x - p1.x;
            Bx = p3.x - p4.x;
            // X bound box test/
            if (Ax < 0)
            {
                x1lo = p2.x;
                x1hi = p1.x;
            }
            else
            {
                x1hi = p2.x;
                x1lo = p1.x;
            }

            if (Bx > 0)
            {
                if (x1hi < p4.x || p3.x < x1lo)
                {
                    return Vector2.zero;
                }
            }
            else
            {
                if (x1hi < p3.x || p4.x < x1lo)
                {
                    return Vector2.zero;
                }
            }

            Ay = p2.y - p1.y;
            By = p3.y - p4.y;
            // Y bound box test//
            if (Ay < 0)
            {
                y1lo = p2.y;
                y1hi = p1.y;
            }
            else
            {
                y1hi = p2.y;
                y1lo = p1.y;
            }

            if (By > 0)
            {
                if (y1hi < p4.y || p3.y < y1lo)
                {
                    return Vector2.zero;
                }
            }
            else
            {
                if (y1hi < p3.y || p4.y < y1lo)
                {
                    return Vector2.zero;
                }
            }

            Cx = p1.x - p3.x;
            Cy = p1.y - p3.y;
            d = By * Cx - Bx * Cy; // alpha numerator//
            f = Ay * Bx - Ax * By; // both denominator//

            // alpha tests//
            if (f > 0)
            {
                if (d < 0 || d > f)
                {
                    return Vector2.zero;
                }
            }
            else
            {
                if (d > 0 || d < f)
                {
                    return Vector2.zero;
                }
            }

            e = Ax * Cy - Ay * Cx; // beta numerator//

            // beta tests //
            if (f > 0)
            {
                if (e < 0 || e > f)
                {
                    return Vector2.zero;
                }
            }
            else
            {
                if (e > 0 || e < f)
                {
                    return Vector2.zero;
                }
            }

            // check if they are parallel
            if (Mathf.Abs(f) < Numbers.Epsilon)
            {
                return Vector2.zero;
            }

            // compute intersection coordinates //
            num = d * Ax; // numerator //
            intersection.x = p1.x + num / f;
            num = d * Ay;
            intersection.y = p1.y + num / f;
            return intersection;
        }

        public static bool Collinear(Vector2Fixed a, Vector2Fixed b, Vector2Fixed c, float epsilon = 0.0001f)
        {
            float detleft = (a.x - c.x) * (b.y - c.y);
            float detright = (a.y - c.y) * (b.x - c.x);
            float val = detleft - detright;
            return (val > -Numbers.Epsilon && val < epsilon);
        }

        public static bool Collinear(Vector2 a, Vector2 b, Vector2 c, float epsilon = 0.0001f)
        {
            float detleft = (a.x - c.x) * (b.y - c.y);
            float detright = (a.y - c.y) * (b.x - c.x);
            float val = detleft - detright;
            return (val > -epsilon && val < epsilon);
        }
        
        private static float FindNearestPoints(Vector2 point, Vector2 p1, Vector2 p2, out Vector2 closest)
        {
            float dx = p2.x - p1.x;
            float dy = p2.y - p1.y;
            if ((Mathf.Abs(dx) < Numbers.Epsilon) && (Mathf.Abs(dy) < Numbers.Epsilon))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = point.x - p1.x;
                dy = point.y - p1.y;
                return Mathf.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            float t = ((point.x - p1.x) * dx + (point.y - p1.y) * dy) / (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new Vector2(p1.x, p1.y);
                dx = point.x - p1.x;
                dy = point.y - p1.y;
            }
            else if (t > 1)
            {
                closest = new Vector2(p2.x, p2.y);
                dx = point.x - p2.x;
                dy = point.y - p2.y;
            }
            else
            {
                closest = new Vector2(p1.x + t * dx, p1.y + t * dy);
                dx = point.x - closest.x;
                dy = point.y - closest.y;
            }

            return Mathf.Sqrt(dx * dx + dy * dy);
        }
        
        public static float FindNearestPoints
        (
            Vector2 segment0A, Vector2 segment0B, Vector2 segment1A, Vector2 segment1B,
            out Vector2 segment0P, out Vector2 segment1P)
        {
            segment0P = segment0A;
            segment1P = segment1A;
            
            // See if the segments intersect.
            if (Intersects(segment0A,segment0B,segment1A,segment1B, out Vector2 intersection))
            {
                // They intersect.
                segment0P = intersection;
                segment1P = intersection;
                return 0;
            }

            // Find the other possible distances.
            Vector2 closest;
            float best_dist = float.MaxValue, test_dist;

            // Try p1.
            test_dist = FindNearestPoints(segment0A, segment1A, segment1B, out closest);
            if (test_dist < best_dist)
            {
                best_dist = test_dist;
                segment0P = segment0A;
                segment1P = closest;
            }

            // Try p2.
            test_dist = FindNearestPoints(segment0B, segment1A, segment1B, out closest);
            if (test_dist < best_dist)
            {
                best_dist = test_dist;
                segment0P = segment0B;
                segment1P = closest;
            }

            // Try p3.
            test_dist = FindNearestPoints(segment1A, segment0A, segment0B, out closest);
            if (test_dist < best_dist)
            {
                best_dist = test_dist;
                segment0P = closest;
                segment1P = segment1A;
            }

            // Try p4.
            test_dist = FindNearestPoints(segment1B, segment0A, segment0B, out closest);
            if (test_dist < best_dist)
            {
                best_dist = test_dist;
                segment0P = closest;
                segment1P = segment1B;
            }

            return best_dist;
        }

        public static Vector2 CalculateBisector(Vector2 a, Vector2 b, Vector2 c)
        {
            return CalculateBisector(a, b, b, c);
        }

        public static Vector2 CalculateBisector(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            // Calculate the midpoint of each segment
            Vector2 midA = new ((a1.x + a2.x) / 2.0f, (a1.y + a2.y) / 2.0f);
            Vector2 midB = new ((b1.x + b2.x) / 2.0f, (b1.y + b2.y) / 2.0f);

            // Calculate the direction vectors of each segment
            float dirA_x = a2.x - a1.x;
            float dirA_y = a2.y - a1.y;
            float dirB_x = b2.x - b1.x;
            float dirB_y = b2.y - b1.y;

            // Calculate the angle between the two direction vectors
            float angle = Mathf.Atan2(dirA_y, dirA_x) - Mathf.Atan2(dirB_y, dirB_x);

            // Calculate the length of the bisector
            float length = Mathf.Sqrt(dirA_x * dirA_x + dirA_y * dirA_y) + Mathf.Sqrt(dirB_x * dirB_x + dirB_y * dirB_y);

            // Calculate the direction vector of the bisector
            float bisector_x = Mathf.Cos(angle / 2.0f) * length;
            float bisector_y = Mathf.Sin(angle / 2.0f) * length;

            // Normalize the bisector vector
            float magnitude = Mathf.Sqrt(bisector_x * bisector_x + bisector_y * bisector_y);
            bisector_x /= magnitude;
            bisector_y /= magnitude;

            // Calculate the point on the bisector
            Vector2 bisector = new (midA.x + bisector_x, midA.y + bisector_y);

            return bisector;
        }

        public static Vector2 CalculateAngularBisector(Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 ab = b - a;
            Vector2 bc = c - b;

            float angle = Vector2.Angle(ab, bc);
            float length = ab.magnitude + bc.magnitude;

            Vector2 bisector = new (Mathf.Cos(angle / 2.0f) * length, Mathf.Sin(angle / 2.0f) * length);

            return bisector;
        }

        public static Vector2[] SplitSegment(Vector2 start, Vector2 finish, float minimumLength)
        {
            Vector2[] result = new Vector2[2];
            Vector2 vector = finish - start;
            Vector2 direction = vector.normalized;
            float length = vector.magnitude;
            int outputSize = Mathf.FloorToInt(length / minimumLength);
            Vector2[] output = new Vector2[outputSize];
            for (int i = 0; i < outputSize; i++)
            {
                if (i == outputSize - 1)
                {
                    output[i] = finish;
                }
                else
                {
                    output[i] = start + direction * (i + 1) / outputSize;
                }
            }

            return result;
        }
    }
}