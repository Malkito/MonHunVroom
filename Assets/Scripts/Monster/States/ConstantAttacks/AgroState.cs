using LordBreakerX.States;
using UnityEngine;

[CreateAssetMenu(fileName = MonsterStates.RAMPAGE, menuName = "Monster/States/Agro State")]
public class AgroState : BaseState
{
    [SerializeField]
    [Range(1, 60)]
    private int _minAttacksPerformed = 2;

    [SerializeField]
    [Range(1, 60)]
    private int _maxAttacksPerformed = 3;

    private MonsterAttackController _monsterAttack;
    private MonsterMovementController _monsterMovement;

    private int _attacksPerformed;
    private int _attacksNeeded;

    public override string ID => MonsterStates.AGRO;

    protected override void OnInitilization()
    {
        _monsterAttack = StateObject.GetComponent<MonsterAttackController>();
        _monsterMovement = StateObject.GetComponent<MonsterMovementController>();
    }

    public override void Enter()
    {
        _monsterAttack.AttackRandomPlayer();
        _attacksNeeded = Random.Range(_minAttacksPerformed, _maxAttacksPerformed + 1);
        _attacksPerformed = 0;
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
                Machine.RequestChangeState(MonsterStates.RAMPAGE);
            else
                _monsterAttack.AttackRandomPlayer();
        }
        
    }
}
