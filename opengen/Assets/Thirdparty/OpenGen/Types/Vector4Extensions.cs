using UnityEngine;

namespace opengen.types
{
    public class Vector4Extensions
    {
        /// <summary>
        /// Calculate the Tangent from a direction
        /// </summary>
        /// <param name="tangentDirection">the normalized right direction of the tangent</param>
        public static Vector4 CalculateTangent(Vector3 tangentDirection)
        {
            Vector4 tangent = new Vector4();
            tangent.x = tangentDirection.x;
            tangent.y = tangentDirection.y;
            tangent.z = tangentDirection.z;
            tangent.w = 1;//TODO: Check whether we need to flip the bi normal - I don't think we do with these planes
            return tangent;
        }

        public static Vector4 RotateTangent(Vector4 tangent, Quaternion rotation)
        {
            Vector3 tangentDirection = new Vector3(tangent.x, tangent.y, tangent.z);
            tangentDirection = rotation * tangentDirection;
            tangent.x = tangentDirection.x;
            tangent.y = tangentDirection.y;
            tangent.z = tangentDirection.z;
            return tangent;
        }
    }
}