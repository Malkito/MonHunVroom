using LordBreakerX.States;
using UnityEngine;

[CreateAssetMenu(fileName = MonsterStates.WANDER, menuName = MonsterStates.CreatePaths.WANDER)]
public class WanderState : BaseState
{
    private const float MIN_STOP_CHANCE = 0.0f;
    private const float MAX_STOP_CHANCE = 1.0f;

    private const float CHANGE_DESTINATION_THRESHOLD = 1.2f;

    private static readonly Color MIN_WANDER_RADIUS_GIZMO_COLOR = Color.blue;
    private static readonly Color MAX_WANDER_RADIUS_GIZMO_COLOR = Color.yellow;

    // variables of states should not be changed once initilized (unless its client side only)
    // variables that change that needs to be done for all clients should be done in a NetworkBehavior script

    [SerializeField]
    [Min(0f)]
    private float _maxWanderRadius;

    [SerializeField]
    [Min(0f)]
    private float _minWanderRadius;

    [SerializeField]
    [Min(0)]
    private float _radiusDecreaseAmount;

    [SerializeField]
    [Min(0f)]
    private float _minStopTime = 0f;

    [SerializeField]
    private float _maxStopTime = 1f;

    [SerializeField]
    [Min(0f)]
    private float _stopChance = 0.3f;

    [SerializeField]
    [Min(0f)]
    private float _timeBetweenRandomAttacks = 10;

    private MonsterController _monster;

    private Timer _randomAttackTimer;
    private Timer _stopTimer;

    private bool _monsterStopped = false;

    public override string ID => MonsterStates.WANDER;

    private void OnValidate()
    {
        _stopChance = Mathf.Clamp(_stopChance, MIN_STOP_CHANCE, MAX_STOP_CHANCE);
    }

    protected override void OnInitilization()
    {
        _monster = StateObject.GetComponent<MonsterController>();

        _randomAttackTimer = new Timer(_timeBetweenRandomAttacks);

        _randomAttackTimer.OnTimerFinished += StartRandomAttack;

        _stopTimer.OnTimerFinished += OnFinishStop;
    }

    private void StartRandomAttack()
    {
        if (_monster != null) Machine.RequestChangeState(MonsterStates.RAMPAGE);
    }

    private void OnFinishStop()
    {
        _monster.RandomDestination(_minWanderRadius, _maxWanderRadius, _radiusDecreaseAmount);
        _monsterStopped = false;
    }

    public override void Enter()
    {
        _stopTimer.Reset();
        _randomAttackTimer.Reset();
        _monster.StopMovement();
        _monsterStopped = false;
    }

    public override void Update()
    {
        _monster.PlayerAttackTimer.Update();
        _randomAttackTimer.Update();

        if (!CanChangeDestination() || _monster.FindingTargetPosition) 
            return;

        _monster.RandomDestination(_minWanderRadius, _maxWanderRadius, _radiusDecreaseAmount);

        //if (_monsterStopped)
        //{
        //    _stopTimer.Update();
        //    return;
        //}

        //Debug.Log("Made it passed");

        //if (Random.value <= _stopChance)
        //{
        //    Debug.Log("Stopping");
        //    _stopTimer = new Timer(Random.Range(_minStopTime, _maxStopTime));
        //    _monsterStopped = true;
        //}
        //else
        //{
        //    _monster.RandomDestination(_minWanderRadius, _maxWanderRadius, _radiusDecreaseAmount);
        //}
    }

    private bool CanChangeDestination()
    {
        return Vector3.Distance(StateObject.transform.position, _monster.TargetPosition) <= CHANGE_DESTINATION_THRESHOLD;
    }

    public override void OnGizmosSelected()
    {
        Gizmos.color = MIN_WANDER_RADIUS_GIZMO_COLOR;
        Gizmos.DrawWireSphere(StateObject.transform.position, _maxWanderRadius);

        Gizmos.color = MAX_WANDER_RADIUS_GIZMO_COLOR;
        Gizmos.DrawWireSphere(StateObject.transform.position, _minWanderRadius);
    }
}
