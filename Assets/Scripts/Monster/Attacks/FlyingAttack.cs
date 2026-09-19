using LordBreakerX.AttackSystem;
using LordBreakerX.Stats;
using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Flying Attack")]
public sealed class FlyingAttack : ScriptableAttack
{
    [SerializeField]
    [Header("Flying Properties")]
    [Min(0)]
    private float _flightHeight = 10;

    [SerializeField]
    [Min(1)]
    private float _flySpeed = 1;

    [SerializeField]
    [Header("Timing Properties")]
    [Min(0)]
    private float _attackDuration = 10;

    [Header("Secondary Attack Properties")]
    [SerializeField]
    private ScriptableAttackTable _scriptableAttackTable;

    private MonsterMovementController _movementController;

    private float _currentDuration;

    private StatHolder _statHolder;

    private ScriptableAttack _subAttack;

    public override void OnAttackCreation()
    {
        _movementController = Controller.GetComponent<MonsterMovementController>();
        _statHolder = Controller.GetComponent<StatHolder>();

        ScriptableAttackTable table = _scriptableAttackTable.Clone(Controller);
        _scriptableAttackTable = table;
    }

    public override bool HasAttackFinished()
    {
        return base.HasAttackFinished() || _currentDuration <= 0;
    }

    public override bool CanUseAttack()
    {
        return base.CanUseAttack();
    }

    public override void OnAttackStarted()
    {
        _flySpeed = _statHolder.GetFloat("Fly-Speed");
        _attackDuration = _statHolder.GetFloat("Flying-Attack-Duration");

        _currentDuration = _attackDuration;
        _subAttack = _scriptableAttackTable.GetRandomAttack();

        _subAttack.OnAttackStarted();
    }

    public override void OnAttackStopped()
    {
        _subAttack.OnAttackStopped();
        _movementController.LandFromFlight();
    }

    public override void OnAttackUpdate()
    {
        if (!_movementController.IsFlying(_flightHeight))
        {
            _movementController.StopMovement();
            _movementController.Fly(_flightHeight, _flySpeed);
            return;
        }

        _currentDuration -= Time.deltaTime;

        _subAttack.OnAttackUpdate();

        if (_subAttack.HasAttackFinished())
        {
            _subAttack = _scriptableAttackTable.GetRandomAttack();
        }
    }

    public override void OnAttackFixedUpdate()
    {
        _subAttack.OnAttackFixedUpdate();
    }

}
