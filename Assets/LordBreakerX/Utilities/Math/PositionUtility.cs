using UnityEngine;

namespace LordBreakerX.Utilities.Math
{
    public static class PositionUtility
    {
        /// <summary>
        /// Generates a random position within a sphere of a specified radius around a starting point.
        /// </summary>
        /// <param name="size">The radius of the sphere.</param>
        /// <param name="origin">The center point from which the random position is generated.</param>
        /// <returns>A random position within the sphere.</returns>
        public static Vector3 GetRandomPositionInCube(float size, Vector3 origin)
        {
            float offsetX = Random.Range(-size, size);
            float offsetY = Random.Range(-size, size);
            float offsetZ = Random.Range(-size, size);

            return origin + new Vector3(offsetX, offsetY, offsetZ);
        }

        /// <summary>
        /// Generates a random position within a disc of a specified radius, oriented by forward and side directions.
        /// </summary>
        /// <param name="size">The radius of the disc.</param>
        /// <param name="origin">The center point of the disc.</param>
        /// <param name="forward">The forward direction vector of the disc.</param>
        /// <param name="right">The right (side) direction vector of the disc.</param>
        /// <returns>A random position within the disc.</returns>
        public static Vector3 GetRandomPositionInSquare(float size, Vector3 origin, Vector3 forward, Vector3 right)
        {
            float forwardOffset = Random.Range(-size, size);
            float rightOffset = Random.Range(-size, size);

            Vector3 offset = (forward * forwardOffset) + (right * rightOffset);
            return origin + offset;
        }

        public static Vector3 GetRandomPositionInSquare(float radius, Vector3 origin)
        {
            return GetRandomPositionInSquare(radius, origin, Vector3.forward, Vector3.right);
        }

        /// <summary>
        /// Generates a random position within a half-square area in front of the given origin.
        /// </summary>
        /// <param name="radius">The maximum distance from the origin along the forward and right directions.</param>
        /// <param name="origin">The starting position.</param>
        /// <param name="forward">The forward direction defining the half-square.</param>
        /// <param name="right">The right direction defining the width of the half-square.</param>
        /// <returns>A random position within the defined half-square area.</returns>
        public static Vector3 GetRandomPositionInFrontHalfSquare(float radius, Vector3 origin, Vector3 forward, Vector3 right)
        {
            float forwardOffset = Random.Range(0, radius);
            float rightOffset = Random.Range(-radius, radius);

            Vector3 offset = (forward * forwardOffset) + (right * rightOffset);
            return origin + offset;
        }

        /// <summary>
        /// Generates a random position within a half-square area in front of the given origin,
        /// assuming the default forward direction as (0, 0, 1) and right direction as (1, 0, 0).
        /// </summary>
        /// <param name="radius">The maximum distance from the origin along the forward and right directions.</param>
        /// <param name="origin">The starting position.</param>
        /// <returns>A random position within the half-square area.</returns>
        public static Vector3 GetRandomPositionInFrontHalfSquare(float radius, Vector3 origin)
        {
            return GetRandomPositionInFrontHalfSquare(radius, origin, Vector3.forward, Vector3.right);
        }
    }
}
