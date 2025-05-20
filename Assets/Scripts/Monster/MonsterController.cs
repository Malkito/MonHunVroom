using LordBreakerX.Health;
using LordBreakerX.States;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class MonsterController : NetworkBehaviour
{
    [SerializeField]
    private Transform[] _eyes;
    [SerializeField]
    private NavMeshAgent _agent;

    [SerializeField]
    private AttackController _attackController;

    [SerializeField]
    private StateMachineNetworked _stateMachine;

    [SerializeField]
    private Laser _laserPrefab;

    [SerializeField]
    ParticleSystem _stompEffect;

    private Animator _animator;

    private NetworkVariable<Vector3> _targetPosition = new NetworkVariable<Vector3>();

    private Dictionary<GameObject, float> _damageTable = new Dictionary<GameObject, float>();

    public Vector3 TargetPosition { get {  return _targetPosition.Value; } }

    public AttackController AttackHandler {  get { return _attackController; } }

    public StateMachineNetworked Machine { get { return _stateMachine; } }

    public bool DestinationReachable { get { return _agent.pathStatus == NavMeshPathStatus.PathComplete; } }

    public GameObject AttackTarget { get; private set; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _animator = GetComponent<Animator>();

        _targetPosition.OnValueChanged += OnTargetPositionChanged;
    }

    #region Movement Logic

    private void OnTargetPositionChanged(Vector3 previousValue, Vector3 newValue)
    {
        _agent.SetDestination(newValue);
    }

    public void StopMovement()
    {
        if (IsServer) _targetPosition.Value = _agent.transform.position;
    }

    public void RandomDestination(float range)
    {
        if (IsServer) _targetPosition.Value = NavMeshUtility.GetRandomPosition(_agent.transform.position, range);
    }

    public void ChangeDestination(Vector3 destination)
    {
        if (IsServer) _targetPosition.Value += destination;
    }

    public void UpdateWalkAnimation()
    {
        if (_agent.velocity.sqrMagnitude >= 0.1f)
        {
            _animator.SetBool("walk", true);
        }
        else
        {
            _animator.SetBool("walk", false);
        }
    }

    public void RequestStartRandomAttack()
    {
        _attackController.StartRandomAttack();
    }
    #endregion

    #region Attacking Logic

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

    #endregion

    #region Damaged Logic

    /// <summary>
    /// and be used on client and server side
    /// </summary>
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

            AttackTarget = highestPair.Key;
        }
        else
        {
            AttackTarget = null;
        }
    }

    public void Stomp(float effectRadius)
    {
        _stompEffect.Play();

        RaycastHit[] hits = Physics.SphereCastAll(transform.position, effectRadius, Vector3.down, effectRadius);
        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.CompareTag("Monster") && IsServer)
            {
                dealDamage damage = hit.collider.gameObject.GetComponent<dealDamage>();
                if (damage != null) damage.dealDamage(50, Color.red, gameObject);
            }

        }
    }

    #endregion
}
