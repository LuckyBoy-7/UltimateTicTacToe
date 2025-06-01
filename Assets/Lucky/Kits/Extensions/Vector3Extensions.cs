using Lucky.Kits.Utilities;
using UnityEngine;

namespace Lucky.Kits.Extensions
{
    public static class Vector3Extensions
    {
        public static Vector3 WithX(this Vector3 orig, float x)
        {
            orig.x = x;
            return orig;
        }

        public static Vector3 WithY(this Vector3 orig, float y)
        {
            orig.y = y;
            return orig;
        }

        public static Vector3 WithZ(this Vector3 orig, float z)
        {
            orig.z = z;
            return orig;
        }

        public static Vector3 Sign(this Vector3 orig)
        {
            return new Vector3(MathUtils.Sign(orig.x), MathUtils.Sign(orig.y), MathUtils.Sign(orig.z));
        }

        public static void Deconstruct(this Vector3 vector, out float x, out float y, out float z)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }
    }
}