using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace LordBreakerX.Health
{
    /// <summary>
    /// Represents an object that has health and can take damage or be healed. 
    /// Invokes events when the health changes or when the object dies.
    /// </summary>
    public class Damageable : MonoBehaviour
    {
        [Min(1)]
        [SerializeField]
        [Tooltip("The maximum and starting amount of health for the object.")]
        private float _maxHealth = 1;

        [SerializeField]
        [Header("Events")]
        [Tooltip("Invoked whenever the health changes.")]
        private UnityEvent<HealthInfo> _onDamaged = new UnityEvent<HealthInfo>();

        [SerializeField]
        private UnityEvent<HealthInfo> _onHealed = new UnityEvent<HealthInfo>();

        [SerializeField]
        private UnityEvent<HealthInfo> _onHealthChanged = new UnityEvent<HealthInfo>();

        [SerializeField]
        [Tooltip("Invoked when the health reaches zero or below.")]
        private UnityEvent _onDeath = new UnityEvent();

        private float _currentHealth;


        public UnityEvent<HealthInfo> OnDamaged { get { return _onDamaged; } }


        public UnityEvent<HealthInfo> OnHealed { get { return _onHealed; } }


        public UnityEvent<HealthInfo> OnHealthChanged { get { return _onHealthChanged; } }


        public UnityEvent OnDeath { get { return _onDeath; } }

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        public void Damage(float amount, GameObject damageSource = null)
        {
            float clampedAmount = Mathf.Clamp(amount, 0, _currentHealth);

            _currentHealth -= clampedAmount;

            if (clampedAmount > 0)
            {
                HealthInfo healthInfo = new HealthInfo(_maxHealth, _currentHealth, clampedAmount, 0, damageSource);
                OnDamaged.Invoke(healthInfo);
                OnHealthChanged.Invoke(healthInfo);
            }

            if (_currentHealth <= 0) OnDeath.Invoke();
        }

        public void StartDamageOverTime(float damagePerTick, float tickInterval, float duration, GameObject damageSource = null)
        {
            StartCoroutine(DamageOverTime(damagePerTick, tickInterval, duration, damageSource));
        }

        private IEnumerator DamageOverTime(float damagePerTick, float tickInterval, float duration, GameObject damageSource = null) 
        {
            float remainingTime = duration;
            WaitForSeconds waitBetweenTicks = new WaitForSeconds(tickInterval);

            while (remainingTime > 0)
            {
                Damage(damagePerTick, damageSource);
                yield return waitBetweenTicks;
                remainingTime -= tickInterval;
            }
        }

        public void Restore(float amount, GameObject healSource = null)
        {
            float clampedAmount = Mathf.Clamp(amount, 0, _maxHealth - _currentHealth);

            if (clampedAmount > 0)
            {
                _currentHealth += clampedAmount;

                HealthInfo healthInfo = new HealthInfo(_maxHealth, _currentHealth, 0, clampedAmount, healSource);
                OnHealed.Invoke(healthInfo);
                OnHealthChanged.Invoke(healthInfo);
            }
        }

        public void StartRestoreOverTime(float healthPerTick, float tickInterval, float duration, GameObject healSource = null)
        {
            StartCoroutine(RestoreOverTime(healthPerTick, tickInterval, duration, healSource));
        }

        private IEnumerator RestoreOverTime(float healthPerTick, float tickInterval, float duration, GameObject healSource = null)
        {
            float remainingTime = duration;
            WaitForSeconds waitBetweenTicks = new WaitForSeconds(tickInterval);

            while (remainingTime > 0)
            {
                Restore(healthPerTick, healSource);
                yield return waitBetweenTicks;
                remainingTime -= tickInterval;
            }
        }
    }
}