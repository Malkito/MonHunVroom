using LordBreakerX.AttackSystem;
using UnityEngine;

[CreateAssetMenu(menuName = "Monster/Attacks/Tail Swipe Creator")]
public class TailSwipeCreator : AttackCreator
{
    public override Attack Create(AttackController controller)
    {
        return new TailSwipe(controller);
    }
}
