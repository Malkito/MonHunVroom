using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace LordBreakerX.Health
{
    /// <summary>
    /// Represents an object that has health and can take damage or be healed. 
    /// Invokes events when the health changes or when the object dies.
    /// </summary>
    /// 

    [RequireComponent(typeof(MonsterStatManager))]
    public class MonsterHealth : NetworkBehaviour, dealDamage
    {
        [SerializeField]
        [Header("Events")]
        [Tooltip("Invoked whenever the health changes.")]
        private UnityEvent<HealthInfo> _onHealthChangedServerSide = new UnityEvent<HealthInfo>();

        [SerializeField]
        [Tooltip("Invoked whenever the health changes.")]
        private UnityEvent<HealthInfo> _onHealthChangedClientSide = new UnityEvent<HealthInfo>();

        [SerializeField]
        [Tooltip("Invoked when the health reaches zero or below.")]
        private UnityEvent _onDeathServerSide = new UnityEvent();

        [SerializeField]
        [Tooltip("Invoked when the health reaches zero or below.")]
        private UnityEvent _onDeathClientSide = new UnityEvent();

        private NetworkVariable<float> _currentHealth = new NetworkVariable<float>(100);

        private MonsterStatManager _monsterStatManager;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _monsterStatManager = GetComponent<MonsterStatManager>();

            if (IsServer)
            {
                _currentHealth.Value = _monsterStatManager.MaxHealth;
                HealthInfo healthInfo = new HealthInfo(_monsterStatManager.MaxHealth, _currentHealth.Value, 0, 0, null);
                _onHealthChangedServerSide.Invoke(healthInfo);
            }

            _currentHealth.OnValueChanged += OnHealthChanged;
        }

        private void OnHealthChanged(float previousValue, float newValue)
        {
            if (IsClient)
            {
                HealthInfo healthInfo = new HealthInfo(_monsterStatManager.MaxHealth, _currentHealth.Value, previousValue - newValue, 0, null);
                _onHealthChangedClientSide.Invoke(healthInfo);

                if (newValue <= 0)
                {
                    _onDeathClientSide.Invoke();
                }
            }
        }

        public void TestDealDamage(GameObject damageSouce)
        {
            dealDamage(1000, Color.red, damageSouce);
        }

        public void dealDamage(float damageDealt, Color flashColor, GameObject damageOrigin)
        {
            if (IsServer)
            {
                float clampedAmount = Mathf.Clamp(damageDealt, 0, _currentHealth.Value);

                _currentHealth.Value -= clampedAmount;

                if (clampedAmount > 0)
                {
                    HealthInfo healthInfo = new HealthInfo(_monsterStatManager.MaxHealth, _currentHealth.Value, clampedAmount, 0, damageOrigin);
                    _onHealthChangedServerSide.Invoke(healthInfo);
                }

                if (_currentHealth.Value <= 0)
                {
                    _onDeathServerSide.Invoke();
                }
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