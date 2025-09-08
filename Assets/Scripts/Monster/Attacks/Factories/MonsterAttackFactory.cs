using LordBreakerX.AttackSystem;
using UnityEngine;

[CreateAssetMenu(menuName = "Monster/Attack Factory")]
public class MonsterAttackFactory : AttackFactory
{
    [SerializeField]
    private LaserEyes _laserEyesTemplate;

    [SerializeField]
    private StompAttack _stompTemplate;

    [SerializeField]
    private TailSwipe _tailSwipeTemplate;

    public override Attack CreateAttack(AttackController attackController, AttackType attackType)
    {
        Attack selectedAttack;

        switch (attackType)
        {
            case AttackType.LaserEyes:
                selectedAttack = _laserEyesTemplate.Copy(attackController);
                break;
            case AttackType.StompAttack:
                selectedAttack = _stompTemplate.Copy(attackController);
                break;
            case AttackType.TailSwipe:
                selectedAttack = _tailSwipeTemplate.Copy(attackController);
                break;
            default:
                Debug.LogError($"{attackType} does not exist in the monster attack factory!");
                return null;
        }

        selectedAttack.Initilize(attackController);
        return selectedAttack;
    }
}
