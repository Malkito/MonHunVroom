using LordBreakerX.AbilitySystem;
using LordBreakerX.States;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Monster States/Neutral")]
public class OldNeutralState : BaseState
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
    private OldAttackPlayerState _attackState;

    private AbilityHandler _abilityHandler;

    private MonsterControllerOld _monsterController;

    protected override void OnInitilization()
    {
        _abilityHandler = StateObject.GetComponent<AbilityHandler>();
        _monsterController = StateObject.GetComponent<MonsterControllerOld>();

        //_playerAttackTimer = new Timer(_timeBetweenPlayerAttacks);
        //_randomAttackTimer = new Timer(_timeBetweenRandomAttacks);

        //_playerAttackTimer.onTimerFinished += OnPlayerAttack;
        //_randomAttackTimer.onTimerFinished += OnRandomAttack;
    }

    private void OnRandomAttack()
    {
        _abilityHandler.StartRandomAbility();
    }

    private void OnPlayerAttack()
    {
        Machine.ChangeState(_attackState);
    }

    public override void Update()
    {
        //_playerAttackTimer.Step();
        //_randomAttackTimer.Step();

        //if (!_abilityHandler.HasActiveAbility && _targetPosition != _agent.destination) _agent.SetDestination(_targetPosition);

        //if (CanWander())
        //{
        //    _targetPosition = NavMeshUtility.GetRandomPosition(_agent.transform.position, _wanderRange);
        //    _agent.SetDestination(_targetPosition);
        //}
    }

    private bool CanWander()
    {
        return true;
        //return !_abilityHandler.HasActiveAbility && (Vector3.Distance(StateObject.transform.position, _targetPosition) <= 1.2f || _agent.pathStatus != NavMeshPathStatus.PathComplete);
    }

    public override void Enter()
    {
        //_randomAttackTimer.Reset();
        //_targetPosition = _agent.transform.position;
        //_agent.SetDestination(_targetPosition);
    }

    public override void OnGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(StateObject.transform.position, _wanderRange);

        Gizmos.color = Color.cyan;
        //Gizmos.DrawSphere(_agent.transform.position, 0.1f);
        //Gizmos.DrawSphere(_targetPosition, 0.1f);
    }
}
