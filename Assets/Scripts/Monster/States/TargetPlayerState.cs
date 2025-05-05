using LordBreakerX.States;
using UnityEngine;

[CreateAssetMenu(menuName ="Monster/States/Target Player")]
public class TargetPlayerState : BaseState
{
    [SerializeField]
    [Min(0)]
    private float _timeInState = 30;

    private MonsterController _monster;
    private Timer _stateTimer;

    protected override void OnInitilization()
    {
        _monster = StateObject.GetComponent<MonsterController>();
        _stateTimer = new Timer(_timeInState);
        _stateTimer.onTimerFinished += LeaveState;
    }

    private void LeaveState()
    {
        _monster.Machine.RequestChangeState("Neutral");
    }

    public override void Enter()
    {
        _stateTimer.Reset();

        _monster.UpdateTarget();
        _monster.ResetDamageTable();

        if (_monster.AttackTarget == null) _monster.Machine.RequestChangeState("Neutral");
    }

    public override void Exit()
    {
        _monster.AttackHandler.RequestStopAttack();
    }

    public override void Update()
    {
        _stateTimer.Step();

        if (!_stateTimer.IsFinished && !_monster.AttackHandler.IsAttacking)
        {
            _monster.AttackHandler.StartRandomAttack();
        }
    }
}
