using Unity.VisualScripting;
using UnityEngine;

namespace LordBreakerX.Utilities
{
    public static class VectorUtility
    {
        public static Vector3 Multiply(this Vector3 originalVector, Vector3 multiplyVector)
        {
            return new Vector3(originalVector.x * multiplyVector.x, originalVector.y * multiplyVector.y, originalVector.z * multiplyVector.z);
        }

        public static Vector3 Devide(this Vector3 originalVector, Vector3 devideVector) 
        {
            return new Vector3(originalVector.x / devideVector.x, originalVector.y / devideVector.y, devideVector.z / devideVector.z);
        }

        public static Vector3 Devide(this Vector3 originalVector, float x, float y, float z)
        {
            return new Vector3(originalVector.x / x, originalVector.y / y, originalVector.z / z);
        }

        public static Vector3 Devide(this Vector3 originalVector, float x, float y)
        {
            return new Vector3(originalVector.x / x, originalVector.y / y, originalVector.z);
        }

        public static Vector3 Devide(this Vector3 originalVector, float x)
        {
            return new Vector3(originalVector.x / x, originalVector.y, originalVector.z);
        }
    }
}
