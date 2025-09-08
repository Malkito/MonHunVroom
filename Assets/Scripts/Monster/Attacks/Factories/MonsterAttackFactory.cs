using LordBreakerX.AttackSystem;
using UnityEngine;

[CreateAssetMenu(menuName = "Monster/Attack Factory")]
public class MonsterAttackFactory : AttackFactory
{
    [SerializeField]
    private LaserEyes _laserEyesTemplate = new LaserEyes();

    [SerializeField]
    private StompAttack _stompTemplate = new StompAttack();

    [SerializeField]
    private TailSwipe _tailSwipeTemplate = new TailSwipe();

    [SerializeField]
    private BlackholeAttack _blackholeTemplate = new BlackholeAttack();

    [SerializeField]
    private UndergroundAttack _undergroundAttackTemplate = new UndergroundAttack();

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
            case AttackType.Blackhole:
                selectedAttack = _blackholeTemplate.Copy(attackController);
                break;
            case AttackType.Underground:
                selectedAttack = _undergroundAttackTemplate.Copy(attackController);
                break;
            default:
                Debug.LogError($"{attackType} does not exist in the monster attack factory!");
                return null;
        }

        selectedAttack.Initilize(attackController);
        return selectedAttack;
    }
}
