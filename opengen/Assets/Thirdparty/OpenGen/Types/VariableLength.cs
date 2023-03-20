using UnityEngine;

namespace opengen.types
{
    [System.Serializable]
    public struct VariableLength
    {
        public float min;
        public float max;
        public float fix;
        public bool fixValue;//toggle to turn variable into a single value
        public AnimationCurve curve;

        //used in editor slider
        [HideInInspector]
        public float eMin;
        [HideInInspector]
        public float eMax;

        public VariableLength(float min, float max, AnimationCurve curve, float eMin = 0, float eMax = 1) : this()
        {
            this.min = min;
            this.max = max;
            this.curve = curve;
            this.eMin = Mathf.Min(eMin, min);
            this.eMax = Mathf.Max(eMax, max);
        }

        public VariableLength(float min, float max, Vector2[] curve, float eMin = 0, float eMax = 1) : this()
        {
            this.min = min;
            this.max = max;
            this.curve = GenerateCurve(curve);
            this.eMin = Mathf.Min(eMin, min);
            this.eMax = Mathf.Max(eMax, max);
        }

        public VariableLength(float min, float max, float eMin = 0, float eMax = 1) : this()
        {
            this.min = min;
            this.max = max;
            curve = GenerateFlatCurve();
            this.eMin = Mathf.Min(eMin, min);
            this.eMax = Mathf.Max(eMax, max);
        }

        public VariableLength(float fix) : this()
        {
            fixValue = true;
            this.fix = fix;
        }

        /// <param name="values">
        /// X is value
        /// Y is chance (0-1)
        /// </param>
        public void SetCurve(Vector2[] values)
        {
            curve = GenerateCurve(values);
        }

        /// <param name="values">
        /// X is value
        /// Y is chance (0-1)
        /// </param>
        public AnimationCurve GenerateCurve(Vector2[] values)
        {
            int valueCount = values.Length;
            if (valueCount == 0)
            {
                Keyframe[] flatKfs = new Keyframe[2];
                flatKfs[0] = new Keyframe(min, 1);
                flatKfs[1] = new Keyframe(max, 1);
                return new AnimationCurve(flatKfs);
            }
            Keyframe[] kfs = new Keyframe[valueCount];
            for (int v = 0; v < valueCount; v++)
            {
                kfs[v] = new Keyframe(values[v].x, values[v].y);
            }

            return new AnimationCurve(kfs);
        }

        public AnimationCurve GenerateFlatCurve()
        {
            Keyframe[] flatKfs = new Keyframe[2];
            flatKfs[0] = new Keyframe(0, 1);
            flatKfs[1] = new Keyframe(1, 1);
            return new AnimationCurve(flatKfs);
        }

        public override string ToString()
        {
            return $"min: {min} - max: {max} - fix: {fix}fixValue: {fixValue}";
        }

        /// <summary>
        /// Is the variable value blank - all zeroes
        /// </summary>
        public bool isBlank()
        {
            if (fixValue)
            {
                return false;
            }

            if (Mathf.Abs(min) > Mathf.Epsilon)
            {
                return false;
            }

            if (Mathf.Abs(max) > Mathf.Epsilon)
            {
                return false;
            }

            return true;
        }

        public void RunBlankCheck()
        {
            if (isBlank())
            {
                min = 0;
                max = 1;
                fix = 0.5f;
                fixValue = Random.value > 0.5f;
                curve = GenerateFlatCurve();
            }
        }
    }
}