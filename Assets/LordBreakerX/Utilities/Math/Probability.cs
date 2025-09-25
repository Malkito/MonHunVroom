using UnityEngine;

namespace LordBreakerX.Utilities
{
    public static class Probability
    {
        public delegate void RollSuccessCallback();

        public const float MIN_CHANCE = 0.0f;
        public const float MAX_CHANCE = 100.0f;

        public static bool IsSuccessful(float chance)
        {
            float clampedChance = Mathf.Clamp(chance, MIN_CHANCE, MAX_CHANCE);
            return Random.Range(MIN_CHANCE, MAX_CHANCE) <= clampedChance;
        }

        public static void PerformChanceRolls(int amount, float chance, RollSuccessCallback onSuccess)
        {
            for (int i = 0; i < amount; i++) 
            {
                if (IsSuccessful(chance))
                {
                    onSuccess.Invoke();
                }
            }
        }

    }
}
