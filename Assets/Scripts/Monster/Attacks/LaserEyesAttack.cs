using UnityEngine;

/*
 *  Make the following changes: 
 *  1. monster should move to a max shooting distance if father then the distance)
 *  2. if the monster isn't facing the target then they should make themselves face the target.
 */

[CreateAssetMenu(menuName = "Monster/Attacks/Laser Eyes")]
public class LaserEyesAttack : Attack
{
    [SerializeField]
    [Header("Timing Properties")]
    [Min(0f)]
    private float _duration;

    [SerializeField]
    [Header("Attacking Properties")]
    [Min(0)]
    private float _attackRate = 0.5f;

    [SerializeField]
    [Min(0)]
    private float _maxAttackDistance = 5.0f;

    [SerializeField]
    [Header("Prefab Properties")]
    private Laser _laser;

    private float _durationLeft;

    private Timer _attackTimer;

    private MonsterAttackController _monsterAttack;

    private MonsterMovementController _monsterMovement;

    protected override void OnInilization(GameObject controlledObject)
    {
        base.OnInilization(controlledObject);
        _monsterAttack = controlledObject.GetComponent<MonsterAttackController>();
        _monsterMovement = controlledObject.GetComponent<MonsterMovementController>();

        _attackTimer = new Timer(_attackRate);
        _attackTimer.OnTimerFinished += () => _monsterAttack.RequestShootLaser(_laser, TargetPosition);
    }

    public override void OnStart()
    {
        _durationLeft = _duration;
    }

    public override void OnStop()
    {
        _attackTimer.Reset();
    }

    public override void OnUpdate()
    {
        if (Vector3.Distance(AttackHandler.transform.position, OffsettedTargetPosition) > _maxAttackDistance)
        {
            _monsterMovement.UpdateWalkAnimation(true);
            _monsterMovement.ChangeDestination(OffsettedTargetPosition);
        }
        else if (IsBehindObject())
        {
            _monsterMovement.UpdateWalkAnimation(true);
            _monsterMovement.ChangeDestination(OffsettedTargetPosition);
        }
        else
        {
            _monsterMovement.UpdateWalkAnimation(false);
            _monsterMovement.StopMovement();
            _attackTimer.Update();
            _durationLeft -= Time.deltaTime;
        }
    }

    public bool IsBehindObject()
    {
        Vector3 directionToTarget = (TargetPosition - AttackHandler.transform.position).normalized;
        float dot = Vector3.Dot(AttackHandler.transform.forward, directionToTarget);
        return dot < 0;
    }

    public override bool CanFinishAttack()
    {
        return _durationLeft <= 0;
    }
}
