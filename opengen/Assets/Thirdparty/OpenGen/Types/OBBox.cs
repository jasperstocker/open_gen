using System;
using System.Collections.Generic;
using opengen.maths;
using UnityEngine;

namespace opengen.types
{
    [Serializable]
    public struct OBBox
    {
        [SerializeField]private bool init;
        [SerializeField]private Vector2 _centre;
        [SerializeField]private Vector2 _size;
        [SerializeField]private float _rotation;
        [SerializeField]private Rectangle _rectangle;
        [SerializeField]private Rect _rect;
        [SerializeField]private Vector2[] _points;
        [SerializeField]private float _area;
        [SerializeField]private float _aspect;
        [SerializeField]private Vector2 _longDir;
        [SerializeField]private Vector2 _shortDir;

        public Vector2 center
        {
            get {return _centre;}
            set
            {
                _centre = value;
                CalculateInternals();
            }
        }

        public Vector2 size
        {
            get {return _size;}
            set
            {
                _size = value;
                CalculateInternals();
            }
        }

        public float rotation
        {
            get {return _rotation;}
            set
            {
                _rotation = value;
                CalculateInternals();
            }
        }

        public float area {get {return _area;}}
        public float aspect {get {return _aspect;}}
        public Vector2 longDir {get {return _longDir;}}
        public Vector2 shortDir {get {return _shortDir;}}
        public float longSize {get; private set;}
        public float shortSize {get; private set;}

        public Vector2 p0 {get {return _points[0];}}
        public Vector2 p1 {get {return _points[1];}}
        public Vector2 p2 {get {return _points[2];}}
        public Vector2 p3 {get {return _points[3];}}
        public Rect rect { get{return new Rect(_centre, _size);} }
        public Vector2[] Points
        {
            get { return _points; }
        }

        public Rect bounds
        {
            get { return _rect; }
        }

        public OBBox(Vector2 centre, Vector2 size, float rotation) : this()
        {
            init = true;
            _centre = centre;
            _size = size;
            _rotation = rotation;
            _rectangle = new Rectangle();
            _rect = new Rect();
            _points = new Vector2[0];
            CalculateInternals();
        }

        public OBBox(Rect bounds, float rotation) : this()
        {
            init = true;
            _centre = bounds.center;
            _size = bounds.size;
            _rotation = rotation;
            _rectangle = new Rectangle();
            _rect = new Rect();
            _points = new Vector2[0];
            CalculateInternals();
            
            longSize = Mathf.Max(bounds.size.x, bounds.size.y);
            shortSize = Mathf.Min(bounds.size.x, bounds.size.y);
            _area = bounds.size.x * bounds.size.y;
            _aspect = bounds.size.x / bounds.size.y;
            
            Vector2 right = Vector2.right.Rotate(-rotation);
            Vector2 up = right.Rotate90Clockwise();
            
            if (_aspect > 1)
            {
                _longDir = right;
                _shortDir = up;
            }
            else
            {
                _longDir = up;
                _shortDir = right;
            }
        }

        public OBBox(Vector2 newCenter, Vector2 dirA, float sizeA, Vector2 dirB, float sizeB) : this()
        {
            init = true;
            _centre = newCenter;
            _size = new Vector2(sizeA, sizeB);
            _rotation = Angles.SignAngle(dirA);
            _rectangle = new Rectangle();
            _rect = new Rect();
            _points = new Vector2[0];
            CalculateInternals();
            _longDir = sizeA > sizeB ? dirA : dirB;
            _shortDir = sizeB > sizeA ? dirA : dirB;
            longSize = Mathf.Max(sizeA, sizeB);
            shortSize = Mathf.Min(sizeA, sizeB);
            _area = sizeA * sizeB;
            _aspect = sizeA / sizeB;
        }

        public OBBox Clone()
        {
            OBBox output = new OBBox();
            output.init = true;
            output._centre = _centre;
            output._size = _size;
            output._rotation = _rotation;
            output._rectangle = _rectangle;
            output._rect = _rect;
            output._points = _points;
            return output;
        }

        public void Clear()
        {
            _centre = Vector2.zero;
            _size = Vector2.zero;
            _rotation = 0;
            _rectangle = new Rectangle();
            _rect = new Rect();
            _points = new Vector2[0];
        }

        public void SetValues(Vector2 newCenter, Vector2 dirA, float sizeA, Vector2 dirB, float sizeB)
        {
            _centre = newCenter;
            _size = new Vector2(sizeA, sizeB);
            _rectangle = new Rectangle();
            _points = new Vector2[0];
            _rotation = Angles.SignAngle(dirA);
            CalculateInternals();
        }

        public bool Intersects(OBBox other)
        {
            if(!_rect.Overlaps(other._rect))
            {
                return false;
            }

            return IsRectIntersecting(_rectangle, other._rectangle);
        }

        public bool Intersects(Rect other)
        {
            if(!_rect.Overlaps(other))
            {
                return false;
            }

            return IsRectIntersecting(_rectangle, new Rectangle(other));
        }

        public void Encapsulate(OBBox other)
        {
            if(!init)
            {
                _centre = other._centre;
            }

            Encapsulate(other.p0);
            Encapsulate(other.p1);
            Encapsulate(other.p2);
            Encapsulate(other.p3);
        }

        public void Encapsulate(Vector2 center, Vector2 size)
        {
            if(!init)
            {
                _centre = center;
            }

            Encapsulate(center - size * 0.5f);
            Encapsulate(center + size * 0.5f);
        }

        public void Encapsulate(Vector2 point)
        {
            if(!init)
            {
                _centre = point;
            }

            Vector2 diff = point - _centre;
            Vector2 diffNorm = diff.Rotate(-_rotation);
            _size.x = Mathf.Max(_size.x, Mathf.Abs(diffNorm.x));
            _size.y = Mathf.Max(_size.y, Mathf.Abs(diffNorm.y));
            CalculateInternals();
        }

        public bool Contains(Vector2 point)
        {
            if(!init)
            {
                return false;
            }

            return PointTriangle(_rectangle, point);


            //https://gamedev.stackexchange.com/questions/44110/point-inside-oriented-bounding-box
//            if(init && _aabBox.Contains(point))
//            {    
//                float xx = (point.x - _centre.x);
//                float yy =  (point.y - _centre.y);
//                float newx = (xx * Mathf.Cos(_rotation) - yy * Mathf.Sin(_rotation));
//                float newy = (xx * Mathf.Sin(_rotation) + yy * Mathf.Cos(_rotation));
//                return (newy > center.y - (_size.y / 2)) && 
//                       (newy < center.y + (_size.y / 2)) 
//                       && (newx > center.x - (_size.x / 2)) && 
//                       (newx < center.x + (_size.x / 2));
//            }
//
//            return false;
        }

        private void CalculateInternals()
        {
            _points = new Vector2[4];
            Vector2 hsize = _size * 0.5f;
            //clockwise
            _points[0] = _centre + new Vector2(-hsize.x, -hsize.y).Rotate(-_rotation);
            _points[1] = _centre + new Vector2(hsize.x, -hsize.y).Rotate(-_rotation);
            _points[2] = _centre + new Vector2(hsize.x, hsize.y).Rotate(-_rotation);
            _points[3] = _centre + new Vector2(-hsize.x, hsize.y).Rotate(-_rotation);
            _rect = new Rect(_centre, Vector2.zero);
            _rect.Encapsulate(_points[0]);
            _rect.Encapsulate(_points[1]);
            _rect.Encapsulate(_points[2]);
            _rect.Encapsulate(_points[3]);
            if(_size.x > _size.y)
            {
                _longDir = new Vector2(Mathf.Sin(Numbers.Rad2Deg * _rotation) * _size.x, Mathf.Cos(Numbers.Rad2Deg * _rotation) * _size.y);
                _shortDir = new Vector2(Mathf.Cos(Numbers.Rad2Deg * _rotation) * _size.x, Mathf.Sin(Numbers.Rad2Deg * _rotation) * _size.y);
            }
            else
            {
                _longDir = new Vector2(Mathf.Sin(Numbers.Rad2Deg * _rotation) * _size.y, Mathf.Cos(Numbers.Rad2Deg * _rotation) * _size.x);
                _shortDir = new Vector2(Mathf.Cos(Numbers.Rad2Deg * _rotation) * _size.y, Mathf.Sin(Numbers.Rad2Deg * _rotation) * _size.x);
            }

            longSize = Mathf.Max(_size.x, _size.y);
            shortSize = Mathf.Min(_size.x, _size.y);
            _rectangle = new Rectangle(this);
            _area = size.x * size.y;
            _aspect = size.x / size.y;
            init = true;
        }

        public override string ToString()
        {
            return string.Format("{0} : {1} : {2}", _size, _centre, _rotation);
        }

        public string ToStringPoints()
        {
            string output = $"{_size} : {_centre} : {_rotation} : {_points.Length} .. <";
            for(int p = 0; p < _points.Length; p++)
            {
                output += $"[{p}, {_points[p]}]";
            }

            output += ">";
            return output;
        }

        [Serializable]
        private struct Rectangle
        {
            public Vector2 FL, FR, BL, BR;

            public Rectangle(OBBox box)
            {
                FL = new Vector2(box.p0.x, box.p0.y);
                FR = new Vector2(box.p1.x, box.p1.y);
                BL = new Vector2(box.p2.x, box.p2.y);
                BR = new Vector2(box.p3.x, box.p3.y);
            }

            public Rectangle(AABBox box)
            {
                FL = new Vector2(box.xMin, box.yMin);
                FR = new Vector2(box.xMax, box.yMin);
                BL = new Vector2(box.xMin, box.yMax);
                BR = new Vector2(box.xMax, box.yMax);
            }

            public Rectangle(Rect box)
            {
                FL = new Vector2(box.xMin, box.yMin);
                FR = new Vector2(box.xMax, box.yMin);
                BL = new Vector2(box.xMin, box.yMax);
                BR = new Vector2(box.xMax, box.yMax);
            }
        }

        private bool PointTriangle(Rectangle r1, Vector2 point)
        {
            if(IsPointInTriangle(point,r1.FL, r1.FR, r1.BR))
            {
                return true;
            }

            if(IsPointInTriangle(point,r1.FL, r1.BR, r1.BL))
            {
                return true;
            }

            return false;
        }

        private bool IsRectIntersecting(Rectangle r1, Rectangle r2)
        {
            Triangle r1_t1 = new (r1.FL, r1.FR, r1.BR);
            Triangle r1_t2 = new (r1.FL, r1.BR, r1.BL);
            Triangle r2_t1 = new (r2.FL, r2.FR, r2.BR);
            Triangle r2_t2 = new (r2.FL, r2.BR, r2.BL);
            
            if(IsTriangleTriangleIntersecting(r1_t1, r2_t1))
            {
                return true;
            }

            if(IsTriangleTriangleIntersecting(r1_t1, r2_t2))
            {
                return true;
            }

            if(IsTriangleTriangleIntersecting(r1_t2, r2_t2))
            {
                return true;
            }

            if(IsTriangleTriangleIntersecting(r1_t2, r2_t1))
            {
                return true;
            }

            return false;
        }

        private bool IsTriangleTriangleIntersecting(Triangle triangle1, Triangle triangle2)
        {
            bool isIntersecting = false;

            //Step 2. Line segment - triangle intersection
            if(AreAnyLineSegmentsIntersecting(triangle1, triangle2))
            {
                isIntersecting = true;
            }
            //Step 3. Point in triangle intersection - if one of the triangles is inside the other
            else if(AreCornersIntersecting(triangle1, triangle2))
            {
                isIntersecting = true;
            }

            return isIntersecting;
        }

        private bool AreAnyLineSegmentsIntersecting(Triangle t1, Triangle t2)
        {
            bool isIntersecting = false;
            for(int i = 0; i < t1.lineSegments.Length; i++)
            {
                for(int j = 0; j < t2.lineSegments.Length; j++)
                {
                    Vector2 t1_p1 = t1.lineSegments[i].p1;
                    Vector2 t1_p2 = t1.lineSegments[i].p2;
                    Vector2 t2_p1 = t2.lineSegments[j].p1;
                    Vector2 t2_p2 = t2.lineSegments[j].p2;
                    if(AreLineSegmentsIntersecting(t1_p1, t1_p2, t2_p1, t2_p2))
                    {
                        isIntersecting = true;
                        //To stop the outer for loop
                        i = int.MaxValue - 1;
                        break;
                    }
                }
            }

            return isIntersecting;
        }

        private bool AreLineSegmentsIntersecting(Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1)
        {
            bool isIntersecting = false;
            float denominator = (b1.y - b0.y) * (a1.x - a0.x) - (b1.x - b0.x) * (a1.y - a0.y);

            //Make sure the denominator is != 0, if 0 the lines are parallel
            if(denominator != 0)
            {
                float u_a = ((b1.x - b0.x) * (a0.y - b0.y) - (b1.y - b0.y) * (a0.x - b0.x)) / denominator;
                float u_b = ((a1.x - a0.x) * (a0.y - b0.y) - (a1.y - a0.y) * (a0.x - b0.x)) / denominator;

                //Is intersecting if u_a and u_b are between 0 and 1
                if(u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1)
                {
                    isIntersecting = true;
                }
            }

            return isIntersecting;
        }

        private bool AreCornersIntersecting(Triangle t1, Triangle t2)
        {
            bool isIntersecting = false;
            //We only have to test one corner from each triangle

            //Triangle 1 in triangle 2
            if(IsPointInTriangle(t1.p1, t2.p1, t2.p2, t2.p3))
            {
                isIntersecting = true;
            }

            //Triangle 2 in triangle 1
            else if(IsPointInTriangle(t2.p1, t1.p1, t1.p2, t1.p3))
            {
                isIntersecting = true;
            }

            return isIntersecting;
        }

        private bool IsPointInTriangle(Vector2 p, Vector2 tp1, Vector2 tp2, Vector2 tp3)
        {
            bool isWithinTriangle = false;
            float denominator = ((tp2.y - tp3.y) * (tp1.x - tp3.x) + (tp3.x - tp2.x) * (tp1.y - tp3.y));
            float a = ((tp2.y - tp3.y) * (p.x - tp3.x) + (tp3.x - tp2.x) * (p.y - tp3.y)) / denominator;
            float b = ((tp3.y - tp1.y) * (p.x - tp3.x) + (tp1.x - tp3.x) * (p.y - tp3.y)) / denominator;
            float c = 1 - a - b;

            //The point is within the triangle if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
            if(a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f)
            {
                isWithinTriangle = true;
            }

            return isWithinTriangle;
        }

        private struct Triangle
        {
            public Vector2 p1, p2, p3;
            public LineSegment[] lineSegments;

            public Triangle(Vector2 p1, Vector2 p2, Vector2 p3)
            {
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
                lineSegments = new LineSegment[3];
                lineSegments[0] = new LineSegment(p1, p2);
                lineSegments[1] = new LineSegment(p2, p3);
                lineSegments[2] = new LineSegment(p3, p1);
            }
        }

        private struct LineSegment
        {
            public Vector2 p1, p2;

            public LineSegment(Vector2 p1, Vector2 p2)
            {
                this.p1 = p1;
                this.p2 = p2;
            }
        }

        public static bool operator ==(OBBox a, OBBox b)
        {
            if(Mathf.Abs(a._rotation - b._rotation) > Numbers.Epsilon)
            {
                return false;
            }

            if(a._rect != b._rect)
            {
                return false;
            }

            if(a._centre != b._centre)
            {
                return false;
            }

            if(a._size != b._size)
            {
                return false;
            }

            return true;
        }

        public static bool operator !=(OBBox a, OBBox b)
        {
            if(Mathf.Abs(a._rotation - b._rotation) < Numbers.Epsilon)
            {
                return false;
            }

            if(a._rect == b._rect)
            {
                return false;
            }

            if(a._centre == b._centre)
            {
                return false;
            }

            if(a._size == b._size)
            {
                return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            return obj.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            return (int)(_rotation * _rect.GetHashCode());
        }

        public static OBBox Fit(Shape shape)
        {
            return Fit(shape.pointList);
        }

        public static OBBox Fit(IList<Vector2> shape)
        {
            int pointCount = shape.Count;
            if (pointCount < 3)
            {
                return new OBBox();
            }

            float smallestArea = float.PositiveInfinity;
            Rect candidateBounds = Rect.zero;
            float candidateAngle = 0;
            
            for (int p = 0; p < pointCount; p++)
            {
                Vector2 p0 = shape[p];
                Vector2 p1 = shape[(p + 1) % pointCount];
                Vector2 dir = (p1 - p0).normalized;
                if (dir.sqrMagnitude < Numbers.Epsilon)
                {
                    continue;//ignore invalid sides
                }

                float angle = Angles.SignAngle(dir);

                Rect bounds = Rect.zero;
                for (int i = 0; i < pointCount; i++)
                {
                    Vector2 rotatedPoint = shape[i].Rotate(angle);
                    if (i == 0)
                    {
                        bounds = new Rect(rotatedPoint, Vector2.zero);
                    }
                    else
                    {
                        bounds.Encapsulate(rotatedPoint);
                    }
                }

                float area = bounds.Area();
                if (area < smallestArea)
                {
                    smallestArea = area;
                    candidateBounds = bounds;
                    candidateAngle = angle;
                }
            }

            candidateBounds.center = candidateBounds.center.Rotate(-candidateAngle);
            return new OBBox(candidateBounds, candidateAngle);
        }
        
        public static OBBox FitRotatingCalipers(IList<Vector2> shape, int angleIntervalMaximum = 5)
        {
            int pointCount = shape.Count;
            if (pointCount < 3)
            {
                return new OBBox();
            }

            float smallestArea = float.PositiveInfinity;
            Rect candidateBounds = Rect.zero;
            float candidateAngle = 0;

            int iterationCount = Mathf.FloorToInt(360f / angleIntervalMaximum);
            float angleInterval = 360f / iterationCount;
            
            for (int p = 0; p < iterationCount; p++)
            {
                float angle = angleInterval * p;
                Rect bounds = Rect.zero;
                for (int i = 0; i < pointCount; i++)
                {
                    Vector2 rotatedPoint = shape[i].Rotate(angle);
                    if (i == 0)
                    {
                        bounds = new Rect(rotatedPoint, Vector2.zero);
                    }
                    else
                    {
                        bounds.Encapsulate(rotatedPoint);
                    }
                }

                float area = bounds.Area();
                if (area < smallestArea)
                {
                    smallestArea = area;
                    candidateBounds = bounds;
                    candidateAngle = angle;
                }
            }

            candidateBounds.center = candidateBounds.center.Rotate(-candidateAngle);
            return new OBBox(candidateBounds, candidateAngle);
        }
        
        public static OBBox Fit(IList<Vector2> shape, float angle)
        {
            int pointCount = shape.Count;
            if (pointCount < 3)
            {
                return new OBBox();
            }

            Rect bounds = Rect.zero;
            for (int i = 0; i < pointCount; i++)
            {
                Vector2 rotatedPoint = shape[i].Rotate(angle);
                if (i == 0)
                {
                    bounds = new Rect(rotatedPoint, Vector2.zero);
                }
                else
                {
                    bounds.Encapsulate(rotatedPoint);
                }
            }
            bounds.center = bounds.center.Rotate(-angle);
            return new OBBox(bounds, angle);
        }

        public void DrawDebug(Color col, float time)
        {
            Debug.DrawLine(p0.Vector3Flat(), p1.Vector3Flat(), col, time);
            Debug.DrawLine(p1.Vector3Flat(), p2.Vector3Flat(), col, time);
            Debug.DrawLine(p2.Vector3Flat(), p3.Vector3Flat(), col, time);
            Debug.DrawLine(p0.Vector3Flat(), p3.Vector3Flat(), col, time);

            // Debug.DrawLine(_centre.Vector3Flat(), (_centre + _shortDir * shortSize * 0.5f).Vector3Flat(), col, time);
            // Debug.DrawLine(_centre.Vector3Flat(), (_centre - _shortDir * shortSize * 0.5f).Vector3Flat(), col, time);
            // Debug.DrawLine(_centre.Vector3Flat(), (_centre + _longDir * longSize * 0.5f).Vector3Flat(), col, time);
            // Debug.DrawLine(_centre.Vector3Flat(), (_centre - _longDir * longSize * 0.5f).Vector3Flat(), col, time);
        }

    }
}