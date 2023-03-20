﻿using System;
using opengen.maths;
using UnityEngine;

[Serializable]
public class Bitmask
{
    private int _value;

    public bool this[int index]
    {
        get {return (_value & 1 << index) > 0;}
        set
        {
            if(value)
            {
                _value = _value | 1 << index;
            }
            else
            {
                _value &= ~(1 << index);
            }
        }
    }

    public int value
    {
        get { return _value; }
        set {_value = value;}
    }

    public void Clear()
    {
        _value = 0;
    }

    public int Max()
    {
        return Numbers.CeilToInt(Mathf.Sqrt(_value));
    }

    public int FirstFalse()
    {
        int max = Max();
        for(int i = 0; i < max; i++)
        {
            if((_value & 1 << i) == 0)
            {
                return i;
            }
        }
        return max;
    }

    public new string ToString()
    {
        string output = "";
        int size = Max();
        for(int i = 0; i < size; i++)
        {
            output = output + ((_value & 1 << i) == 0 ? "0" : "1");
        }

        return output;
    }
}