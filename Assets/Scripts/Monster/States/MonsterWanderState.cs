using LordBreakerX.Attributes;
using UnityEngine;

[CreateAssetMenu(menuName = MonsterState.CREATE_PATH + "Monster Wander State")]
public class MonsterWanderState : MonsterState
{
    [SerializeField]
    [Min(1)]
    private float _minDuration = 5;

    [SerializeField]
    [Min(1)]
    private float _maxDuration = 10;

    [SerializeField]
    private MonsterAttackState _attackState;

    private Timer _durationTimer;

    private void OnValidate()
    {
        _maxDuration = Mathf.Max(_maxDuration, _minDuration);
    }

    private void OnDurationEnded()
    {
        Machine.RequestTransitionTo(_attackState);
    }

    protected override void OnInitlizedState()
    {
        _durationTimer = new Timer(OnDurationEnded);
    }

    protected override void OnEnterState()
    {
        // set wander duration
        float duration = Random.Range(_minDuration, _maxDuration);
        _durationTimer.SetDuration(duration);

        if (IsServer)
        {
            MovementHandler.StopMovement();
        }
    }

    protected override void OnUpdateState()
    {
        if (IsServer)
        {
            _durationTimer.Update();
            MovementHandler.Wander();
        }
    }
}
