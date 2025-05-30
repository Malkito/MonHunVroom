using UnityEngine;

[CreateAssetMenu(menuName = "Monster/Attacks/Tail Swipe")]
public class TailSwipeAttack : MonsterAttack
{
    [SerializeField]
    [Header("Properties")]
    [Min(0)]
    private float _maxTailSwipeDistance = 1;

    private bool _swipeStarted = false;

    public override void OnStop()
    {
        _swipeStarted = false;
        Monster.StopMovement();
    }

    public override void OnUpdate()
    {
        Vector3 attackPosition = GetAttackPosition();
        Vector3 checkPosition = new Vector3(attackPosition.x, Parent.transform.position.y, attackPosition.z);
        Monster.ChangeDestination(attackPosition);

        if (!_swipeStarted && Vector3.Distance(Parent.transform.position, checkPosition) <= _maxTailSwipeDistance)
        {
            Monster.StopMovement();
            Monster.TailSwipe();
            _swipeStarted = true;
        }
    }

    public override bool CanFinishAttack()
    {
        return _swipeStarted && Monster.TailSwipeFinished();
    }
}
