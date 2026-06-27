using LordBreakerX.AttackSystem;
using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Special Grab Attack")]
public class SpecialGrabAttack : ScriptableAttack
{
    [SerializeField]
    private float _throwRadius = 100f;

    [SerializeField]
    [Min(0f)]
    private float _maxThrowDistance = 100f;

    private AttackTarget _positionTarget;

    private bool _thrownedObject = false;
    private bool _grabObject = false;

    private MonsterMovementController _monsterMovement;
    private MonsterAttackController _monsterAttack;

    public override void OnAttackCreation()
    {
        _monsterMovement = Controller.GetComponent<MonsterMovementController>();
        _monsterAttack = Controller.GetComponent<MonsterAttackController>();
    }

    public override void OnAttackStarted()
    {
        _thrownedObject = false;
        _grabObject = false;
        _positionTarget = TargetUtility.GetRandomTarget<dealDamage>(Controller.transform.position, _throwRadius, Controller.IgnoredLayers);

        if (Target.IsTargettingObject)
        {
            Target.TargetTransform.position = _monsterAttack.ThrowPoint.position;
        }
    }

    public override void OnAttackUpdate()
    {
        Vector3 finalPosition = _positionTarget.GetPosition();

        if (!_grabObject)
        {
            Target.TargetTransform.position = _monsterAttack.ThrowPoint.position;
            _grabObject = true;
        }
        else if (_monsterMovement.ReachedDestination(finalPosition, _maxThrowDistance))
        {
            _monsterMovement.StopMovement();
            ThrowObject(Target.GetPosition(), finalPosition);
        }
        else if (Target.IsTargettingObject)
        {
            Target.TargetTransform.position = _monsterAttack.ThrowPoint.position;
            _monsterMovement.ChangeDestination(finalPosition);
        }
    }

    private void ThrowObject(Vector3 startPosition, Vector3 finalPosition)
    {
        Vector3 direction = finalPosition - startPosition;
        float distance = Vector3.Distance(startPosition, finalPosition);

        if (Target.IsTargettingObject)
        {
            Rigidbody rigidbody = Target.Object.GetComponent<Rigidbody>();
            rigidbody.AddForce(distance * rigidbody.mass * direction, ForceMode.Force);
        }

        _thrownedObject = true;
    }

    public override bool HasAttackFinished()
    {
        return _thrownedObject && _grabObject;
    }

}
