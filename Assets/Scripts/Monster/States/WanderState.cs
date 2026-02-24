using LordBreakerX.States.Networked;
using UnityEngine;

[CreateAssetMenu(menuName = MonsterState.CREATE_PATH + "Monster Wander State")]
public sealed class WanderState : MonsterState
{
    [SerializeField]
    [Min(0f)]
    private float _timeBetweenRandomAttacks = 5;

    [SerializeField]
    private MonsterState _attackState;

    private Timer _randomAttackTimer;

    protected override void OnInitlizedState()
    {
        _randomAttackTimer = new Timer(_timeBetweenRandomAttacks);
    }

    protected override void OnStateEnabled()
    {
        _randomAttackTimer.OnTimerFinished += StartRandomAttack;
    }

    protected override void OnStateDisabled()
    {
        _randomAttackTimer.OnTimerFinished -= StartRandomAttack;
    }

    private void StartRandomAttack()
    {
        if (MovementHandler != null) 
            Machine.RequestTransitionTo(_attackState);
    }

    protected override void OnEnterState()
    {
        if (IsServer)
        {
            _randomAttackTimer.Reset();
            MovementHandler.StopMovement();
        }

        MovementHandler.UpdateWalkAnimation(true);
    }

    protected override void OnUpdateState()
    {
        if (IsServer)
        {
            _randomAttackTimer.Update();
            MovementHandler.Wander();
        }
    }
}