using LordBreakerX.Utilities.AI;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class MonsterMovementController : NetworkBehaviour
{
    public const string WALK_ANIMATION_VARIABLE = "walk";

    [SerializeField]
    [Header("Wandering Properties")]
    [Min(1)]
    private float _minMovementRadius;

    [SerializeField]
    [Min(1)]
    private float _maxMovementRadius;

    [SerializeField]
    [Range(0, 1)]
    private float _radiusDecreaseRate = 0.01f;

    private NavMeshAgent _monsterAgent;
    private Animator _monsterAnimator;

    private RandomPathGenerator _pathGenerator;

    public Vector3 CurrentDestination { get; private set; }

    private void OnValidate()
    {
        _minMovementRadius = Mathf.Clamp(_minMovementRadius, 1, _maxMovementRadius);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _monsterAgent = GetComponent<NavMeshAgent>();
        _monsterAnimator = GetComponent<Animator>();
        _pathGenerator = new RandomPathGenerator(transform, _minMovementRadius, _maxMovementRadius, _radiusDecreaseRate);
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

    public void SetRandomDestination()
    {
        if (_pathGenerator == null) _pathGenerator = new RandomPathGenerator(transform, _minMovementRadius, _maxMovementRadius, _radiusDecreaseRate);
        if (IsServer && !_pathGenerator.IsFindingPath) StartCoroutine(DetermineTargetPosition());
    }

    private IEnumerator DetermineTargetPosition()
    {
        yield return _pathGenerator.FindReachablePath();

        _monsterAgent.SetPath(_pathGenerator.GeneratedPath);
        CurrentDestination = _monsterAgent.destination;
    }

    public void ChangeDestination(Vector3 destination)
    {
        if (IsServer)
        {
            _monsterAgent.SetDestination(destination);
            CurrentDestination = destination;
        }
    }
}
