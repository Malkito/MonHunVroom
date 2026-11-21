using LordBreakerX.States;
using UnityEngine;

[CreateStateMenu("MonsterWander", "Monster Wander State", "Monster States")]
[System.Serializable]
public class MonsterWanderState : MonsterState
{
    [SerializeField]
    [Min(1)]
    private float _minDuration = 5;

    [SerializeField]
    [Min(1)]
    private float _maxDuration = 10;

    [SerializeField]
    private StateReference _attackState;

    private Timer _durationTimer;

    public MonsterWanderState() : base()
    {

    }

    public MonsterWanderState(IStateMachine machine, MonsterWanderState toCopy) : base(machine, toCopy)
    {
        _minDuration = toCopy._minDuration;
        _maxDuration = toCopy._maxDuration;
        _attackState = toCopy._attackState;
    }

    public override void OnCreate()
    {
        _durationTimer = new Timer(OnDurationEnded);
    }

    private void OnDurationEnded()
    {
        Machine.ChangeState(_attackState);
    }

    public override void OnEnter()
    {
        // set wander duration
        float duration = Random.Range(_minDuration, _maxDuration);
        _durationTimer.SetDuration(duration);

        if (IsServer)
        {
            MovementHandler.StopMovement();
        }
    }

    public override void OnUpdate()
    {
        if (IsServer)
        {
            _durationTimer.Update();
            MovementHandler.Wander();
        }
    }

    public override State CreateCopy(IStateMachine machine) => new MonsterWanderState(machine, this);
}
