using UnityEngine;

namespace LordBreakerX.Utilities.Math
{
    public static class PositionUtility
    {
        /// <summary>
        /// Generates a random position within a cube of a specified size centered around a given point.
        /// </summary>
        /// <param name="halfSize">The half-size of the cube along each axis.</param>
        /// <param name="origin">The center point of the cube.</param>
        /// <returns>A random position within the cube.</returns>
        public static Vector3 GetRandomPositionInCube(float halfSize, Vector3 origin)
        {
            float offsetX = Random.Range(-halfSize, halfSize);
            float offsetY = Random.Range(-halfSize, halfSize);
            float offsetZ = Random.Range(-halfSize, halfSize);

            return origin + new Vector3(offsetX, offsetY, offsetZ);
        }

        /// <summary>
        /// Generates a random position within a square of a specified size, oriented using given direction vectors.
        /// </summary>
        /// <param name="halfSize">The half-size of the square along each direction.</param>
        /// <param name="origin">The center point of the square.</param>
        /// <param name="forward">The forward direction vector of the square.</param>
        /// <param name="right">The right (side) direction vector of the square.</param>
        /// <returns>A random position within the square.</returns>
        public static Vector3 GetRandomPositionInSquare(float halfSize, Vector3 origin, Vector3 forward, Vector3 right)
        {
            float forwardOffset = Random.Range(-halfSize, halfSize);
            float rightOffset = Random.Range(-halfSize, halfSize);

            Vector3 offset = (forward * forwardOffset) + (right * rightOffset);
            return origin + offset;
        }

        public static Vector3 GetRandomPositionInSquare(float halfSize, Vector3 origin)
        {
            return GetRandomPositionInSquare(halfSize, origin, Vector3.forward, Vector3.right);
        }



        /// <summary>
        /// Generates a random position within a half-square area in front of the given origin.
        /// </summary>
        /// <param name="halfSize">The maximum distance from the origin along the forward and right directions.</param>
        /// <param name="origin">The starting position.</param>
        /// <param name="forward">The forward direction defining the half-square.</param>
        /// <param name="right">The right direction defining the width of the half-square.</param>
        /// <returns>A random position within the defined half-square area.</returns>
        public static Vector3 GetRandomPositionInFrontHalfSquare(float halfSize, Vector3 origin, Vector3 forward, Vector3 right)
        {
            float forwardOffset = Random.Range(0, halfSize);
            float rightOffset = Random.Range(-halfSize, halfSize);

            Vector3 offset = (forward * forwardOffset) + (right * rightOffset);
            return origin + offset;
        }

        /// <summary>
        /// Generates a random position within a half-square area in front of the given origin,
        /// assuming the default forward direction as (0, 0, 1) and right direction as (1, 0, 0).
        /// </summary>
        /// <param name="halfSize">The maximum distance from the origin along the forward and right directions.</param>
        /// <param name="origin">The starting position.</param>
        /// <returns>A random position within the half-square area.</returns>
        public static Vector3 GetRandomPositionInFrontHalfSquare(float halfSize, Vector3 origin)
        {
            return GetRandomPositionInFrontHalfSquare(halfSize, origin, Vector3.forward, Vector3.right);
        }

    }
}
