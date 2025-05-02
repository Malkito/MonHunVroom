using LordBreakerX.AbilitySystem;
using LordBreakerX.States;
using UnityEngine;

[CreateAssetMenu(menuName = "Monster/States/Neutral")]
public class NeutralState : BaseState
{
    // variables of states should not be changed once initilized

    // variables that change that needs to be done for all clients should be done in a NetworkBehavior script

    [SerializeField]
    private float _wanderRange;

    [SerializeField]
    [Min(0f)]
    private float _timeBetweenPlayerAttacks = 30;

    [SerializeField]
    [Min(0f)]
    private float _timeBetweenRandomAttacks = 10;

    [SerializeField]
    private BaseState _attackState;

    private MonsterController _monster;
    private AbilityHandler _abilityHandler;

    private Timer _playerAttackTimer;
    private Timer _randomAttackTimer;

    protected override void OnInitilization()
    {
        _abilityHandler = StateObject.GetComponent<AbilityHandler>();
        _monster = StateObject.GetComponent<MonsterController>();
        _abilityHandler = StateObject.GetComponent<AbilityHandler>();

        _playerAttackTimer = new Timer(_timeBetweenPlayerAttacks);
        _randomAttackTimer = new Timer(_timeBetweenRandomAttacks);

        _playerAttackTimer.onTimerFinished += () => { Machine.ChangeState(_attackState); }; 

        _randomAttackTimer.onTimerFinished += () => { if (_monster != null) _monster.RequestStartRandomAttack(); };
    }

    public override void Enter()
    {
        _randomAttackTimer.Reset();
        _playerAttackTimer.Reset();
        _monster.StopMovement();
    }

    public override void Update()
    {
        _playerAttackTimer.Step();
        _randomAttackTimer.Step();

        _monster.UpdateWalkAnimation();

        if (CanChangeDestination())
        {
            _monster.RandomDestination(_wanderRange);
        }
    }

    private bool CanChangeDestination()
    {
        return (Vector3.Distance(StateObject.transform.position, _monster.TargetPosition) <= 1.2f || !_monster.DestinationReachable);
        //return !_abilityHandler.HasActiveAbility && (Vector3.Distance(StateObject.transform.position, _monster.TargetPosition) <= 1.2f || !_monster.DestinationReachable);
    }

    public override void OnGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(StateObject.transform.position, _wanderRange);
    }
}
