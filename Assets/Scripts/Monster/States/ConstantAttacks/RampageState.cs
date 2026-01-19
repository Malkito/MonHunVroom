using LordBreakerX.States;
using LordBreakerX.Utilities;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = MonsterStates.RAMPAGE, menuName = "Monster/States/Rampage State")]
public class RampageState : BaseState
{
    [SerializeField]
    [Range(0.0f, 120.0f)]
    private float _attackPlayerDelay = 5.0f;

    [SerializeField]
    [Range(0.0f, 120.0f)]
    private float _changeTargetDelay = 2.5f;

    [SerializeField]
    [Range(0.0f, 100.0f)]
    private float _playerAttackChance = 80.0f;

    [SerializeField]
    [Range(0.0f, 100.0f)]
    private float _randomTargetChance = 50.0f;

    [SerializeField]
    [Min(0.0f)]
    private float _targetAttackRadius;

    [SerializeField]
    private LayerMask _ignoredLayers;

    public override string ID => MonsterStates.RAMPAGE;

    private MonsterAttackController _monsterAttack;
    private MonsterMovementController _monsterMovement;

    private Timer _playerAttackCheckTimer;
    private Timer _changeTargetTimer;

    protected override void OnInitilization()
    {
        _monsterAttack = StateObject.GetComponent<MonsterAttackController>();
        _monsterMovement = StateObject.GetComponent<MonsterMovementController>();

        _playerAttackCheckTimer = new Timer(_attackPlayerDelay);
        _playerAttackCheckTimer.OnTimerFinished += PlayerAttackCheck;

        _changeTargetTimer = new Timer(_changeTargetDelay);
        _changeTargetTimer.OnTimerFinished += ChangeToRandomTarget;
    }

    private void ChangeToRandomTarget()
    {
        if (Probability.IsSuccessful(_randomTargetChance))
        {
            _monsterAttack.AttackRandomObject(_targetAttackRadius, _ignoredLayers);
        }
    }

    private void PlayerAttackCheck()
    {
        if (Probability.IsSuccessful(_playerAttackChance))
        {
            Machine.RequestChangeState(MonsterStates.AGRO);
        }
    }

    public override void Enter()
    {
        _playerAttackCheckTimer.Reset();
        _monsterMovement.StopMovement();
    }

    public override void Exit()
    {
        _monsterMovement.StopMovement();
        _monsterAttack.StopAttack();
    }
          
    public override void Update()
    {
        _changeTargetTimer.Update();

        if (!_monsterAttack.IsAttacking)
        {
            _monsterMovement.Wander();
        }

        _playerAttackCheckTimer.Update();
    }
}
