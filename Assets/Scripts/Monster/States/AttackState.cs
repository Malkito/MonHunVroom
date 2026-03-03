using LordBreakerX.Utilities;
using UnityEngine;

[CreateAssetMenu(menuName = MonsterState.CREATE_PATH + "Monster Attack State")]
public sealed class AttackState : MonsterState
{
    [SerializeField]
    [Range(1, 60)]
    private int _minAttacksPerformed = 2;

    [SerializeField]
    [Range(1, 60)]
    private int _maxAttacksPerformed = 3;

    [SerializeField]
    [Range(0.0f, 100.0f)]
    private float _playerAttackChance = 80.0f;

    [SerializeField]
    [Min(0.0f)]
    private float _attackRadius = 30.0f;

    [SerializeField]
    private MonsterState _wanderState;

    private bool _isAttackingPlayers;

    private int _attacksPerformed;
    private int _attacksNeeded;

    protected override void OnEnterState()
    {
        if (IsServer)
        {
            _attacksNeeded = Random.Range(_minAttacksPerformed, _maxAttacksPerformed + 1);
            _attacksPerformed = 0;

            _isAttackingPlayers = Probability.IsSuccessful(_playerAttackChance);

            Attack();
        }
    }

    private void Attack() 
    {
        if (_isAttackingPlayers) AttackHandler.AttackRandomPlayer();
        else AttackHandler.AttackRandomObject<dealDamage>();
    }

    protected override void OnExitState()
    {
        if (IsServer)
        {
            MovementHandler.StopMovement();
            AttackHandler.StopAttack();
        }
    }

    protected override void OnUpdateState()
    {
        if (!AttackHandler.IsAttacking && IsServer)
        {
            _attacksPerformed += 1;

            if (_attacksPerformed >= _attacksNeeded)
                Machine.RequestTransitionTo(_wanderState);
            else
                Attack();
        }
    }


}
