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

    private NavMeshAgent _monsterAgent;

    private Animator _monsterAnimator;

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

    public void Wander()
    {
        if (ReachedDestination())
        {
            Vector2 random = Random.insideUnitCircle * _wanderRadius;
            Vector3 position = new Vector3(random.x, transform.position.y, random.y);

            _monsterAgent.SetDestination(position);
        }
    }

    public bool ReachedDestination()
    {
        return Vector3.Distance(_monsterAgent.destination, transform.position) <= _reachedDestinationDistance;
    }

    public void ChangeDestination(Vector3 destination)
    {
        if (IsServer)
        {
            _monsterAgent.SetDestination(destination);
        }
    }
}
