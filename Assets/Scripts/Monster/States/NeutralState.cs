using LordBreakerX.AbilitySystem;
using LordBreakerX.States;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Monster States/Neutral")]
public class NeutralState : BaseState
{
    [SerializeField]
    [Min(1)]
    [Tooltip("The range that around the monster that the monster can select a wander point from")]
    private float _wanderRange = 10;

    [SerializeField]
    [Min(0f)]
    private float _timeBetweenPlayerAttacks = 30;

    [SerializeField]
    [Min(0f)]
    private float _timeBetweenRandomAttacks = 10;

    [SerializeField]
    private AttackPlayerState _attackState;

    private NavMeshAgent _agent;

    private Timer _playerAttackTimer;
    private Timer _randomAttackTimer;

    private AbilityHandler _abilityHandler;

    protected override void OnInitilization()
    {
        _agent = StateObject.GetComponent<NavMeshAgent>();
        _abilityHandler = StateObject.GetComponent<AbilityHandler>();

        _playerAttackTimer = new Timer(_timeBetweenPlayerAttacks);
        _randomAttackTimer = new Timer(_timeBetweenRandomAttacks);

        _playerAttackTimer.onTimerFinished += OnPlayerAttack;
        _randomAttackTimer.onTimerFinished += OnRandomAttack;
    }

    private void OnRandomAttack()
    {
        _abilityHandler.StartRandomAbility();
        _agent.SetDestination(_agent.transform.position);
    }

    private void OnPlayerAttack()
    {
        Machine.ChangeState(_attackState);
    }

    public override void Update()
    {
        _playerAttackTimer.Step();
        _randomAttackTimer.Step();

        if (CanWamder())
        {
            ChangeDestination();
        }
    }

    private bool CanWamder()
    {
        return !_abilityHandler.HasActiveAbility && _agent.velocity.sqrMagnitude < 0.01f && !_agent.pathPending;
    }

    private void ChangeDestination()
    {
        Vector3 randomPoint = Random.insideUnitSphere * _wanderRange;
        randomPoint += _agent.transform.position;
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, _wanderRange, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
    }

    public override void Enter()
    {
        _randomAttackTimer.Reset();
    }

    public override void Exit()
    {
        
    }

    public override void FixedUpdate()
    {
        
    }

    public override void LateUpdate()
    {
        
    }
}
