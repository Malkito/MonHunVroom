using LordBreakerX.States;
using LordBreakerX.Utilities;
using UnityEngine;

[CreateStateMenu("MonsterAttack", "Monster Attack State", "Monster States")]
[System.Serializable]
public class MonsterAttackState : MonsterState
{
    [Header("Attacks")]
    [Min(1)]
    [SerializeField]
    private int _minAttacks = 1;

    [SerializeField]
    [Min(1)]
    private int _maxAttacks = 3;

    [Header("Chance")]
    [SerializeField]
    [Range(0.0f, 100.0f)]
    private float _playerAttackChance = 80.0f;

    [Header("States")]
    [SerializeField]
    private StateReference _wanderState;

    private int _attacksCompleted;
    private int _attacksNeeded;

    public MonsterAttackState() : base()
    {

    }

    public MonsterAttackState(IStateMachine machine, MonsterAttackState toCopy) : base(machine, toCopy)
    {
        _minAttacks = toCopy._minAttacks;
        _maxAttacks = toCopy._maxAttacks;

        _playerAttackChance = toCopy._playerAttackChance;

        _wanderState = toCopy._wanderState;

        _attacksCompleted = 0;
        _attacksNeeded = 0;
    }

    public override void OnEnter()
    {
        _attacksCompleted = -1;
        _attacksNeeded = Random.Range(_minAttacks, _maxAttacks);
    }

    public override void OnUpdate()
    {
        if (AttackHandler.IsAttacking || !IsServer) return;

        _attacksCompleted++;

        if (_attacksCompleted >= _attacksNeeded)
        {
            Machine.ChangeState(_wanderState);
        }
        else if (TryUpdateTarget())
        {
            AttackHandler.StartRandomAttack();
        }
        else
        {
            Machine.ChangeState(_wanderState);
        }
    }

    private bool TryUpdateTarget()
    {
        if (Probability.IsSuccessful(_playerAttackChance))
        {
            if (AttackHandler.TryTargetPlayer())
                return true;
        }

        return AttackHandler.TryTargetRandomObject<dealDamage>();
    }

    public override State CreateCopy(IStateMachine machine) => new MonsterAttackState(machine, this);
}
