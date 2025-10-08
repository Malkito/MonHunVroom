using LordBreakerX.States;
using LordBreakerX.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = MonsterStates.ATTACK, menuName = MonsterStates.CreatePaths.ATTACK)]
public class AttackState : BaseState
{
    [SerializeField]
    [Range(0.0f, 100.0f)]
    private float _playerAttackChance = 80.0f;

    [SerializeField]
    [Range(0.0f, 100.0f)]
    private float _targetLargestThreatChance = 20.0f;

    [SerializeField]
    [Min(0.0f)]
    private float _attackRadius = 30.0f;

    [SerializeField]
    [Range(0.0f, 360.0f)]
    private float _stateDuration = 30.0f;

    public override string ID => MonsterStates.ATTACK;

    private MonsterAttackController _monsterAttack;
    private MonsterMovementController _monsterMovement;

    protected override void OnInitilization()
    {
        _monsterAttack = StateObject.GetComponent<MonsterAttackController>();
    }

    public override void Enter()
    {
        if (Probability.IsSuccessful(_playerAttackChance))
        {
            _monsterAttack.AttackRandomPlayer();
        }
        else
        {
            _monsterAttack.AttackRandomPosition();
        }
    }

    public override void Exit()
    {
        _monsterMovement.StopMovement();
        _monsterAttack.StopAttack();
    }


}
