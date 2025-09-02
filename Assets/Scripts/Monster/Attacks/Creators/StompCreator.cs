using LordBreakerX.AttackSystem;
using UnityEngine;

[CreateAssetMenu(menuName = "Monster/Attacks/Stomp")]
public class StompCreator : AttackCreator
{
    [SerializeField]
    private StompAttack _stomp;

    public override Attack Create(AttackController controller)
    {
        return new StompAttack(controller, _stomp);
    }
}
