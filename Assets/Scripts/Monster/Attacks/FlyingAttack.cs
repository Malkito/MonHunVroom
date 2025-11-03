using LordBreakerX.AttackSystem;
using LordBreakerX.Tables;
using UnityEngine;

[System.Serializable]
public class FlyingAttack : Attack
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

    private WeightTable<Attack> _internalAttackTable = new WeightTable<Attack>();
    private Attack _subAttack;

    public FlyingAttack(AttackController controller) : base(controller)
    {
        _movementController = controller.GetComponent<MonsterMovementController>();
    }

    public override bool HasAttackFinished()
    {
        return _currentDuration <= 0;
    }

    public override void OnStart()
    {
        _currentDuration = _attackDuration;
        _subAttack = _internalAttackTable.GetRandomEntry();

        _subAttack.OnStart();
    }

    public override void OnStop()
    {
        _subAttack.OnStop();
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
            _subAttack = _internalAttackTable.GetRandomEntry();
        }
    }

    public override void OnAttackFixedUpdate()
    {
        _subAttack.OnAttackFixedUpdate();
    }

    public override Attack Clone(AttackController attackController)
    {
        FlyingAttack copy = new FlyingAttack(attackController);
        copy._flightHeight = _flightHeight;
        copy._flySpeed = _flySpeed;
        copy._attackDuration = _attackDuration;
        copy._scriptableAttackTable = _scriptableAttackTable;
        copy._internalAttackTable = _scriptableAttackTable.CreateTable(attackController);
        return copy;
    }

}
