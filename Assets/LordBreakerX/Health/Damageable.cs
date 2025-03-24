using UnityEngine;
using UnityEngine.Events;

namespace LordBreakerX.Health
{
    public class Damageable : MonoBehaviour
    {
        [Min(1)]
        [SerializeField]
        private float _maxHealth = 1;

        [SerializeField]
        [Header("Events")]
        private UnityEvent<float> _onHealthChanged = new UnityEvent<float>();

        [SerializeField]
        private UnityEvent _onDeath = new UnityEvent();

        private float _currentHealth;

        public UnityEvent<float> OnHealthChanged { get { return _onHealthChanged; } }
        public UnityEvent OnDeath { get { return _onDeath; } }


        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        public void Damage(float amount)
        {
            float clampedAmount = Mathf.Max(amount, 0);

            _currentHealth -= clampedAmount;

            OnHealthChanged.Invoke(_currentHealth);

            if (_currentHealth <= 0) OnDeath.Invoke();
        }

        public void Restore(float amount)
        {
            float clampedAmount = Mathf.Clamp(amount, 0, _maxHealth - _currentHealth);

            _currentHealth += clampedAmount;

            OnHealthChanged.Invoke(_currentHealth);
        }
    }
}