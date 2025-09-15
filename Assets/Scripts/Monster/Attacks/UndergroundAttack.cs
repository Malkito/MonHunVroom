using LordBreakerX.AttackSystem;
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

    [SerializeField]
    private float _attackDuration;

    private float _throwDelay;

    private float _durationLeft;

    private MonsterMovementController _monsterMovement;

    protected override void OnInitilize(AttackController attackController)
    {
        _monsterMovement = attackController.GetComponent<MonsterMovementController>();
    }

    public override bool HasAttackFinished()
    {
        return _durationLeft <= 0;
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

    public override void OnStart()
    {
        _monsterMovement.UpdateWalkAnimation(true);
        _monsterMovement.SetUnderground(true);
        _durationLeft = _attackDuration;
        ResetThrowDelay();
    }

    private void ResetThrowDelay()
    {
        _throwDelay = Random.Range(_minThrowRate, _maxThrowRate);
    }

    public override void OnStop()
    {
        _monsterMovement.UpdateWalkAnimation(false);
        _monsterMovement.SetUnderground(false);
    }

    public override void OnAttackUpdate()
    {
        _monsterMovement.Wander();

        _throwDelay -= Time.deltaTime;
        _durationLeft -= Time.deltaTime;

        if (_throwDelay <= 0)
        {
            ResetThrowDelay();

            for (int x = 0; x < _maxThrowAmount; x++)
            {
                float chance = Random.Range(1.0f, 100);
                if (chance <= _throwChance)
                {
                    _roubblePrefab.CreateRouble(Controller.transform.position, _minThrowSrength, _maxThrowSrength);
                }
            }
        }
    }

}
