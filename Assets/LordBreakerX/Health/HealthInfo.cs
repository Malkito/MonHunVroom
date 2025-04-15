using UnityEngine;

namespace LordBreakerX.Health
{
    public struct HealthInfo
    {
        public float Maxhealth { get; private set; }
        public float CurrentHealth { get; private set; }
        public float DamageCaused { get; private set; }
        public float HealthRestored { get; private set; }

        public GameObject Source { get; private set; }

        public HealthInfo(float maxHealth, float currentHealth, float damageCaused, float healthRestored, GameObject source)
        {
            Maxhealth = maxHealth;
            CurrentHealth = currentHealth;
            DamageCaused = damageCaused;
            HealthRestored = healthRestored;
            Source = source;
        }
    }
}
