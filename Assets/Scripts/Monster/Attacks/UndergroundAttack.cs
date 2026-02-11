using LordBreakerX.AttackSystem;
using LordBreakerX.Utilities;
using UnityEngine;

[CreateAssetMenu()]
public class UndergroundAttack : ScriptableAttack
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

    public override void OnAttackCreation()
    {
        _monsterMovement = Controller.GetComponent<MonsterMovementController>();
        _durationTimer = new Timer(_attackDuration);
        _throwAttemptTimer = new Timer();
        _throwAttemptTimer.OnTimerFinished += AttemptThrow;
    }

    public override void OnAttackStarted()
    {
        _monsterMovement.UpdateWalkAnimation(true);
        _monsterMovement.SetUnderground(true);
        ResetThrowDelay();
        _durationTimer.Reset();
        Debug.Log("Attack Started");
    }

    private void ResetThrowDelay()
    {
        float throwDelay = Random.Range(_minThrowRate, _maxThrowRate);
        _throwAttemptTimer.SetDuration(throwDelay);
    }

    public override void OnAttackStopped()
    {
        _monsterMovement.UpdateWalkAnimation(false);
        _monsterMovement.SetUnderground(false);
        Debug.Log("Attack Ended");
    }

    private void AttemptThrow()
    {
        ResetThrowDelay();
        Probability.PerformChanceRolls(_maxThrowAmount, _throwChance, OnSucessfulThrow);
    }

    private void OnSucessfulThrow()
    {
        ThrowStrength throwStrength = new ThrowStrength(_minThrowSrength, _maxThrowSrength);
        Roubble roubble = _roubblePrefab.CreateRouble(Controller.transform.position, throwStrength);
        Controller.SpawnProjectile(roubble.gameObject);
    }

    public override bool HasAttackFinished()
    {
        return _durationTimer.IsComplete;
    }

    public override void OnAttackUpdate()
    {
        _monsterMovement.Wander();

        _throwAttemptTimer.Update();
        _durationTimer.Update(false);
    }
}
