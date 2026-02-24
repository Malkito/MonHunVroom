using LordBreakerX.States.Networked;

public abstract class MonsterState : NetworkScriptableState
{
    protected const string DEAD_ANIMATION_VARIABLE = "dead";

    public MonsterMovementController MovementHandler { get; private set; }
    public MonsterAttackController AttackHandler { get; private set; }

    protected sealed override void OnCreateState()
    {
        MovementHandler = MachineObject.GetComponent<MonsterMovementController>();
        AttackHandler = MachineObject.GetComponent<MonsterAttackController>();
        OnInitlizedState();
    }

    protected virtual void OnInitlizedState()
    {
        
    }
}
