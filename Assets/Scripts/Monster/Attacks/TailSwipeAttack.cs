using UnityEngine;

[CreateAssetMenu(menuName = "Monster/Attacks/Tail Swipe")]
public class TailSwipeAttack : Attack
{
    private const string TAIL_SWIPE_ANIMATION = "tail swipe";

    [SerializeField]
    [Header("Properties")]
    [Min(0)]
    private float _maxTailSwipeDistance = 1;

    private bool _swipeStarted = false;

    private Animator _animator;
    private MonsterMovementController _movementController;


    protected override void OnInilization(GameObject controlledObject)
    {
        _animator = controlledObject.GetComponent<Animator>();
        _movementController = controlledObject.GetComponent<MonsterMovementController>();
    }

    public override void OnStop()
    {
        _swipeStarted = false;
        _movementController.StopMovement();
    }

    public override void OnUpdate()
    {
        Vector3 attackPosition = TargetProvider.GetTargetPosiiton();
        Vector3 checkPosition = new Vector3(attackPosition.x, AttackHandler.transform.position.y, attackPosition.z);
        _movementController.ChangeDestination(attackPosition);

        if (!_swipeStarted && Vector3.Distance(AttackHandler.transform.position, checkPosition) <= _maxTailSwipeDistance)
        {
            _movementController.StopMovement();
            _animator.Play(TAIL_SWIPE_ANIMATION);
            _swipeStarted = true;
        }
    }

    public override bool CanFinishAttack()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        return _swipeStarted && !stateInfo.IsName(TAIL_SWIPE_ANIMATION);
    }
}
