using LordBreakerX.AttackSystem;
using UnityEngine;


[CreateAssetMenu(menuName = "Attacks/Push Attack")]
public class PushAttack : ScriptableAttack
{
    [SerializeField]
    [Min(0)]
    private float _pushForce = 100f;

    [SerializeField]
    private float _pushDamage = 1f;

    [SerializeField]
    [Min(0)]
    private float _maxPushDistance = 30.0f;

    private bool _pushedObject = false;

    private MonsterMovementController _monsterMovement;

    public override void OnAttackCreation()
    {
        _monsterMovement = Controller.GetComponent<MonsterMovementController>();
    }

    public override bool CanUseAttack()
    {
        return Target.IsTargettingObject;
    }

    public override bool HasAttackFinished()
    {
        return _pushedObject;
    }

    public override void OnAttackStarted()
    {
        _pushedObject = false;
    }

    public override void OnAttackUpdate()
    {
        if (!_pushedObject)
        {
            Vector3 targetPosition = Target.GetPosition();
            _monsterMovement.ChangeDestination(targetPosition);

            if (_monsterMovement.ReachedDestination(_maxPushDistance))
            {
                PushObject();
            }
        }
    }

    private void PushObject()
    {
        _pushedObject = true;

        Rigidbody targetRigidBody = Target.TargetObject.GetComponent<Rigidbody>();

        Vector3 pushDirection = (Target.GetPosition() - Position).normalized;

        if (targetRigidBody != null)
        {
            targetRigidBody.AddForce(_pushForce * targetRigidBody.mass * pushDirection, ForceMode.Force);
        }

        dealDamage targetHealth = Target.TargetObject.GetComponent<dealDamage>();
        if (targetHealth != null)
        {
            targetHealth.dealDamage(_pushDamage, Color.red, Controller.gameObject);
        }
    }
}
