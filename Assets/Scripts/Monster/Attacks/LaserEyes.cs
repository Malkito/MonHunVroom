using LordBreakerX.AttackSystem;
using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Laser Eyes Attack")]
public class LaserEyes : ScriptableAttack
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

    public override void OnAttackCreation()
    {
        _monsterAttack = Controller.GetComponent<MonsterAttackController>();
        _monsterMovement = Controller.GetComponent<MonsterMovementController>();
        _attackTimer = new Timer(_attackRate);
        _attackTimer.OnTimerFinished += () => _monsterAttack.ShootLaser(_laser, Target.GetCenteredPosition());
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
        Vector3 targetPosition = Target.GetPosition();

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

    public override void OnAttackStarted()
    {
        _durationLeft = _duration;
        _attackDistance = Random.Range(_minAttackDistance, _maxAttackDistance);
        _monsterAttack.ChooseEye();
    }

    public override void OnAttackStopped()
    {
        _attackTimer.Reset();
    }

    public bool IsBehindObject()
    {
        Vector3 directionToTarget = (Target.GetPosition() - _monsterAttack.transform.position).normalized;
        float dot = Vector3.Dot(_monsterAttack.transform.forward, directionToTarget);
        return dot < 0;
    }
}
