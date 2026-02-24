using UnityEngine;

[CreateAssetMenu(menuName = MonsterState.CREATE_PATH + "Monster Agro State")]
public sealed class AgroState : MonsterState
{
    [SerializeField]
    [Range(1, 60)]
    private int _minAttacksPerformed = 2;

    [SerializeField]
    [Range(1, 60)]
    private int _maxAttacksPerformed = 3;

    [SerializeField]
    private MonsterState _rampageState;

    private int _attacksPerformed;
    private int _attacksNeeded;

    protected override void OnEnterState()
    {
        if (IsServer)
        {
            AttackHandler.AttackRandomPlayer();
            _attacksNeeded = Random.Range(_minAttacksPerformed, _maxAttacksPerformed + 1);
            _attacksPerformed = 0;
        }
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
                Machine.RequestTransitionTo(_rampageState);
            else
                AttackHandler.AttackRandomPlayer();
        }
        
    }
}
