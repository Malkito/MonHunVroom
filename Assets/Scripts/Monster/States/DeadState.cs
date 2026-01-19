using LordBreakerX.States;
using UnityEngine;

[CreateAssetMenu(fileName = MonsterStates.DEAD, menuName = "Monster/States/Dead State")]
public class DeadState : BaseState
{
    public const string DEAD_ANIMATION_VARIABLE = "dead";

    private MonsterMovementController _movementController;
    private MonsterAttackController _attackController;

    public override string ID => MonsterStates.DEAD;

    protected override void OnInitilization()
    {
        _movementController = StateObject.GetComponent<MonsterMovementController>();
        _attackController = StateObject.GetComponent<MonsterAttackController>();
    }

    public override void Enter()
    {
        _movementController.StopMovement();
        _movementController.UpdateWalkAnimation(false);
        _movementController.MonsterAnimator.SetBool(DEAD_ANIMATION_VARIABLE, true);
        _movementController.MonsterAnimator.SetBool(MonsterMovementController.WALK_ANIMATION_VARIABLE, false);
    }

    public override void Update()
    {
        _movementController.StopMovement();
        _attackController.StopAttack();
    }
}
