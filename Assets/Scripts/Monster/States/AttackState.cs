using LordBreakerX.States;
using LordBreakerX.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = MonsterStates.ATTACK, menuName = "Monster/States/Attack State")]
public class AttackState : BaseState
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
    private LayerMask _ignoredLayers;

    public override string ID => MonsterStates.ATTACK;

    private MonsterAttackController _monsterAttack;
    private MonsterMovementController _monsterMovement;

    private bool _isAttackingPlayers;

    private int _attacksPerformed;
    private int _attacksNeeded;

    protected override void OnInitilization()
    {
        _monsterAttack = StateObject.GetComponent<MonsterAttackController>();
        _monsterMovement = StateObject.GetComponent<MonsterMovementController>();
    }

    public override void Enter()
    {
        _attacksNeeded = Random.Range(_minAttacksPerformed, _maxAttacksPerformed + 1);
        _attacksPerformed = 0;

        _isAttackingPlayers = Probability.IsSuccessful(_playerAttackChance);

        Attack();
    }

    private void Attack() 
    {
        if (_isAttackingPlayers) _monsterAttack.AttackRandomPlayer();
        else _monsterAttack.AttackRandomObject(_attackRadius, _ignoredLayers);
    }

    public override void Exit()
    {
        _monsterMovement.StopMovement();
        _monsterAttack.StopAttack();
    }

    public override void Update()
    {
        if (!_monsterAttack.IsAttacking)
        {
            _attacksPerformed += 1;

            if (_attacksPerformed >= _attacksNeeded)
                Machine.RequestChangeState(MonsterStates.WANDER);
            else
                Attack();
        }
    }


}
