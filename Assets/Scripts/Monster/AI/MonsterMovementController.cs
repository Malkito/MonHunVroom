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
        ChangeDestination(transform.position);
    }

    public bool Wander()
    {
        if (ReachedDestination())
        {
            Vector3 randomPosition = NavMeshUtility.GetRandomPosition(transform.position, _wanderRadius);
            _monsterAgent.SetDestination(randomPosition);
            return true;
        }
        return false;
    }

    public void SetUnderground(bool isUnderground)
    {
        _collider.enabled = !isUnderground;
        _model.gameObject.SetActive(!isUnderground);
    }

    public bool ReachedDestination()
    {
        Vector3 destination = new Vector3(_monsterAgent.destination.x, transform.position.y, _monsterAgent.destination.z);
        return Vector3.Distance(destination, transform.position) <= _reachedDestinationDistance;
    }

    public void ChangeDestination(Vector3 destination)
    {
        if (IsServer)
        {
            _monsterAgent.SetDestination(destination);
        }
    }
}
