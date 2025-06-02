using LordBreakerX.Health;
using LordBreakerX.States;
using LordBreakerX.Utilities.Tags;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class MonsterController : NetworkBehaviour
{
    public const string WALK_ANIMATION_VARIABLE = "walk";

    private const string STARTING_MONSTER_TAG = "Monster";

    private const string TAIL_SWIPE_ANIMATION = "tail swipe";

    private const float STOMP_DAMAGE_AMOUNT = 50;

    private static readonly Color STOMP_FLASH_COLOR = Color.red;

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

    [SerializeField]
    [Min(0f)]
    private float _timeBetweenPlayerAttacks = 30;

    [SerializeField]
    [TagDropdown]
    private string _monsterTag = STARTING_MONSTER_TAG;

    private Animator _animator;

    private Dictionary<GameObject, float> _damageTable = new Dictionary<GameObject, float>();

    public Vector3 TargetPosition { get; private set; }

    public AttackController AttackHandler {  get { return _attackController; } }

    public StateMachineNetworked Machine { get { return _stateMachine; } }

    public GameObject AttackTarget { get; private set; }

    public Timer PlayerAttackTimer { get; private set; }

    public bool FindingTargetPosition { get; private set; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _animator = GetComponent<Animator>();

        PlayerAttackTimer = new Timer(_timeBetweenPlayerAttacks);
        PlayerAttackTimer.OnTimerFinished += () => { Machine.ChangeStateWhen(MonsterStates.TARGET_PLAYER, () => !AttackHandler.IsAttacking && !AttackHandler.IsRequestingAttack); };
    }

    private void Update()
    {
        if (_animator != null) 
        {
            UpdateWalkAnimation();
        }
    }

    #region Movement Logic

    private void UpdateWalkAnimation()
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

    public void StopMovement()
    {
        ChangeDestination(transform.position);
    }

    public void RandomDestination(float minRadius, float maxRadius, float decreaseRate)
    {
        if (IsServer && !FindingTargetPosition) StartCoroutine(DetermineTargerPosition(minRadius, maxRadius, decreaseRate));
    }

    private IEnumerator DetermineTargerPosition(float minRadius, float maxRadius, float decreaseRate)
    {
        FindingTargetPosition = true;

        float currentRange = maxRadius;
        Vector3 position = NavMeshUtility.GetRandomPosition(_agent.transform.position, currentRange);

        NavMeshPath path = new NavMeshPath();

        while (!IsCompletePath(position, path))
        {
            currentRange = Mathf.Clamp(currentRange - decreaseRate, minRadius, maxRadius);
            position = NavMeshUtility.GetRandomPosition(_agent.transform.position, currentRange);
            yield return null;
        }

        TargetPosition = position;
        _agent.SetPath(path);
        
        FindingTargetPosition = false;
    }

    public bool IsCompletePath(Vector3 targetPosition, NavMeshPath path)
    {
        if (NavMesh.CalculatePath(_agent.transform.position, targetPosition, NavMesh.AllAreas, path))
        {
            return path.status == NavMeshPathStatus.PathComplete;
        }
        return false;
    }

    public void ChangeDestination(Vector3 destination)
    {
        if (IsServer)
        {
            _agent.SetDestination(destination);
            TargetPosition = destination;
            FindingTargetPosition = false;
        }
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
            if (!hit.collider.CompareTag(_monsterTag) && IsServer)
            {
                dealDamage damage = hit.collider.gameObject.GetComponent<dealDamage>();
                if (damage != null) damage.dealDamage(STOMP_DAMAGE_AMOUNT, STOMP_FLASH_COLOR, gameObject);
            }

        }
    }

    public void TailSwipe()
    {
        _animator.Play(TAIL_SWIPE_ANIMATION);
    }

    public bool TailSwipeFinished()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        return !stateInfo.IsName(TAIL_SWIPE_ANIMATION);
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        if (AttackTarget != null) 
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(AttackTarget.transform.position, Vector3.one);
        }

        if (_agent != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(_agent.destination, Vector3.one);
        }
    }
}
