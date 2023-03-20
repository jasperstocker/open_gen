namespace opengen.types
{
    public class DelaunayEdge
    {
        public DelaunayPoint Point1 { get; }
        public DelaunayPoint Point2 { get; }

        public DelaunayEdge(DelaunayPoint point1, DelaunayPoint point2)
        {
            Point1 = point1;
            Point2 = point2;
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

            var edge = obj as DelaunayEdge;

            var samePoints = Point1 == edge.Point1 && Point2 == edge.Point2;
            var samePointsReversed = Point1 == edge.Point2 && Point2 == edge.Point1;
            return samePoints || samePointsReversed;
        }

        public override int GetHashCode()
        {
            int hCode = (int)Point1.x ^ (int)Point1.y ^ (int)Point2.x ^ (int)Point2.y;
            return hCode.GetHashCode();
        }

        public static bool Equals(DelaunayEdge a, DelaunayEdge b)
        {
            bool x0 = a.Point1 == b.Point1;
            bool x1 = a.Point1 == b.Point2;
            bool x2 = a.Point2 == b.Point2;
            bool x3 = a.Point2 == b.Point1;

            return (x0 && x2) || (x1 && x3);
        }
    }
}