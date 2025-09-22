using LordBreakerX.AttackSystem;
using LordBreakerX.Utilities;
using UnityEngine;

[System.Serializable]
public class UndergroundAttack : Attack
{
    [Header("Throw Properties")]
    [Min(1)]
    [SerializeField]
    private float _minThrowSrength = 100;

    [SerializeField]
    [Min(1)]
    private float _maxThrowSrength = 200;

    [SerializeField]
    [Min(0)]
    private float _minThrowRate = 2;

    [SerializeField]
    [Min(0)]
    private float _maxThrowRate = 5;

    [SerializeField]
    [Range(0, 100)]
    private float _throwChance = 65;

    [SerializeField]
    [Range(0, 10)]
    private int _maxThrowAmount = 3;

    [SerializeField]
    private Roubble _roubblePrefab;

    [Header("Time Properties")]
    [SerializeField]
    private float _attackDuration;

    private MonsterMovementController _monsterMovement;

    private Timer _throwAttemptTimer;
    private Timer _durationTimer;

    public override void OnStart()
    {
        _monsterMovement.UpdateWalkAnimation(true);
        _monsterMovement.SetUnderground(true);
        ResetThrowDelay();
        _durationTimer.Reset();
    }

    private void ResetThrowDelay()
    {
        float throwDelay = Random.Range(_minThrowRate, _maxThrowRate);
        _throwAttemptTimer.SetDuration(throwDelay);
    }

    public override void OnStop()
    {
        _monsterMovement.UpdateWalkAnimation(false);
        _monsterMovement.SetUnderground(false);
    }


    protected override void OnInitilize(AttackController attackController)
    {
        _monsterMovement = attackController.GetComponent<MonsterMovementController>();
        _durationTimer = new Timer(_attackDuration);
        _throwAttemptTimer = new Timer();
        _throwAttemptTimer.OnTimerFinished += AttemptThrow;
    }

    private void AttemptThrow()
    {
        ResetThrowDelay();
        Probability.PerformChanceRolls(_maxThrowAmount, _throwChance, OnSucessfulThrow);
    }

    private void OnSucessfulThrow()
    {
        ThrowStrength throwStrength = new ThrowStrength(_minThrowSrength, _maxThrowSrength);
        _roubblePrefab.CreateRouble(Controller.transform.position, throwStrength);
    }

    public override bool HasAttackFinished()
    {
        return _durationTimer.IsComplete;
    }

    public override Attack Copy(AttackController attackController)
    {
        UndergroundAttack attack = new UndergroundAttack();
        attack._minThrowSrength = _minThrowSrength;
        attack._maxThrowSrength = _maxThrowSrength;
        attack._minThrowRate = _minThrowRate;
        attack._maxThrowRate = _maxThrowRate;
        attack._throwChance = _throwChance;
        attack._maxThrowAmount = _maxThrowAmount;
        attack._roubblePrefab = _roubblePrefab;
        attack._attackDuration = _attackDuration;
        return attack;
    }

    public override void OnAttackUpdate()
    {
        _monsterMovement.Wander();

        _throwAttemptTimer.Update();
        _durationTimer.Update(false);
    }
}
