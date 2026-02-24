using LordBreakerX.States.Networked;
using UnityEngine;

[CreateAssetMenu(menuName = MonsterState.CREATE_PATH + "Monster Dead State")]
public sealed class DeadState : MonsterState
{
    protected override void OnEnterState()
    {
        if (IsServer)
        {
            MovementHandler.StopMovement();
        }

        MovementHandler.MonsterAnimator.SetBool(DEAD_ANIMATION_VARIABLE, true);
        MovementHandler.MonsterAnimator.SetBool(MonsterMovementController.WALK_ANIMATION_VARIABLE, false);
    }

    protected override void OnUpdateState()
    {
        if (IsServer)
        {
            MovementHandler.StopMovement();
            AttackHandler.StopAttack();
        }
    }
}
