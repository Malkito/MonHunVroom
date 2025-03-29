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
        private UnityEvent<float> _onHealthChanged = new UnityEvent<float>();

        [SerializeField]
        [Tooltip("Invoked when the health reaches zero or below.")]
        private UnityEvent _onDeath = new UnityEvent();

        private float _currentHealth;

        /// <summary>
        /// Event triggered whenever the health changes.
        /// The event passes the current health value as a parameter.
        /// </summary>
        public UnityEvent<float> OnHealthChanged { get { return _onHealthChanged; } }

        /// <summary>
        /// Event triggered when the health reaches zero or below.
        /// </summary>
        public UnityEvent OnDeath { get { return _onDeath; } }


        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        /// <summary>
        /// Reduces the object's health by the specified amount.
        /// Invokes the <see cref="OnHealthChanged"/> event with the new health value.
        /// If the health reaches zero, it also invokes the <see cref="OnDeath"/> event.
        /// </summary>
        /// <param name="amount">The amount of health to remove. Must be 0 or greater.</param>
        public void Damage(float amount)
        {
            float clampedAmount = Mathf.Max(amount, 0);

            _currentHealth = Mathf.Max(_currentHealth - clampedAmount, 0);

            OnHealthChanged.Invoke(_currentHealth);

            if (_currentHealth <= 0) OnDeath.Invoke();
        }

        /// <summary>
        /// Restores the object's health by the specified amount.
        /// Invokes the <see cref="OnHealthChanged"/> event with the new health value.
        /// </summary>
        /// <param name="amount">The amount of health to restore. Must be 0 or greater.</param>
        public void Restore(float amount)
        {
            float clampedAmount = Mathf.Max(amount, 0);

            _currentHealth = Mathf.Min(_currentHealth + clampedAmount, _maxHealth);

            OnHealthChanged.Invoke(_currentHealth);
        }
    }
}