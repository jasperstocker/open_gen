using System;
using System.Collections.Generic;
using UnityEngine;

namespace opengen.maths.spines
{
    [Serializable]
    public abstract class Spline : ISpline
    {
        private List<SplinePoint> _points = new();
        private float _length = 0;
        private int _pointCount = 0;
        private bool _isLoop = false;
        private bool _normalise = false;

        public SplinePoint GetPoint(int index)
        {
            return _points[index];
        }

        public void AddPoint(SplinePoint point)
        {
            _points.Add(point);
        }
        
        public Vector3 Position(float t)
        {
            t = Numbers.Clamp01(t);
            return CalculatePosition(t);
        }

        public int PointCount => _pointCount;

        public float Length => _length;

        public bool Normalise
        {
            get => _normalise;
            set => _normalise = value;
        }

        protected virtual Vector3 CalculatePosition(float t)
        {
            int[] pointIndices = CalculateSplinePointIndices(t);
            float pointT = CalculateInterPointT(t);
            Vector3 p0 = _points[pointIndices[0]].position;
            Vector3 p1 = _points[pointIndices[1]].position;
            Vector3 output = Vector3.Lerp(p0, p1, pointT);
            return output;
        }

        protected virtual int[] CalculateSplinePointIndices(float t)
        {
            int baseIndex = _normalise ? CalculateNormalizedBaseIndex(t) : CalculateBaseIndex(t);

            int[] output = new int[2];
            output[0] = baseIndex;
            output[1] = baseIndex < _pointCount - 1 ? baseIndex + 1 : (_isLoop ? 0 : baseIndex);
            return output;
            // int index0 = _isLoop ? (baseIndex > 0 ? baseIndex - 1 : _pointCount - 1) : Mathf.Max(0, baseIndex - 1);
            // int index
        }

        protected virtual int CalculateBaseIndex(float t)
        {
            return Numbers.FloorToInt(_pointCount * t);
        }

        protected virtual int CalculateNormalizedBaseIndex(float t)
        {
            float processDistance = _length * t;
            for (int i = 0; i < _pointCount; i++)
            {
                float pointDistance = _points[i].distance;
                if (processDistance < pointDistance)
                {
                    return i;
                }

                processDistance += -pointDistance;
            }

            return _pointCount - 1;
        }

        protected virtual float CalculateInterPointT(float t)
        {
            if (_normalise)
            {
                float processDistance = _length * t;
                for (int i = 0; i < _pointCount; i++)
                {
                    float pointDistance = _points[i].distance;
                    if (processDistance < pointDistance)
                    {
                        return processDistance / pointDistance;
                    }

                    processDistance += -pointDistance;
                }

                return 1;
            }
            else
            {
                float interTSpacing = 1.0f / _pointCount;
                int baseIndex = CalculateBaseIndex(t);
                float baseT = interTSpacing * baseIndex;
                return t - baseT;
            }
        }

        protected virtual void CalculateInternals()
        {
            _pointCount = _points.Count;
            _length = 0;
            for (int i = 0; i < _pointCount; i++)
            {
                int ib = i < _pointCount - 1 ? i + 1 : 0;
                Vector3 p0 = _points[i].position;
                Vector3 p1 = _points[ib].position;
                float distance = Vector3.Distance(p0, p1);
                _points[i].distance = distance;
                if(i < _pointCount - 1 || _isLoop)
                {
                    _length += distance;
                }
            }
        }
    }
}