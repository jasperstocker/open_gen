using System;
using opengen.types;
using UnityEngine;

namespace opengen.maths.spines
{
    [Serializable]
    public class SplinePoint
    {
        protected Vector3 _position;
        protected Quaternion _rotation;
        protected float _distance;//to the next point

        public SplinePoint(Vector3 position, Quaternion rotation)
        {
            Initialise(position, rotation);
        }

        public SplinePoint(Vector3 position, Vector3 rotation)
        {
            Initialise(position, Quaternion.Euler(rotation));
        }

        public SplinePoint(Vector3 position)
        {
            Initialise(position, Quaternion.identity);
        }

        protected virtual void Initialise(Vector3 position, Quaternion rotation)
        {
            _position = position;
            _rotation = rotation;
        }

        public Vector3 position
        {
            get => _position;
            set => _position = value;
        }

        public Quaternion rotation
        {
            get => _rotation;
            set => _rotation = value;
        }

        public float distance
        {
            get => _distance;
            set => _distance = value;
        }
    }
}