using opengen.types;
using UnityEngine;
// ReSharper disable UnusedMember.Local - library class

namespace opengen.maths
{
    public static class Lines
    {
        public static bool IsParallel(Vector2 dirA, Vector2 dirB)
        {
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
            intersection = a1 + t * b;
            return true;
        }

        public static bool Intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            Vector2 b = a2 - a1;
            Vector2 d = b2 - b1;
            float bDotDPerp = b.x * d.y - b.y * d.x;

            // if b dot d == 0, it means the lines are parallel so have infinite intersection points
            if (Mathf.Abs(bDotDPerp) < Numbers.Epsilon)
            {
                return false;
            }
            
            return true;
        }

        public static Vector3 ClosestPointOnLine(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 v1 = point - a;
            Vector3 v2 = b - a;
            float sqrMag = v2.sqrMagnitude;
            float dot = Vector3.Dot(v1, v2);
            float t = dot / sqrMag;
            Vector3 v3 = a + v2 * t;
            return v3;
        }

        public static Vector2 ClosestPointOnLine(Vector2 a, Vector2 b, Vector2 point)
        {
            Vector2 v1 = point - a;
            Vector2 v2 = b - a;
            float sqrMag = v2.sqrMagnitude;
            float dot = Vector2.Dot(v1, v2);
            float t = dot / sqrMag;
            Vector2 v3 = a + v2 * t;
            return v3;
        }

        public static Vector2Fixed ClosestPointOnLine(Vector2Fixed a, Vector2Fixed b, Vector2Fixed p)
        {
            Vector2Fixed aToP = p - a;
            Vector2Fixed aToB = b - a;
            float aToB2 = aToB.x * aToB.x + aToB.y * aToB.y;
            float aToPDotAToB = aToP.x * aToB.x + aToP.y * aToB.y;
            float t = aToPDotAToB / aToB2;
            return new Vector2Fixed(a.vx + aToB.vx * t, a.vy + aToB.vy * t);
        }

        public static Vector2Fixed ClosestPointOnSegment(Vector2Fixed a, Vector2Fixed b, Vector2Fixed p)
        {
            Vector2Fixed aToP = p - a;
            Vector2Fixed aToB = b - a;
            float aToB2 = aToB.x * aToB.x + aToB.y * aToB.y;
            float aToPDotAToB = aToP.x * aToB.x + aToP.y * aToB.y;
            float t = aToPDotAToB / aToB2;
            Vector2Fixed o = new(a.vx + aToB.vx * t, a.vy + aToB.vy * t);
            Vector2Fixed aToO = o - a;
            float aToODotAToB = aToO.x * aToB.x + aToO.y * aToB.y;
            if (aToODotAToB <= Numbers.Epsilon)
            {
                return a;
            }

            float aToO2 = aToO.x * aToO.x + aToO.y * aToO.y;
            return aToB2 < aToO2 ? b : o;
        }

        public static Vector2 ClosestPointOnSegment(Vector2 a, Vector2 b, Vector2 p)
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

        public static bool PointOnLine(Vector2Fixed p, Vector2Fixed a, Vector2Fixed b)
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

        public static bool PointOnLine(Vector2 p, Vector2 a, Vector2 b)
        {
            float cross = (p.y - a.y) * (b.x - a.x) - (p.x - a.x) * (b.y - a.y);
            if (Mathf.Abs(cross) > Numbers.Epsilon)
            {
                return false;
            }

            return true;
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

        public static bool FastLineIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            if (a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2)
            {
                return false;
            }

            return (Ccw(a1, b1, b2) != Ccw(a2, b1, b2)) && (Ccw(a1, a2, b1) != Ccw(a1, a2, b2));
        }

        public static bool FastLineIntersection(Vector2Fixed a1, Vector2Fixed a2, Vector2Fixed b1, Vector2Fixed b2)
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

            Vector2 intersectionPoint = IntersectionPoint4(a2, a1, b1, b2);
            if (float.IsNaN(intersectionPoint.x) || float.IsNaN(intersectionPoint.y))
            {
                //flip the second line to find the intersection point
                intersectionPoint = IntersectionPoint4(a2, a1, b1, b2);
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
                if (FastLineIntersection(a0, a1, p0, p1))
                {
                    return FindIntersection(a0, a1, p0, p1);
                }
            }

            return Vector2.zero;
        }

        // public static Vector2 FindIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, Vector3 debugOrigin)
        // {
        //     Vector2 intersectionPoint = FindIntersection(a1, a2, b1, b2);
        //     // Vector3 ta1 = new Vector3(a1.x, 0, a1.y) + debugOrigin;
        //     // Vector3 ta2 = new Vector3(a2.x, 0, a2.y) + debugOrigin;
        //     // Vector3 tb1 = new Vector3(b1.x, 0, b1.y) + debugOrigin;
        //     // Vector3 tb2 = new Vector3(b2.x, 0, b2.y) + debugOrigin;
        //     // Debug.DrawLine(ta1, ta2, Color.magenta, .5f);
        //     // Debug.DrawLine(tb1, tb2, Color.yellow, .5f);
        //     // Vector3 intersectionPointV3 = new Vector3(intersectionPoint.x, 0, intersectionPoint.y) + debugOrigin;
        //     // Debug.DrawLine(intersectionPointV3, intersectionPointV3 + Vector3.up, Color.red, .5f);
        //     return intersectionPoint;
        // }

        private static Vector2 IntersectionPoint(Vector2 lineA, Vector2 originA, Vector2 lineB, Vector2 originB)
        {
            // calculate differences  
            float xD1 = lineA.x;
            float xD2 = lineB.x;
            float yD1 = lineA.y;
            float yD2 = lineB.y;
            float xD3 = originA.x - originB.x;
            float yD3 = originA.y - originB.y;

            // find intersection Pt between two lines    
            Vector2 pt = new (0, 0);
            float div = yD2 * xD1 - xD2 * yD1;
            float ua = (xD2 * yD3 - yD2 * xD3) / div;
            pt.x = originA.x + ua * xD1;
            pt.y = originA.y + ua * yD1;

            // return the valid intersection  
            return pt;
        }

        private static Vector2 IntersectionPoint2(Vector2 lineA, Vector2 originA, Vector2 lineB, Vector2 originB)
        {
            Vector2 lineA2 = lineA + originA;
            Vector2 lineB2 = lineB + originB;
            Vector3 crossA = Vector3.Cross(new Vector3(lineA.x, lineA.y, 1), new Vector3(lineA2.x, lineA2.y, 1));
            Vector3 crossB = Vector3.Cross(new Vector3(lineB.x, lineB.y, 1), new Vector3(lineB2.x, lineB2.y, 1));
            Vector3 crossAB = Vector3.Cross(crossA, crossB);
            Vector2 pt = new(0, 0);
            pt.x = crossAB.x / crossAB.z;
            pt.x = crossAB.y / crossAB.z;

            // return the valid intersection  
            return pt;
        }

        public static Vector2 IntersectionPoint4(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            Vector2 intersection = new();
            float x1lo, x1hi, y1lo, y1hi;
            float Ax /*,offset*/ = p2.x - p1.x;
            float Bx = p3.x - p4.x;
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

            float Ay = p2.y - p1.y;
            float By = p3.y - p4.y;
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

            float Cx = p1.x - p3.x;
            float Cy = p1.y - p3.y;
            float d = By * Cx - Bx * Cy; // alpha numerator//
            float f = Ay * Bx - Ax * By; // both denominator//

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

            float e = Ax * Cy - Ay * Cx; // beta numerator//

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
            float num = d * Ax; // numerator //
            intersection.x = p1.x + num / f;
            num = d * Ay;
            intersection.y = p1.y + num / f;
            return intersection;
        }

        private static float FindDistanceToSegment(Vector2 pt, Vector2 p1, Vector2 p2, out Vector2 closest)
        {
            float dx = p2.x - p1.x;
            float dy = p2.y - p1.y;
            if ((Mathf.Abs(dx) < Numbers.Epsilon) && (Mathf.Abs(dy) < Numbers.Epsilon))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = pt.x - p1.x;
                dy = pt.y - p1.y;
                return Mathf.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            float t = ((pt.x - p1.x) * dx + (pt.y - p1.y) * dy) / (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new Vector2(p1.x, p1.y);
                dx = pt.x - p1.x;
                dy = pt.y - p1.y;
            }
            else if (t > 1)
            {
                closest = new Vector2(p2.x, p2.y);
                dx = pt.x - p2.x;
                dy = pt.y - p2.y;
            }
            else
            {
                closest = new Vector2(p1.x + t * dx, p1.y + t * dy);
                dx = pt.x - closest.x;
                dy = pt.y - closest.y;
            }

            return Mathf.Sqrt(dx * dx + dy * dy);
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

        // Return the shortest distance between the two segments
        // p1 --> p2 and p3 --> p4.
        public static float FindDistanceBetweenSegments(
            Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4,
            out Vector2 close1, out Vector2 close2)
        {
            close1 = p1;
            close2 = p3;
            
            // See if the segments intersect.
            if (Intersects(p1,p2,p3,p4, out Vector2 intersection))
            {
                // They intersect.
                close1 = intersection;
                close2 = intersection;
                return 0;
            }

            // Find the other possible distances.
            Vector2 closest;
            float best_dist = float.MaxValue;

            // Try p1.
            float test_dist = FindDistanceToSegment(p1, p3, p4, out closest);
            if (test_dist < best_dist)
            {
                best_dist = test_dist;
                close1 = p1;
                close2 = closest;
            }

            // Try p2.
            test_dist = FindDistanceToSegment(p2, p3, p4, out closest);
            if (test_dist < best_dist)
            {
                best_dist = test_dist;
                close1 = p2;
                close2 = closest;
            }

            // Try p3.
            test_dist = FindDistanceToSegment(p3, p1, p2, out closest);
            if (test_dist < best_dist)
            {
                best_dist = test_dist;
                close1 = closest;
                close2 = p3;
            }

            // Try p4.
            test_dist = FindDistanceToSegment(p4, p1, p2, out closest);
            if (test_dist < best_dist)
            {
                best_dist = test_dist;
                close1 = closest;
                close2 = p4;
            }

            return best_dist;
        }
    }
}