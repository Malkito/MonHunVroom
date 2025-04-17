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
        private UnityEvent<HealthInfo> _onHealthChanged = new UnityEvent<HealthInfo>();

        [SerializeField]
        [Tooltip("Invoked when the health reaches zero or below.")]
        private UnityEvent _onDeath = new UnityEvent();

        private float _currentHealth;

        private HealthEffectType _activeEffect = HealthEffectType.None;

        private void Awake()
        {
            _currentHealth = _maxHealth;
            HealthInfo healthInfo = new HealthInfo(_maxHealth, _currentHealth, 0, 0, null);
            _onHealthChanged.Invoke(healthInfo);
            OnAwake();
        }

        public virtual void OnAwake()
        {

        }

        /// <summary>
        /// Applies damage to the object, reducing its current health by a clamped amount.
        /// Invokes health changed and death events if applicable.
        /// </summary>
        /// <param name="amount">The amount of damage to apply.</param>
        /// <param name="damageSource">Optional source of the damage.</param>
        public void Damage(float amount, GameObject damageSource = null)
        {
            float clampedAmount = Mathf.Clamp(amount, 0, _currentHealth);

            _currentHealth -= clampedAmount;

            if (clampedAmount > 0)
            {
                HealthInfo healthInfo = new HealthInfo(_maxHealth, _currentHealth, clampedAmount, 0, damageSource);
                _onHealthChanged.Invoke(healthInfo);
                OnDamaged(healthInfo);
            }

            if (_currentHealth <= 0)
            {
                _onDeath.Invoke();
                OnDeath();
            }
        }

        /// <summary>
        /// Don't use this method for doing damage as will be removed in the future (just here for testing purposes)
        /// </summary>
        /// <param name="amount"></param>
        public void TestDamage(float amount)
        {
            Damage(amount, null);
        }

        protected virtual void OnDamaged(HealthInfo healthInfo) {  }

        protected virtual void OnDeath() { }


        public void StartDamageOverTime(float damagePerTick, float tickInterval, float duration, GameObject damageSource = null)
        {
            if (_activeEffect == HealthEffectType.DamageOverTime)
            {
                Debug.LogWarning("Didn't start Damage Over Time since there is already one active!");
                return;
            }
            StartCoroutine(DamageOverTime(damagePerTick, tickInterval, duration, damageSource));
        }

        private IEnumerator DamageOverTime(float damagePerTick, float tickInterval, float duration, GameObject damageSource = null) 
        {
            float remainingTime = duration;
            WaitForSeconds waitBetweenTicks = new WaitForSeconds(tickInterval);
            _activeEffect = HealthEffectType.DamageOverTime;

            while (remainingTime > 0 && _activeEffect == HealthEffectType.DamageOverTime)
            {
                Damage(damagePerTick, damageSource);
                yield return waitBetweenTicks;
                remainingTime -= tickInterval;
            }

            if (_activeEffect == HealthEffectType.DamageOverTime) _activeEffect = HealthEffectType.None;
        }

        public void Restore(float amount, GameObject healSource = null)
        {
            float clampedAmount = Mathf.Clamp(amount, 0, _maxHealth - _currentHealth);

            if (clampedAmount > 0)
            {
                _currentHealth += clampedAmount;

                HealthInfo healthInfo = new HealthInfo(_maxHealth, _currentHealth, 0, clampedAmount, healSource);
                _onHealthChanged.Invoke(healthInfo);
                OnHealed(healthInfo);
            }
        }

        protected virtual void OnHealed(HealthInfo healthInfo)
        {

        }

        public void StartRestoreOverTime(float healthPerTick, float tickInterval, float duration, GameObject healSource = null)
        {
            if (_activeEffect == HealthEffectType.HealOverTime)
            {
                Debug.LogWarning("Didn't start Restore Over Time since there is already one active!");
                return;
            }
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