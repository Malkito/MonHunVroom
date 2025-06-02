using LordBreakerX.States;
using UnityEngine;

[CreateAssetMenu(fileName = MonsterStates.TARGET_PLAYER, menuName = MonsterStates.CreatePaths.TARGET_PLAYER)]
public class TargetPlayerState : BaseState
{
    [SerializeField]
    [Min(0)]
    private float _timeInState = 30;

    private MonsterController _monster; 
    private Timer _stateTimer;

    public override string ID => MonsterStates.TARGET_PLAYER;

    protected override void OnInitilization()
    {
        _monster = StateObject.GetComponent<MonsterController>();
        _stateTimer = new Timer(_timeInState);
        _stateTimer.OnTimerFinished += LeaveState;
    }

    private void LeaveState()
    {
        _monster.Machine.RequestChangeState(MonsterStates.WANDER);
    }

    public override void Enter()
    {
        _monster.PlayerAttackTimer.Reset();
        _stateTimer.Reset();

        _monster.UpdateTarget();
        _monster.ResetDamageTable();

        if (_monster.AttackTarget == null) _monster.Machine.RequestChangeState(MonsterStates.WANDER);
    }

    public override void Exit()
    {
        _monster.AttackHandler.RequestStopAttack();
    }

    public override void Update()
    {
        _stateTimer.Update();

        if (!_stateTimer.IsComplete && !_monster.AttackHandler.IsAttacking)
        {
            _monster.AttackHandler.RequestStartAttack();
        }
    }
}
