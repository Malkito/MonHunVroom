using LordBreakerX.AttackSystem;
using LordBreakerX.Tables;
using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Flying Attack")]
public class FlyingAttack : ScriptableAttack
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

    private WeightTable<ScriptableAttack> _internalAttackTable = new WeightTable<ScriptableAttack>();
    private ScriptableAttack _subAttack;

    public override void OnAttackCreation()
    {
        _movementController = Controller.GetComponent<MonsterMovementController>();
    }

    public override bool HasAttackFinished()
    {
        return _currentDuration <= 0;
    }

    public override void OnAttackStarted()
    {
        _currentDuration = _attackDuration;
        _subAttack = _internalAttackTable.GetRandomEntry();

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
            _subAttack = _internalAttackTable.GetRandomEntry();
        }
    }

    public override void OnAttackFixedUpdate()
    {
        _subAttack.OnAttackFixedUpdate();
    }

}
