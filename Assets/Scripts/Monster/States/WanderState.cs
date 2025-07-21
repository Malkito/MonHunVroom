using LordBreakerX.States;
using UnityEngine;

[CreateAssetMenu(fileName = MonsterStates.WANDER, menuName = MonsterStates.CreatePaths.WANDER)]
public class WanderState : BaseState
{
    private const float CHANGE_DESTINATION_THRESHOLD = 1.2f;

    [SerializeField]
    [Min(0f)]
    private float _timeBetweenRandomAttacks = 10;

    private MonsterMovementController _monsterMovement;
    private MonsterAttackController _monsterAttack;

    private Timer _randomAttackTimer;

    public override string ID => MonsterStates.WANDER;

    protected override void OnInitilization()
    {
        _monsterMovement = StateObject.GetComponent<MonsterMovementController>();
        _monsterAttack = StateObject.GetComponent<MonsterAttackController>();

        _randomAttackTimer = new Timer(_timeBetweenRandomAttacks);

        _randomAttackTimer.OnTimerFinished += StartRandomAttack;
    }

    private void StartRandomAttack()
    {
        if (_monsterMovement != null) Machine.RequestChangeState(MonsterStates.RAMPAGE);
    }

    public override void Enter()
    {
        _randomAttackTimer.Reset();
        _monsterMovement.StopMovement();
        _monsterMovement.UpdateWalkAnimation(true);
    }

    public override void Update()
    {
        _monsterAttack.PlayerAttackTimer.Update();
        _randomAttackTimer.Update();

        if (!CanChangeDestination()) 
            return;

        _monsterMovement.SetRandomDestination();
    }

    private bool CanChangeDestination()
    {
        return Vector3.Distance(StateObject.transform.position, _monsterMovement.CurrentDestination) <= CHANGE_DESTINATION_THRESHOLD;
    }
}
