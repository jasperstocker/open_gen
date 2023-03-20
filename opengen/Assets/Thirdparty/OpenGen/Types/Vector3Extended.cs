using UnityEngine;

namespace opengen.types
{
    public static class Vector3Extended
    {
        public static Vector2 Vector2Up(this Vector3 value)
        {
            return new Vector2(value.x, value.y);
        }

        public static Vector2 Vector2Flat(this Vector3 value)
        {
            return new Vector2(value.x, value.z);
        }
        
        public static Vector4 Vector4(this Vector3 value, float w = 0)
        {
            return new Vector4(value.x, value.y, value.z, w);
        }
    }
}