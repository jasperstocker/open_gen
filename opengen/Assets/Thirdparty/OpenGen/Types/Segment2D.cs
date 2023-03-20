using opengen.maths;
using UnityEngine;

namespace opengen.types
{
	public class Segment2D
	{
		private Vector2 _p0;
		private Vector2 _p1;
		private SegmentShape _s0;
		private SegmentShape _s1;

		public Vector2 p0 => _p0;
		public Vector2 p1 => _p1;
		public SegmentShape s0 => _s0;
		public SegmentShape s1 => _s1;

		public Segment2D(Vector2 a, Vector2 b)
		{
			_p0 = a;
			_p1 = b;
			_s0 = null;
			_s1 = null;
		}

		public Segment2D(Vector2 a, Vector2 b, SegmentShape initialShape)
		{
			_p0 = a;
			_p1 = b;
			_s0 = initialShape;
			_s1 = null;
		}

		public Segment2D(Vector2 a, Vector2 b, SegmentShape shape0, SegmentShape shape1)
		{
			_p0 = a;
			_p1 = b;
			_s0 = shape0;
			_s1 = shape1;
		}

		public bool Intersects(Vector2 intersectionLine0, Vector2 intersectionLine1, out Vector2 intersectionPoint)
		{
			if (Segments.Intersects(intersectionLine0, intersectionLine1, _p0, _p1))
			{
				intersectionPoint = Segments.FindIntersection(intersectionLine0, intersectionLine1, _p0, _p1);
				return true;
			}
			
			intersectionPoint = Vector2.zero;
			return false;
		}

		public float SquareMagnitude(Segment2D other)
		{
			float sqr0 = (_p0 - other._p0).sqrMagnitude;
			float sqr1 = (_p1 - other._p0).sqrMagnitude;
			float sqr2 = (_p0 - other._p1).sqrMagnitude;
			float sqr3 = (_p1 - other._p1).sqrMagnitude;
			return Mathf.Min(sqr0, sqr1, sqr2, sqr3);
		}

		public float SquareMagnitude(Vector2 point)
		{
			float sqr0 = (_p0 - point).sqrMagnitude;
			float sqr1 = (_p1 - point).sqrMagnitude;
			return Mathf.Min(sqr0, sqr1);
		}

		public Vector2 NextPoint(Segment2D previous)
		{
			float p0sqmg = previous.SquareMagnitude(_p0);
			float p1sqmg = previous.SquareMagnitude(_p1);

			if (p0sqmg < p1sqmg)
			{
				return _p1;
			}

			return _p0;
		}

		public void ReplaceShapeReference(SegmentShape oldReference, SegmentShape newReference)
		{
			if (_s0 == oldReference)
			{
				_s0 = newReference;
			}

			if (_s1 == oldReference)
			{
				_s1 = newReference;
			}
		}

		public void SetShape0(SegmentShape shape)
		{
			_s0 = shape;
		}

		public void SetShape1(SegmentShape shape)
		{
			_s1 = shape;
		}
	}
}