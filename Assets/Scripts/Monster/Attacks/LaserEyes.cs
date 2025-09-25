using LordBreakerX.AttackSystem;
using UnityEngine;

[System.Serializable]
public class LaserEyes : Attack
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

    public override bool HasAttackFinished()
    {
        return _durationLeft <= 0;
    }

    public override void OnAttackFixedUpdate()
    {

    }

    public override void OnAttackUpdate()
    {
        if (Vector3.Distance(_monsterAttack.transform.position, TargetPosition) > _maxAttackDistance)
        {
            _monsterMovement.UpdateWalkAnimation(true);
            _monsterMovement.ChangeDestination(TargetPosition);
        }
        else if (IsBehindObject())
        {
            _monsterMovement.UpdateWalkAnimation(true);
            _monsterMovement.ChangeDestination(TargetPosition);
        }
        else
        {
            _monsterMovement.UpdateWalkAnimation(false);
            _monsterMovement.StopMovement();
            _attackTimer.Update();
            _durationLeft -= Time.deltaTime;
        }
    }

    public override void OnStart()
    {
        _durationLeft = _duration;
    }

    public override void OnStop()
    {
        _attackTimer.Reset();
    }

    public bool IsBehindObject()
    {
        Vector3 directionToTarget = (TargetPosition - _monsterAttack.transform.position).normalized;
        float dot = Vector3.Dot(_monsterAttack.transform.forward, directionToTarget);
        return dot < 0;
    }

    public override Attack Copy(AttackController attackController)
    {
        LaserEyes attack = new LaserEyes();
        attack._duration = _duration;
        attack._attackRate = _attackRate;
        attack._maxAttackDistance = _maxAttackDistance;
        attack._laser = _laser;
        return attack;
    }

    protected override void OnInitilize(AttackController attackController)
    {
        _monsterAttack = attackController.GetComponent<MonsterAttackController>();
        _monsterMovement = attackController.GetComponent<MonsterMovementController>();
        _attackTimer = new Timer(_attackRate);
        _attackTimer.OnTimerFinished += () => _monsterAttack.RequestShootLaser(_laser, TargetPosition);
    }
}
