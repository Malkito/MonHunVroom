using LordBreakerX.AttackSystem;
using TMPro;
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
    private float _attackRate = 0.1f;

    [SerializeField]
    [Min(0)]
    private float _minAttackDistance = 2.5f;

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

    private float _attackDistance;

    public LaserEyes(AttackController controller) : base(controller)
    {
        _monsterAttack = controller.GetComponent<MonsterAttackController>();
        _monsterMovement = controller.GetComponent<MonsterMovementController>();
        _attackTimer = new Timer(_attackRate);
        _attackTimer.OnTimerFinished += () => _monsterAttack.RequestShootLaser(_laser, GetCenteredTargetPosition());
    }

    public override bool HasAttackFinished()
    {
        return _durationLeft <= 0;
    }

    public override void OnAttackFixedUpdate()
    {

    }

    public override void OnAttackUpdate()
    {
        Vector3 targetPosition = GetTargetPosition();

        if (Vector3.Distance(_monsterAttack.transform.position, targetPosition) > _attackDistance
            || IsBehindObject())
        {
            _monsterMovement.UpdateWalkAnimation(true);
            _monsterMovement.ChangeDestination(targetPosition);
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
        _attackDistance = Random.Range(_minAttackDistance, _maxAttackDistance);
        _monsterAttack.ChooseEye();
    }

    public override void OnStop()
    {
        _attackTimer.Reset();
    }

    public bool IsBehindObject()
    {
        Vector3 directionToTarget = (GetTargetPosition() - _monsterAttack.transform.position).normalized;
        float dot = Vector3.Dot(_monsterAttack.transform.forward, directionToTarget);
        return dot < 0;
    }

    public override Attack Clone(AttackController attackController)
    {
        LaserEyes attack = new LaserEyes(attackController);
        attack._duration = _duration;
        attack._attackRate = _attackRate;
        attack._minAttackDistance = _minAttackDistance;
        attack._maxAttackDistance = _maxAttackDistance;
        attack._laser = _laser;
        return attack;
    }
}
