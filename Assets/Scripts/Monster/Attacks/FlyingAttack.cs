using LordBreakerX.AttackSystem;
using LordBreakerX.Tables;
using UnityEngine;

[System.Serializable]
public class FlyingAttack : Attack
{
    [SerializeField]
    [Header("Flying Properties")]
    [Min(0)]
    private float _minHeight = 10;

    [SerializeField]
    [Min(0)]
    private float _maxHeight = 20;

    [SerializeField]
    [Min(0)]
    private float _flySpeed = 1;

    [SerializeField]
    [Header("Timing Properties")]
    [Min(0)]
    private float _duration = 10;

    [SerializeField]
    [Min(0)]
    private float _maxHeightTime = 5;

    [Header("Secondary Attack Properties")]
    [SerializeField]
    private ScriptableAttackTable _scriptableAttackTable;

    private MonsterAttackController _controller;

    private MonsterMovementController _movementController;

    private Vector3 _flyHeight;

    private float _flightTime;

    private float _currentDuration;

    private Animator _animator;


    private WeightTable<Attack> _internalAttackTable = new WeightTable<Attack>();
    private Attack _subAttack;

    public FlyingAttack(AttackController controller) : base(controller)
    {
        _controller = controller.GetComponent<MonsterAttackController>();
        _movementController = controller.GetComponent<MonsterMovementController>();
        _animator = controller.GetComponent<Animator>();
    }

    public override bool HasAttackFinished()
    {
        return _currentDuration <= 0;
    }

    private void RandomFlightHeight()
    {
        float height = Random.Range(_minHeight, _maxHeight);
        _flyHeight = _controller.Model.transform.position + new Vector3(0, height);
        _flightTime = Random.Range(0, _maxHeightTime);
    }

    public override void OnStart()
    {
        _animator.enabled = false;

        RandomFlightHeight();
        _currentDuration = _duration;
        _subAttack = _internalAttackTable.GetRandomEntry();

        _subAttack.OnStart();
    }

    public override void OnStop()
    {
        _controller.Model.transform.position = _controller.transform.position;
        _subAttack.OnStop();
        _animator.enabled = true;
    }

    public override void OnAttackUpdate()
    {
        Vector3 modelPosition = _controller.Model.transform.position;
        _controller.Model.transform.position = Vector3.MoveTowards(modelPosition, _flyHeight, _flySpeed * Time.deltaTime);

        _flightTime -= Time.deltaTime;

        if (_flightTime <= 0)
        {
            RandomFlightHeight();
        }

            _movementController.Wander();

        _subAttack.OnAttackUpdate();
    }

    public override void OnAttackFixedUpdate()
    {
        _subAttack.OnAttackFixedUpdate();
    }

    public override Attack Clone(AttackController attackController)
    {
        FlyingAttack copy = new FlyingAttack(attackController);
        copy._minHeight = _minHeight;
        copy._maxHeight = _maxHeight;
        copy._flySpeed = _flySpeed;
        copy._duration = _duration;
        copy._scriptableAttackTable = _scriptableAttackTable;
        copy._internalAttackTable = _scriptableAttackTable.CreateTable(attackController);
        return copy;
    }
}
