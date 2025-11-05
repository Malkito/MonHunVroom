using LordBreakerX.Health;
using LordBreakerX.States;
using LordBreakerX.Utilities;
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
    private float _wanderRadius;

    [SerializeField]
    private float _reachedDestinationDistance = 0.2f;

    [SerializeField]
    private ParticleSystem _undergroundParticle;

    [SerializeField]
    private Transform _model;

    [SerializeField]
    private Transform _monsterTransform;

    [SerializeField]
    private Collider _collider;

    private NavMeshAgent _monsterAgent;

    private Animator _monsterAnimator;

    private float _monsterStartHeight;

    private MonsterHealth _health;

    public ParticleSystem UndergroundParticle { get => _undergroundParticle; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _monsterAgent = GetComponent<NavMeshAgent>();
        _monsterAnimator = GetComponent<Animator>();
        _monsterAgent.speed = EnemyStatManager.MovementSpeed;
        _monsterAgent.angularSpeed = EnemyStatManager.TurningSpeed;
        _monsterStartHeight = _monsterTransform.localPosition.y;
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

    public bool ReachedDestination(float reachedDistance)
    {
        return _monsterAgent.ReachedDestination(reachedDistance);
    }

    public void ChangeDestination(Vector3 destination)
    {
        if (IsServer)
        {
            _monsterAgent.SetDestination(destination);
        }
    }

    public void Fly(float flyHeight, float flySpeed)
    {
        Vector3 flyPosition = new Vector3(_monsterTransform.position.x, flyHeight, _monsterTransform.position.z);
        _monsterTransform.position = Vector3.MoveTowards(_monsterTransform.position, flyPosition, flySpeed * Time.deltaTime);
    }

    public bool IsFlying(float flyHeight)
    {
        Vector3 flyPosition = new Vector3(_monsterTransform.position.x, flyHeight, _monsterTransform.position.z);
        return Vector3.Distance(_monsterTransform.position, flyPosition) <= 0.2f;
    }

    public void LandFromFlight() 
    {
        Vector3 monsterPosition = _monsterTransform.localPosition;
        Vector3 stopPosition = new Vector3(monsterPosition.x, _monsterStartHeight, monsterPosition.y);
        _monsterTransform.localPosition = stopPosition;
    }

    public MonsterHealth GetMonsterHealth()
    {
        if (_health == null)
        {
            _health = _monsterTransform.GetComponent<MonsterHealth>();
        }
        return _health;
    }

}
