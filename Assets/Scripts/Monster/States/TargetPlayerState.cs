using LordBreakerX.States;
using UnityEngine;

[CreateAssetMenu(fileName = MonsterStates.TARGET_PLAYER, menuName = MonsterStates.CreatePaths.TARGET_PLAYER)]
public class TargetPlayerState : BaseState
{
    [SerializeField]
    [Min(0)]
    private float _timeInState = 30;

    private MonsterAttackController _monsterAttack;
    private StateMachineNetworked _machine;

    private Timer _stateTimer;

    public override string ID => MonsterStates.TARGET_PLAYER;

    protected override void OnInitilization()
    {
        _monsterAttack = StateObject.GetComponent<MonsterAttackController>();
        _machine = StateObject.GetComponent<StateMachineNetworked>();
        _stateTimer = new Timer(_timeInState);
        _stateTimer.OnTimerFinished += LeaveState;
    }

    private void LeaveState()
    {
        _machine.RequestChangeState(MonsterStates.WANDER);
    }

    public override void Enter()
    {
        _monsterAttack.PlayerAttackTimer.Reset();
        _stateTimer.Reset();

        _monsterAttack.UpdateTarget();
        _monsterAttack.ResetDamageTable();

        if (!_monsterAttack.HasTarget) _machine.RequestChangeState(MonsterStates.WANDER);
    }

    public override void Exit()
    {
        _monsterAttack.RequestStopAttack();
    }

    public override void Update()
    {
        _stateTimer.Update();

        if (!_stateTimer.IsComplete && !_monsterAttack.IsAttacking)
        {
            _monsterAttack.RequestStartAttack();
        }
    }
}
