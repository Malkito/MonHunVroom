using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;


namespace LordBreakerX.Health
{
    /// <summary>
    /// Represents an object that has health and can take damage or be healed. 
    /// Invokes events when the health changes or when the object dies.
    /// </summary>
    /// 
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

        [HideInInspector] public float numOfFireOnMonster;

        private Coroutine damageOverTimeCoroutineHolder;

        [SerializeField]
        private SkinnedMeshRenderer mat;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                _currentHealth.Value = EnemyStatManager.MaxHealth;
                HealthInfo healthInfo = new HealthInfo(EnemyStatManager.MaxHealth, _currentHealth.Value, 0, 0, null);
                _onHealthChangedServerSide.Invoke(healthInfo);
            }

            _currentHealth.OnValueChanged += OnHealthChanged;
        }

        private void OnHealthChanged(float previousValue, float newValue)
        {
            if (IsClient)
            {
                HealthInfo healthInfo = new HealthInfo(EnemyStatManager.MaxHealth, _currentHealth.Value, previousValue - newValue, 0, null);
                _onHealthChangedClientSide.Invoke(healthInfo);

                if (newValue <= 0)
                {
                    _onDeathClientSide.Invoke();
                }
            }
        }

        public void dealDamage(float damageDealt, Color flashColor, GameObject damageOrigin)
        {
            if (IsServer)
            {
                float clampedAmount = Mathf.Clamp(damageDealt, 0, _currentHealth.Value);

                _currentHealth.Value -= clampedAmount;

                if (clampedAmount > 0)
                {
                    HealthInfo healthInfo = new HealthInfo(EnemyStatManager.MaxHealth, _currentHealth.Value, clampedAmount, 0, damageOrigin);
                    _onHealthChangedServerSide.Invoke(healthInfo);
                }

                if (_currentHealth.Value <= 0)
                {
                    _onDeathServerSide.Invoke();
                }
            }
        }

        private void Update()
        {
            if (numOfFireOnMonster > 0)
            {
                if(damageOverTimeCoroutineHolder != null)
                {
                    return;
                }
                damageOverTimeCoroutineHolder = StartCoroutine(damageOverTimeCoroutine(numOfFireOnMonster, 1));

            }
            else
            {
                stopDamageOverTime();
            }           
        }

        private IEnumerator damageOverTimeCoroutine(float damagePerTick, float burnInterval)
        {
            float elapsedTime = 0f;
            while(elapsedTime < burnInterval)
            {
                yield return new WaitForSeconds(1f);
                dealDamage(damagePerTick, Color.red, gameObject);
                //StartCoroutine(flashDamageColor(Color.red, 0.1f));
                elapsedTime += 1f;
            }

            damageOverTimeCoroutineHolder = null;
        }

        private void stopDamageOverTime()
        {
            if (damageOverTimeCoroutineHolder != null)
            {
                StopCoroutine(damageOverTimeCoroutineHolder);
                damageOverTimeCoroutineHolder = null;
            }
        }


        private IEnumerator flashDamageColor(Color flashColor, float flashTime) // flashes the color of hte object when it takes damage
        {
            Color baseMat = mat.materials[0].color;
            mat.materials[0].color = flashColor;
            yield return new WaitForSeconds(flashTime);
            mat.materials[0].color = baseMat;
        }

        public void increaseFireNumber()
        {
            numOfFireOnMonster++;
            
        }

        public void decreaseFireNumber()
        {
            numOfFireOnMonster--;
        }
    }
}