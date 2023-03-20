using System;
using System.Collections.Generic;
using opengen.maths;
using UnityEngine;

namespace opengen.types
{
    [Serializable]
    public struct AABBox
    {
        private Rect _core;
        private bool _initialised;

        public AABBox(float x, float y, float width, float height)
        {
            _core = new Rect(x, y, width, height);
            _initialised = true;
        }

        public AABBox(Vector2 position, Vector2 size)
        {
            _core = new Rect(position.x, position.y, size.x, size.y);
            _initialised = true;
        }

        public AABBox(AABBox copy)
        {
            _core = new Rect(copy.x, copy.y, copy.width, copy.height);
            _initialised = copy.initialised;
        }

        public AABBox(Vector2[] points)
        {
            if(points.Length > 0)
            {
                _core = new Rect(points[0].x, points[0].y, 0, 0);
                _initialised = true;
                int pointSize = points.Length;
                for(int p = 0; p < pointSize; p++)
                {
                    Encapsulate(points[p]);
                }
            }
            else
            {
                _core = new Rect();
                _initialised = false;
            }
        }

        public AABBox(IList<Vector2> points)
        {
            if(points.Count > 0)
            {
                _core = new Rect(points[0].x, points[0].y, 0, 0);
                _initialised = true;
                int pointSize = points.Count;
                for(int p = 0; p < pointSize; p++)
                {
                    Encapsulate(points[p]);
                }
            }
            else
            {
                _core = new Rect();
                _initialised = false;
            }
        }

        public AABBox(Rect copy)
        {
            _core = new Rect(copy.x, copy.y, copy.width, copy.height);
            _initialised = copy.size.x != 0 && copy.size.y != 0;
        }

        public AABBox(Bounds bounds)
        {
            _core = new Rect(bounds.center.x - bounds.extents.x, bounds.center.z - bounds.extents.z, bounds.size.x, bounds.size.z);
            _initialised = bounds.extents.magnitude > Numbers.Epsilon;
        }

        public float x {get {return _core.x;}}
        public float y {get {return _core.y;}}
        public float width {get {return _core.width;}}
        public float height {get {return _core.height;}}
        public Vector2 center {get {return _core.center;}}
        public Rect rect {get {return _core;}}
        public float xMin {get {return _core.xMin;}}
        public float xMax {get {return _core.xMax;}}
        public float yMin {get {return _core.yMin;}}
        public float yMax {get {return _core.yMax;}}
        public Vector2 min {get {return _core.min;}}
        public Vector2 max {get {return _core.max;}}
        public Vector2 size {get {return _core.size;}}

        public bool Contains(float x, float y)
        {
            return Contains(new Vector2(x, y));
        }

        public bool Contains(Vector2 point)
        {
            return _core.Contains(point, true);
        }

        public void Encapsulate(float x, float y)
        {
            Encapsulate(new Vector2(x, y));
        }

        public void Encapsulate(Vector2 point)
        {
            if(_core.Contains(point))
            {
                return;
            }

            if(!_initialised)
            {
                _core.position = point;
                _initialised = true;
                return;
            }

            if(_core.xMin > point.x)
            {
                _core.xMin = point.x;
            }
            else if(_core.xMax < point.x)
            {
                _core.xMax = point.x;
            }

            if(_core.yMin > point.y)
            {
                _core.yMin = point.y;
            }
            else if(_core.yMax < point.y)
            {
                _core.yMax = point.y;
            }
        }

        public void Encapsulate(Vector2[] points)
        {
            int pointSize = points.Length;
            for(int p = 0; p < pointSize; p++)
            {
                Encapsulate(points[p]);
            }
        }

        public void Encapsulate(List<Vector2> points)
        {
            int pointSize = points.Count;
            for(int p = 0; p < pointSize; p++)
            {
                Encapsulate(points[p]);
            }
        }

        public void Encapsulate(Rect rectangle)
        {
            Encapsulate(rectangle.min);
            Encapsulate(rectangle.max);
        }

        public void Encapsulate(AABBox bounds)
        {
            Encapsulate(bounds.min);
            Encapsulate(bounds.max);
        }

        public bool Overlaps(AABBox other, bool allowInverse = false)
        {
            return _core.Overlaps(other.rect, allowInverse);
        }

        public void Expand(float margin)
        {
            _core.min = new Vector2(_core.xMin - margin, _core.yMin - margin);
            _core.size = new Vector2(_core.width + margin * 2f, _core.height + margin * 2f);
        }

        public bool initialised {get {return _initialised;}}

        // public void DebugDraw(Color col, float duration = 1)
        // {
        //     Debug.DrawLine(new Vector3(_core.x, 0, _core.y), new Vector3(_core.x, 0, _core.y + _core.height), col, duration);
        //     Debug.DrawLine(new Vector3(_core.x, 0, _core.y), new Vector3(_core.x + _core.width, 0, _core.y), col, duration);
        //     Debug.DrawLine(new Vector3(_core.x + _core.width, 0, _core.y), new Vector3(_core.x + _core.width, 0, _core.y + _core.height), col, duration);
        //     Debug.DrawLine(new Vector3(_core.x, 0, _core.y + _core.height), new Vector3(_core.x + _core.width, 0, _core.y + _core.height), col, duration);
        // }

        public float Area()
        {
            return _core.width * _core.height;
        }

        public float GetNormalisedX(float pointX)
        {
            return (pointX - min.x) / size.x;
        }

        public float GetNormalisedY(float pointY)
        {
            return (pointY - min.y) / size.y;
        }

        public void Clear()
        {
            _core.size = Vector2.zero;
            _core.position = Vector2.zero;
            _initialised = false;
        }

        public override string ToString()
        {
            return _core.ToString();
        }
        

        public static bool operator ==(AABBox a, AABBox b)
        {
            return a._core == b._core;
        }

        public static bool operator !=(AABBox a, AABBox b)
        {
            return a._core != b._core;
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
            return _core.GetHashCode();
        }

        public void DebugDraw()
        {
            _core.DebugDraw();
        }
    }
}