using LordBreakerX.States.Networked;
using LordBreakerX.Utilities;
using UnityEngine;

[CreateAssetMenu(menuName = MonsterState.CREATE_PATH + "Monster Rampage State")]
public sealed class RampageState : MonsterState
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
    private MonsterState _agroState;

    private Timer _playerAttackCheckTimer;
    private Timer _changeTargetTimer;

    protected override void OnInitlizedState()
    {
        _playerAttackCheckTimer = new Timer(_attackPlayerDelay);
        _changeTargetTimer = new Timer(_changeTargetDelay);
    }

    protected override void OnStateEnabled()
    {
        _playerAttackCheckTimer.OnTimerFinished += PlayerAttackCheck;
        _changeTargetTimer.OnTimerFinished += ChangeToRandomTarget;
    }

    protected override void OnStateDisabled()
    {
        _playerAttackCheckTimer.OnTimerFinished -= PlayerAttackCheck;
        _changeTargetTimer.OnTimerFinished -= ChangeToRandomTarget;
    }

    private void ChangeToRandomTarget()
    {
        if (IsServer && Probability.IsSuccessful(_randomTargetChance))
        {
            AttackHandler.AttackRandomObject<dealDamage>();
        }
    }

    private void PlayerAttackCheck()
    {
        if (IsServer && Probability.IsSuccessful(_playerAttackChance))
        {
            Machine.RequestTransitionTo(_agroState);
        }
    }

    protected override void OnEnterState()
    {
        if (IsServer)
        {
            _playerAttackCheckTimer.Reset();
            MovementHandler.StopMovement();
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
        if (!IsServer) 
            return;

        _playerAttackCheckTimer.Update();

        if (!AttackHandler.IsAttacking)
            MovementHandler.Wander();
    }
}