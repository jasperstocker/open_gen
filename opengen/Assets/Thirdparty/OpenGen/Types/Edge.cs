using UnityEngine;

namespace opengen.types
{
    public struct Edge
    {
        public Vector2 point1 { get; }
        public Vector2 point2 { get; }

        public Edge(Vector2 point1, Vector2 point2)
        {
            this.point1 = point1;
            this.point2 = point2;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            var edge = (Edge)obj;

            var samePoints = point1 == edge.point1 && point2 == edge.point2;
            var samePointsReversed = point1 == edge.point2 && point2 == edge.point1;
            return samePoints || samePointsReversed;
        }

        public override int GetHashCode()
        {
            int hCode = (int)point1.x ^ (int)point1.y ^ (int)point2.x ^ (int)point2.y;
            return hCode.GetHashCode();
        }
    }
}