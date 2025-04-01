using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Monster/Laser Eyes")]
public class LaserEyesAttack : MonsterAttack
{
    [SerializeField]
    private Laser _laser;

    public override void FinishAbility()
    {
         
    }

    public override void FixedUpdate()
    {

    }

    public override void TargetAttack(Vector3 targetPosition)
    {
        Transform eye = Monster.GetRandomEye();
        Laser.Create(_laser, eye.position, targetPosition);
    }

    public override void Update()
    {

    }
}
