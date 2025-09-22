using LordBreakerX.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class MonsterMovementController : NetworkBehaviour
{
    public const string WALK_ANIMATION_VARIABLE = "walk";

    [SerializeField]
    [Header("Wandering Properties")]
    private float _wanderRadius;

    [SerializeField]
    private float _reachedDestinationDistance = 0.2f;

    [SerializeField]
    private ParticleSystem _undergroundParticle;

    [SerializeField]
    private Transform _model;

    [SerializeField]
    private Collider _collider;

    private NavMeshAgent _monsterAgent;

    private Animator _monsterAnimator;

    public ParticleSystem UndergroundParticle { get => _undergroundParticle; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _monsterAgent = GetComponent<NavMeshAgent>();
        _monsterAnimator = GetComponent<Animator>();
        _monsterAgent.speed = EnemyStatManager.MovementSpeed;
        _monsterAgent.angularSpeed = EnemyStatManager.TurningSpeed;
    }

    public void UpdateWalkAnimation(bool isWalking)
    {
        if (_monsterAnimator != null) 
        {
            _monsterAnimator.SetBool(WALK_ANIMATION_VARIABLE, isWalking);
        }
    }

    public void StopMovement()
    {
        _monsterAgent.StopAgent();
    }

    public void Wander()
    {
        if (ReachedDestination() && IsServer)
        {
            _monsterAgent.SetRandomDestination(_wanderRadius);
        }
    }

    public void SetUnderground(bool isUnderground)
    {
        _collider.enabled = !isUnderground;
        _model.gameObject.SetActive(!isUnderground);
        _undergroundParticle.gameObject.SetActive(isUnderground);
    }

    public bool ReachedDestination()
    {
        return _monsterAgent.ReachedDestination(_reachedDestinationDistance);
    }

    public void ChangeDestination(Vector3 destination)
    {
        if (IsServer)
        {
            _monsterAgent.SetDestination(destination);
        }
    }
}
