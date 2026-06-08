using LordBreakerX.Attributes;
using LordBreakerX.Utilities;
using UnityEngine;

[CreateAssetMenu(menuName = MonsterState.CREATE_PATH + "Monster Attack State")]
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
    private MonsterWanderState _wanderState;

    private int _attacksCompleted;
    private int _attacksNeeded;

    private void OnValidate()
    {
        _maxAttacks = Mathf.Max(_minAttacks, _maxAttacks);
    }

    protected override void OnEnterState()
    {
        _attacksCompleted = -1;
        _attacksNeeded = Random.Range(_minAttacks, _maxAttacks + 1);
    }

    protected override void OnUpdateState()
    {
        if (AttackHandler.IsAttacking || !IsServer) 
            return;

        _attacksCompleted++;

        if (_attacksCompleted >= _attacksNeeded)
        {
            Machine.RequestTransitionTo(_wanderState);
        }
        else if (TryUpdateTarget())
        {
            AttackHandler.StartRandomAttack();
        }
        else
        {
            Machine.RequestTransitionTo(_wanderState);
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
}
