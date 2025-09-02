using LordBreakerX.AttackSystem;
using UnityEngine;

[CreateAssetMenu(menuName = "Monster/Attacks/Laser Eyes")]
public class LaserEyesCreator : AttackCreator
{
    [SerializeField]
    private LaserEyes _laserEyes;

    public override Attack Create(AttackController controller)
    {
        return new LaserEyes(controller, _laserEyes);
    }
}
