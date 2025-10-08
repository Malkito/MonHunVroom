using LordBreakerX.AttackSystem;
using UnityEngine;

public class TailSwipe : Attack
{
    private const string TAIL_SWIPE_ANIMATION = "tail swipe";

    private MonsterMovementController _monsterMovement;

    private Animator _animator;

    public TailSwipe(AttackController controller) : base(controller)
    {
        _monsterMovement = controller.GetComponent<MonsterMovementController>();
        _animator = controller.GetComponent<Animator>();
    }

    public override void OnStart()
    {
        _monsterMovement.UpdateWalkAnimation(false);
        _monsterMovement.StopMovement();
        _animator.Play(TAIL_SWIPE_ANIMATION);
    }

    public override bool HasAttackFinished()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        return !stateInfo.IsName(TAIL_SWIPE_ANIMATION);
    }


    public override void OnStop() { }

    public override void OnAttackFixedUpdate() { }

    public override void OnAttackUpdate() { }

    public override Attack Clone(AttackController attackController)
    {
        return new TailSwipe(attackController);
    }
}
