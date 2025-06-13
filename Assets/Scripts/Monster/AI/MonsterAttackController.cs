using LordBreakerX.Attributes;
using LordBreakerX.Health;
using LordBreakerX.States;
using LordBreakerX.Utilities.AI;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MonsterAttackController : AttackController
{
    [SerializeField]
    [RequiredField]
    [Header("Laser Eyes")]
    private Transform[] _eyes;

    [SerializeField]
    [RequiredField]
    private Laser _laserPrefab;

    [SerializeField]
    [Min(0f)]
    private float _timeBetweenPlayerAttacks = 30;

    private Dictionary<GameObject, float> _damageTable = new Dictionary<GameObject, float>();

    private Timer _playerAttackTimer;

    private StateMachineNetworked _machine;

    public Timer PlayerAttackTimer { get { return _playerAttackTimer; } }

    public bool IsFindingAttackPosition { get; private set; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _machine = GetComponent<StateMachineNetworked>();
        _playerAttackTimer = new Timer(_timeBetweenPlayerAttacks);
        _playerAttackTimer.OnTimerFinished += () => { _machine.ChangeStateWhen(MonsterStates.TARGET_PLAYER, () => !IsAttacking && !IsRequestingAttack); };
    }

    //------------------------------
    // Laser Eyes Methods
    //------------------------------

    public void RequestShootLaser(Laser prefab, Vector3 attackPosition)
    {
        if (IsServer)
        {
            ShootLaser(prefab, attackPosition);
            ShootLaserClientRpc(attackPosition);
        }
    }

    private void ShootLaser(Laser prefab, Vector3 attackPosition)
    {
        int randomEyeIndex = Random.Range(0, _eyes.Length);
        Vector3 eyePosition = _eyes[randomEyeIndex].position;

        Laser.CreateLaser(prefab, gameObject, eyePosition, attackPosition);
    }

    [ClientRpc(RequireOwnership = false)]
    private void ShootLaserClientRpc(Vector3 attackPosition)
    {
        if (!IsHost || !IsServer) ShootLaser(_laserPrefab, attackPosition);
    }

    //-------------------------------------
    // Targeting Player Methods
    //-------------------------------------

    public void OnMonsterHealthChanged(HealthInfo healthInfo)
    {
        if (!IsServer) return;

        Debug.Log($"Called Damaged Monster with following: (CH:{healthInfo.CurrentHealth}, MH:{healthInfo.Maxhealth}, HS:{healthInfo.Source != null}, DC:{healthInfo.DamageCaused})");

        if (healthInfo.Source == null || healthInfo.DamageCaused <= 0) return;

        if (_damageTable.ContainsKey(healthInfo.Source))
            _damageTable[healthInfo.Source] += healthInfo.DamageCaused;
        else
            _damageTable[healthInfo.Source] = healthInfo.DamageCaused;
    }

    public void ResetDamageTable()
    {
        if (IsServer) _damageTable.Clear();
    }

    public void UpdateTarget()
    {
        if (!IsServer) return;

        if (_damageTable.Count > 0)
        {
            KeyValuePair<GameObject, float> highestPair = new KeyValuePair<GameObject, float>(null, 0);

            foreach (KeyValuePair<GameObject, float> damagePair in _damageTable)
            {
                if (damagePair.Value > 0 && damagePair.Value > highestPair.Value)
                {
                    highestPair = damagePair;
                }
            }

            TargetProvider.SetTarget(highestPair.Key.transform, transform.position);
        }
        else
        {
            TargetProvider.SetTargetPosition(transform.position);
        }
    }

    //-------------------------------------
    // Random Attacking Methods
    //-------------------------------------

    public void RequestRandomAttackPosition(Vector3 start, float attackRadius)
    {
        if (IsServer) StartCoroutine(RandomAttackPosition(start, attackRadius));
    }

    private IEnumerator RandomAttackPosition(Vector3 start, float attackRadius)
    {
        RandomPathGenerator generator = new RandomPathGenerator(transform, attackRadius);
        yield return generator.FindReachablePath();
        TargetProvider.SetTargetPosition(generator.GeneratedDestination);
        RequestStartAttack();
    }
}

