using System;
using System.Collections.Generic;
using opengen.types;
using UnityEngine;
using Random = System.Random;

namespace opengen.maths
{
    [Serializable]
    public class RandomGenerator
    {
        [SerializeField]
        private uint _seed;
        [SerializeField]
        private uint _mZ;
        [SerializeField]
        private uint _mW;
        
        public RandomGenerator(uint newSeed = 1)
        {
            if(newSeed == 0)
            {
                newSeed = 1;
            }

            _seed = newSeed;
            _mZ = _seed;
            _mW = _seed;
        }
        
        public RandomGenerator()
        {
            _seed = GenerateSeed();
            if(_seed == 0)
            {
                _seed = 1;
            }

            _mZ = _seed;
            _mW = _seed;
        }

        public void Reset()
        {
            Debug.Log("reset");
            _mZ = _mW = _seed;
        }

        public float output
        {
            get
            {
                _mZ = 36969 * (_mZ & 65535) + (_mZ >> 16);
                _mW = 18000 * (_mW & 65535) + (_mW >> 16);
                uint u = (_mZ << 16) + _mW;
                float val = (u + 1.0f) * 2.328306435454494e-10f;
                return val;
            }
        }

        public float OneRange()
        {
            return Range(-1f, 1f);
        }

        public float OutputRange(float min, float max)
        {
            return min + output * (max - min);
        }

        public float Range(float min, float max)
        {
            return OutputRange(min, max);
        }

        public int OutputRange(int min, int max)
        {
            return min + Mathf.RoundToInt(output * (max - min));
        }

        public int Range(int min, int max)
        {
            return OutputRange(min, max);
        }

        public int Index(int length)
        {
            if (length == 0)
            {
                return -1;
            }

            if (length == 1)
            {
                return 0;
            }

            float val = output * (length - Mathf.Epsilon);
            return Mathf.FloorToInt(val);
        }

        public Vector2 Position(Rect bounds)
        {
            Vector2 pos = new();
            pos.x = bounds.size.x * output + bounds.xMin;
            pos.y = bounds.size.y * output + bounds.yMin;
            return pos;
        }

        public bool outputBool
        {
            get
            {
                return output < 0.5f;
            }
        }

        public bool OutputBool(float chance)
        {
            return output < chance;
        }

        public uint generateSeed
        {
            get
            {
                return (uint)(output * uint.MaxValue);
            }
        }

        public uint seed
        {
            get { return _seed; }

            set
            {
                if(value == _seed)
                {
                    return;
                }

                if(value == 0)
                {
                    value = 1;
                }

                _seed = value;
                Reset();
            }
        }

        public void GenerateNewSeed()
        {
            seed = GenerateSeed();//use thread safe System.Random
            Reset();
        }

        public static uint GenerateSeed()
        {
            Random rdn = new();
            return (uint)rdn.Next(1, 9999999);
        }


        public float VariableLength(VariableLength variableLength)
        {
            if(variableLength.fixValue)
            {
                return variableLength.fix;
            }

            if (variableLength.curve == null || variableLength.curve.length == 0)
            {
                float outpt = Range(variableLength.min, variableLength.max);
                return outpt;
            }
            float[] probs = new float[100];
            float total = 0;
            for (int i = 0; i < 100; i++)
            {
                float prob = variableLength.curve.Evaluate(i / 99f);
                probs[i] = prob;
                total += prob;
            }
            float randomPoint = output * total;
            for (int i = 0; i < 100; i++)
            {
                if (randomPoint < probs[i])
                {
                    return variableLength.min + (variableLength.max - variableLength.min) * (i / 100f);
                }

                randomPoint -= probs[i];
            }
            return 1f;
        }

        public float Range(AnimationCurve curve)
        {
            float[] probs = new float[100];
            float total = 0;
            for (int i = 0; i < 100; i++)
            {
                float prob = curve.Evaluate(i / 99f);
                probs[i] = prob;
                total += prob;
            }
            float randomPoint = output * total;

            for (int i = 0; i < 100; i++)
            {
                if (randomPoint < probs[i])
                {
                    return i / 100f;
                }

                randomPoint -= probs[i];
            }
            return 1f;
        }

        public bool OddsBool(float chance)
        {
            return output < chance;
        }

        public int ArrayItem(float[] odds)
        {
            int oddLength = odds.Length;
            float oddsTotal = 0;
            for (int i = 0; i < oddLength; i++)
            {
                oddsTotal += odds[i];
            }

            if (oddsTotal < Mathf.Epsilon)
            {
                return Index(oddLength);//no odds assigned so just randomise
            }

            float randomPoint = output * oddsTotal;
            for (int i = 0; i < oddLength; i++)
            {
                if (randomPoint < odds[i])
                {
                    return i;
                }

                randomPoint -= odds[i];
            }
            return oddLength - 1;
        }

        public int ArrayItem(List<float> odds)
        {
            int oddLength = odds.Count;
            float oddsTotal = 0;
            for (int i = 0; i < oddLength; i++)
            {
                oddsTotal += odds[i];
            }

            if (oddsTotal < Mathf.Epsilon)
            {
                return Index(oddLength);//no odds assigned so just randomise
            }

            float randomPoint = output * oddsTotal;
            for (int i = 0; i < oddLength; i++)
            {
                if (randomPoint < odds[i])
                {
                    return i;
                }

                randomPoint -= odds[i];
            }
            return oddLength - 1;
        }

        public int[] Indexes(int length, int amount)
        {
            bool[] used = new bool[length];

            int loop = length > amount ? amount : length;
            int[] indexes = new int[loop];
            for (int x = 0; x < loop; x++)
            {
                int rIndex = Index(length);
                int it = length;
                while (true)
                {
                    if (!used[rIndex])
                    {
                        used[rIndex] = true;
                        indexes[x] = rIndex;
                        break;
                    }
                    rIndex++;
                    if (rIndex == length)
                    {
                        rIndex = 0;
                    }

                    it--;
                    if (it < 0)
                    {
                        break;
                    }
                }
            }

            return indexes;
        }
        
        public float SampleGaussian(float mean, float stddev)
        {
            // The method requires sampling from a uniform random of (0,1]
            // but Random.NextDouble() returns a sample of [0,1).
            float x1 = 1 - output;
            float x2 = 1 - output;

            float y1 = Mathf.Sqrt(-2.0f * Mathf.Log(x1)) * Mathf.Cos(2.0f * Mathf.PI * x2);
            return y1 * stddev + mean;
        }
    }
}