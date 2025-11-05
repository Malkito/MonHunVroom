using LordBreakerX.AttackSystem;
using UnityEngine;

[CreateAssetMenu(menuName = "Monster/Attacks/Tail Swipe")]
public class TailSwipeCreator : AttackCreator
{
    public override Attack Create(AttackController controller)
    {
        return new TailSwipe(controller);
    }
}
