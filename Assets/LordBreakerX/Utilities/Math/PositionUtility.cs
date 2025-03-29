using UnityEngine;

namespace LordBreakerX.Utilities.Math
{
    public static class PositionUtility
    {
        /// <summary>
        /// Generates a random position within a sphere of a specified radius around a starting point.
        /// </summary>
        /// <param name="radius">The radius of the sphere.</param>
        /// <param name="origin">The center point from which the random position is generated.</param>
        /// <returns>A random position within the sphere.</returns>
        public static Vector3 GetRandomPositionInSphere(float radius, Vector3 origin)
        {
            float offsetX = Random.Range(-radius, radius);
            float offsetY = Random.Range(-radius, radius);
            float offsetZ = Random.Range(-radius, radius);

            return origin + new Vector3(offsetX, offsetY, offsetZ);
        }

        /// <summary>
        /// Generates a random position within a disc of a specified radius, oriented by forward and side directions.
        /// </summary>
        /// <param name="radius">The radius of the disc.</param>
        /// <param name="origin">The center point of the disc.</param>
        /// <param name="forward">The forward direction vector of the disc.</param>
        /// <param name="right">The right (side) direction vector of the disc.</param>
        /// <returns>A random position within the disc.</returns>
        public static Vector3 GetRandomPositionInDisc(float radius, Vector3 origin, Vector3 forward, Vector3 right)
        {
            float forwardOffset = Random.Range(-radius, radius);
            float rightOffset = Random.Range(-radius, radius);

            Vector3 offset = (forward * forwardOffset) + (right * rightOffset);
            return origin + offset;
        }

    }
}
