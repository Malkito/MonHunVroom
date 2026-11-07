using LordBreakerX.NetcodeStates;
using LordBreakerX.States;

public abstract class MonsterState : NetworkState
{
    protected const string DEAD_ANIMATION_VARIABLE = "dead";

    public MonsterMovementController MovementHandler { get; private set; }
    public MonsterAttackController AttackHandler { get; private set; }

    protected MonsterState() : base()
    {

    }

    protected MonsterState(IStateMachine machine, MonsterState toCopy) : base(machine, toCopy)
    {
        MovementHandler = machine.ParentObject.GetComponent<MonsterMovementController>();
        AttackHandler = machine.ParentObject.GetComponent<MonsterAttackController>();
    }
}
