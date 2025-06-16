using LordBreakerX.States;
using UnityEngine;

[CreateAssetMenu(fileName = MonsterStates.DEAD, menuName = MonsterStates.CreatePaths.DEAD)]
public class DeadState : BaseState
{
    public const string DEAD_ANIMATION_VARIABLE = "dead";

    private MonsterMovementController _movementController;
    private Animator _animator;

    public override string ID => MonsterStates.DEAD;

    protected override void OnInitilization()
    {
        _movementController = StateObject.GetComponent<MonsterMovementController>();
        _animator = StateObject.GetComponent<Animator>();
    }

    public override void Enter()
    {
        _movementController.StopMovement();
        _movementController.UpdateWalkAnimation(false);
        _animator.SetBool(DEAD_ANIMATION_VARIABLE, true);
        _animator.SetBool(MonsterMovementController.WALK_ANIMATION_VARIABLE, false);
    }
}
