using UnityEngine;

[CreateAssetMenu(menuName = "Monster/Attacks/Tail Swipe")]
public class TailSwipeAttack : Attack
{
    private const string TAIL_SWIPE_ANIMATION = "tail swipe";

    [SerializeField]
    [Header("Properties")]
    [Min(0)]
    private float _maxTailSwipeDistance = 1;

    private Animator _animator;
    private MonsterMovementController _movementController;

    protected override void OnInilization(GameObject controlledObject)
    {
        _animator = controlledObject.GetComponent<Animator>();
        _movementController = controlledObject.GetComponent<MonsterMovementController>();
    }

    public override void OnStart()
    {
        _movementController.UpdateWalkAnimation(false);

        _movementController.StopMovement();
        _animator.Play(TAIL_SWIPE_ANIMATION);
    }

    public override bool CanFinishAttack()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        return !stateInfo.IsName(TAIL_SWIPE_ANIMATION);
    }
}
