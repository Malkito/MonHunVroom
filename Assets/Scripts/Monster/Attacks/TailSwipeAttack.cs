using LordBreakerX.AttackSystem;
using UnityEngine;

[CreateAssetMenu(menuName = "Monster/Attacks/Tail Swipe")]
public class TailSwipeAttack : ScriptableAttack
{
    private const string TAIL_SWIPE_ANIMATION = "tail swipe";

    public override void OnStart(AttackController attackHandler)
    {
        attackHandler.Movement.UpdateWalkAnimation(false);
        attackHandler.Movement.StopMovement();
        attackHandler.Animator.Play(TAIL_SWIPE_ANIMATION);
    }

    public override AttackProgress GetAttackProgress(AttackController attackHandler)
    {
        if (HasAnimationFinished(attackHandler.Animator)) return AttackProgress.FinishedAttack;
        else return AttackProgress.Attacking;
    }

    private bool HasAnimationFinished(Animator animator)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return !stateInfo.IsName(TAIL_SWIPE_ANIMATION);
    }

    public override void OnStop(AttackController attackHandler) { }

    public override void OnAttackFixedUpdate(AttackController attackHandler) { }

    public override void OnAttackUpdate(AttackController attackHandler) { }

    public override void OnPreperationFixedUpdate(AttackController attackHandler) { }

    public override void OnPreperationUpdate(AttackController attackHandler) { }
}
