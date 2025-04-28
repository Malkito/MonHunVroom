using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace LordBreakerX.Health
{
    /// <summary>
    /// Represents an object that has health and can take damage or be healed. 
    /// Invokes events when the health changes or when the object dies.
    /// </summary>
    public class MonsterHealth : MonoBehaviour, dealDamage
    {
        [Min(1)]
        [SerializeField]
        [Tooltip("The maximum and starting amount of health for the object.")]
        private float _maxHealth = 1;

        [SerializeField]
        [Header("Events")]
        [Tooltip("Invoked whenever the health changes.")]
        private UnityEvent<HealthInfo> _onHealthChanged = new UnityEvent<HealthInfo>();

        [SerializeField]
        [Tooltip("Invoked when the health reaches zero or below.")]
        private UnityEvent _onDeath = new UnityEvent();

        private float _currentHealth;

        private void Awake()
        {
            _currentHealth = _maxHealth;
            HealthInfo healthInfo = new HealthInfo(_maxHealth, _currentHealth, 0, 0, null);
            _onHealthChanged.Invoke(healthInfo);
        }

        public void TestDealDamage(GameObject damageSouce)
        {
            dealDamage(1000, Color.red, damageSouce);
        }

        public void dealDamage(float damageDealt, Color flashColor, GameObject damageOrigin)
        {
            float clampedAmount = Mathf.Clamp(damageDealt, 0, _currentHealth);

            _currentHealth -= clampedAmount;

            if (clampedAmount > 0)
            {
                HealthInfo healthInfo = new HealthInfo(_maxHealth, _currentHealth, clampedAmount, 0, damageOrigin);
                _onHealthChanged.Invoke(healthInfo);
            }

            if (_currentHealth <= 0)
            {
                _onDeath.Invoke();
            }
        }

        public void increaseFireNumber()
        {
            
        }

        public void decreaseFireNumber()
        {
            
        }
    }
}