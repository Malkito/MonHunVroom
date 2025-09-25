using UnityEngine;

namespace LordBreakerX.Utilities
{
    public static class VectorMath
    {
        /// <summary>
        /// Performs component-wise multiplication between two <see cref="Vector2"/> values.
        /// </summary>
        /// <param name="multiplicand">The first vector.</param>
        /// <param name="multiplier">The second vector.</param>
        /// <returns>
        /// A new <see cref="Vector2"/> containing the result of multiplying
        /// each component of <paramref name="multiplicand"/> by the corresponding
        /// component of <paramref name="multiplier"/>.
        /// </returns>
        public static Vector2 Multiply(this Vector2 multiplicand, Vector2 multiplier)
        {
            return Multiply((Vector3)multiplicand, (Vector3)multiplier);
        }

        /// <summary>
        /// Performs component-wise multiplication between two <see cref="Vector3"/> values.
        /// </summary>
        /// <param name="multiplicand">The first vector.</param>
        /// <param name="multiplier">The second vector.</param>
        /// <returns>
        /// A new <see cref="Vector3"/> containing the result of multiplying
        /// each component of <paramref name="multiplicand"/> by the corresponding
        /// component of <paramref name="multiplier"/>.
        /// </returns>
        public static Vector3 Multiply(this Vector3 multiplicand, Vector3 multiplier)
        {
            float x = multiplicand.x * multiplier.x;
            float y = multiplicand.y * multiplier.y;
            float z = multiplicand.z * multiplier.z;
            return new Vector4(x, y, z);
        }

        public static Vector3 MultiplyVectors(Vector3 multplicand, params Vector3[] multipliers)
        {
            Vector3 current = multplicand;

            foreach (Vector3 factor in multipliers) 
            {
                current = Multiply(current, factor);
            }

            return current;
        }

        /// <summary>
        /// Performs component-wise division between two <see cref="Vector2"/> values.
        /// </summary>
        /// <param name="dividend">The vector whose components will be divided.</param>
        /// <param name="divisor">The vector whose components will be used as divisors.</param>
        /// <returns>
        /// A new <see cref="Vector2"/> containing the result of dividing
        /// each component of <paramref name="dividend"/> by the corresponding
        /// component of <paramref name="divisor"/>.
        /// </returns>
        /// <remarks>
        /// If a component of <paramref name="divisor"/> is zero, a warning will be logged
        /// and the corresponding component from <paramref name="dividend"/> will be returned
        /// unchanged instead of throwing an exception.
        /// </remarks>
        public static Vector2 Devide(this Vector2 dividend, Vector2 divisor)
        {
            float x = Devide(dividend.x, divisor.x);
            float y = Devide(dividend.y, divisor.y);
            return new Vector2(x, y);
        }

        /// <summary>
        /// Performs component-wise division between two <see cref="Vector3"/> values.
        /// </summary>
        /// <param name="dividend">The vector whose components will be divided.</param>
        /// <param name="divisor">The vector whose components will be used as divisors.</param>
        /// <returns>
        /// A new <see cref="Vector3"/> containing the result of dividing
        /// each component of <paramref name="dividend"/> by the corresponding
        /// component of <paramref name="divisor"/>.
        /// </returns>
        /// <remarks>
        /// If a component of <paramref name="divisor"/> is zero, a warning will be logged
        /// and the corresponding component from <paramref name="dividend"/> will be returned
        /// unchanged instead of throwing an exception.
        /// </remarks>
        public static Vector3 Devide(this Vector3 dividend, Vector3 divisor)
        {
            float x = Devide(dividend.x, divisor.x);
            float y = Devide(dividend.y, divisor.y);
            float z = Devide(dividend.z, divisor.z);
            return new Vector3(x, y, z);
        }

        private static float Devide(float dividend, float divisor) 
        {
            if (divisor == 0)
            {
                Debug.LogWarning($"Tried to devide {dividend} by divisor of zero!");
                return dividend;
            }

            return dividend / divisor;
        }

    }
}
