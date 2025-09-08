using UnityEngine;

namespace LordBreakerX.Utilities.Math
{
    public static class Percentage
    {
        private const float MIN_PERCENTAGE = 0.0f;
        private const float MAX_PERCENTAGE = 100.0f;

        private const float MIN_NORMALIZED_VALUE = 0.0f;
        private const float MAX_NORMALIZED_VALUE = 1.0f;

        #region Getting Percentages

        public static float Normalize(float value, float minValue, float maxValue)
        {
            float clampedValue = Mathf.Clamp(value, minValue, maxValue);
            float normalizedPercentage = (clampedValue - minValue) / (maxValue - minValue);
            return Mathf.Clamp(normalizedPercentage, MIN_NORMALIZED_VALUE, MAX_NORMALIZED_VALUE);
        }

        public static float Normalize(float percentage)
        {
            return Mathf.Clamp(percentage / 100, MIN_NORMALIZED_VALUE, MAX_NORMALIZED_VALUE);
        }

        public static float Create(float value, float minValue, float maxValue)
        {
            return Normalize(value, minValue, maxValue) * MAX_PERCENTAGE;
        }

        #endregion

        #region Reverse Percentages 

        public static float Reverse(float value, float minValue, float maxValue)
        {
            return MAX_PERCENTAGE - Normalize(value, minValue, maxValue) * MAX_PERCENTAGE;
        }

        public static float Reverse(float percentage)
        {
            return MAX_PERCENTAGE - percentage;
        }

        #endregion

        #region Mapping Percentages

        public static float MapToFloat(float percentage, float minValue, float maxValue)
        {
            float clampedPercentage = Mathf.Clamp(percentage, MIN_PERCENTAGE, MAX_PERCENTAGE);
            return minValue + (maxValue - minValue) * (clampedPercentage / 100);
        }

        public static Vector2 MapToVectorTwo(float percentage, Vector2 minValue, Vector2 maxValue)
        {
            float x = MapToFloat(percentage, minValue.x, maxValue.x);
            float y = MapToFloat(percentage, minValue.y, maxValue.y);
            return new Vector2(x, y);
        }

        public static Vector3 MapToVectorThree(float percentage, Vector3 minValue, Vector3 maxValue)
        {
            Vector2 xyValues = MapToVectorTwo(percentage, (Vector2)minValue, (Vector2)maxValue);
            float z = MapToFloat(percentage, minValue.z, maxValue.z);
            return new Vector3(xyValues.x, xyValues.y, z);
        }

        public static Vector4 MapToVectorFour(float percentage, Vector4 minValue, Vector4 maxValue)
        {
            Vector3 xyzValues = MapToVectorThree(percentage, (Vector3)minValue, (Vector3)maxValue);
            float w = MapToFloat(percentage, minValue.w, maxValue.w);
            return new Vector4(xyzValues.x, xyzValues.y, xyzValues.z, w);
        }

        public static Color  MapToColor(float percentage, Color minValue, Color maxValue)
        {
            float red = MapToFloat(percentage, minValue.r, maxValue.r);
            float green = MapToFloat(percentage, minValue.g, maxValue.g);
            float blue = MapToFloat(percentage, minValue.b, maxValue.b);
            float alpha = MapToFloat(percentage, minValue.a, maxValue.a);
            return new Color(red, green, blue, alpha);
        }

        public static Color MapToColor(float percentage, Gradient colorGradient)
        {
            float normalizedPercentage = Normalize(percentage);
            return colorGradient.Evaluate(normalizedPercentage);
        }
        #endregion
    }
}
