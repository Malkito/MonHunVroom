using LordBreakerX.States;
using UnityEngine;

/// <summary>
/// Special state started when a player enters above the monster for a short period
/// </summary>
[CreateStateMenu("MonsterSpecialGrab", "Monster Special Grab", "Monster States")]
[System.Serializable]
public class MonsterGrabState : MonsterState
{
    [SerializeField]
    private SpecialGrabAttack _specialGrab;

    private bool _startedAttack;

    [SerializeField]
    private StateReference _wanderState;

    public MonsterGrabState() : base()
    {

    }

    public MonsterGrabState(IStateMachine machine, MonsterGrabState toCopy) : base(machine, toCopy)
    {
        _specialGrab = toCopy._specialGrab;
        _wanderState = toCopy._wanderState;

        _startedAttack = false;
    }

    public override void OnEnter()
    {
        AttackHandler.StopAttack();
        _startedAttack = false;
    }

    public override void OnUpdate()
    {
        if (AttackHandler.IsAttacking) return;

        if (_startedAttack)
        {
            Machine.ChangeState(_wanderState);
        }
        else
        {
            AttackHandler.StartEntryAttack(_specialGrab);
            _startedAttack = true;
        }
    }

    public override State CreateCopy(IStateMachine machine) => new MonsterGrabState(machine, this);

}
