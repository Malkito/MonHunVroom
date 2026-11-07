using LordBreakerX.States;

[CreateStateMenu("MonsterDead", "Monster Dead State", "Monster States")]
[System.Serializable]
public sealed class DeadState : MonsterState
{
    public DeadState() : base()
    {

    }

    public DeadState(IStateMachine machine, DeadState toCopy) : base(machine, toCopy)
    {

    }

    public override void OnEnter()
    {
        if (IsServer)
        {
            MovementHandler.StopMovement();
        }

        MovementHandler.MonsterAnimator.SetBool(DEAD_ANIMATION_VARIABLE, true);
        MovementHandler.MonsterAnimator.SetBool(MonsterMovementController.WALK_ANIMATION_VARIABLE, false);
    }

    public override void OnUpdate()
    {
        if (IsServer)
        {
            MovementHandler.StopMovement();
            AttackHandler.StopAttack();
        }
    }

    public override State CreateCopy(IStateMachine machine) => new DeadState(machine, this);
}
