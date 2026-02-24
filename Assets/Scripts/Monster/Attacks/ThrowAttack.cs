using LordBreakerX.AttackSystem;
using LordBreakerX.Utilities;
using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Throw Attack")]
public sealed class ThrowAttack : ScriptableAttack
{
    [SerializeField]
    [Min(0f)]
    private float _yOffset = 20f;

    [SerializeField]
    [Min(0)]
    private float _randomTargetRadius = 10f;

    [SerializeField]
    [Range(0f, 100f)]
    private float _throwTargetChance = 50;

    [SerializeField]
    [Min(0f)]
    private float _maxThrowDistance = 100f;

    [SerializeField]
    private float _maxPickupDistance = 100f;

    private AttackTarget _thrownTarget;

    private AttackTarget _positionTarget;

    private bool _reachedObject = false;
    private bool _thrownedObject = false;

    private MonsterMovementController _monsterMovement;

    public override void OnAttackCreation()
    {
        _monsterMovement = Controller.GetComponent<MonsterMovementController>();
    }

    public override void OnAttackStarted()
    {
        _reachedObject = false;
        _thrownedObject = false;

        // determines if throwing target
        if (Target.IsTargettingObject && Probability.IsSuccessful(_throwTargetChance))
        {
            _thrownTarget = Target;
            _positionTarget = TargetUtility.GetRandomTarget(Position, _randomTargetRadius, IgnoredLayers);
        }
        else
        {
            _positionTarget = Target;
            _thrownTarget = TargetUtility.GetRandomTarget(Position, _randomTargetRadius, IgnoredLayers);
        }


    }

    public override void OnAttackUpdate()
    {
        Vector3 throwObjectPosition = _thrownTarget.GetPosition();
        Vector3 finalPosition = _positionTarget.GetPosition();

        if (_reachedObject)
        {
            if (_monsterMovement.ReachedDestination(finalPosition, _maxThrowDistance))
            {
                _monsterMovement.StopMovement();
                ThrowObject(throwObjectPosition, finalPosition);
            }
            else if (_thrownTarget.IsTargettingObject)
            {
                _monsterMovement.ChangeDestination(finalPosition);
                _thrownTarget.Object.transform.position = Position + new Vector3(0, _yOffset);
            }
        }
        else
        {
            _monsterMovement.ChangeDestination(throwObjectPosition);
            _reachedObject = _monsterMovement.ReachedDestination(throwObjectPosition, _maxPickupDistance);
        }
    }

    private void ThrowObject(Vector3 throwObjectPosition, Vector3 finalPosition)
    {
        Vector3 direction = finalPosition - throwObjectPosition;
        float distance = Vector3.Distance(throwObjectPosition, finalPosition);

        if (_thrownTarget.IsTargettingObject)
        {
            Rigidbody rigidbody = _thrownTarget.Object.GetComponent<Rigidbody>();
            rigidbody.AddForce(distance * rigidbody.mass * direction, ForceMode.Force);
        }

        _thrownedObject = true;
    }

    public override bool HasAttackFinished()
    {
        return (_reachedObject && _thrownedObject) || !_thrownTarget.IsTargettingObject;
    }
}
