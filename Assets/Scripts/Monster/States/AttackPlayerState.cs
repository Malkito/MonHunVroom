using LordBreakerX.AbilitySystem;
using LordBreakerX.States;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName ="Monster States/Attack")]
public class AttackPlayerState : BaseState
{
    [SerializeField]
    [Min(0)]
    private float _timeInState = 30;

    [SerializeField]
    private NeutralState _neutralState;

    private Timer _stateTimer;

    private NavMeshAgent _agent;

    private MonsterController _monster;

    private AbilityHandler _abilityHandler;

    protected override void OnInitilization()
    {
        _agent = StateObject.GetComponent<NavMeshAgent>();
        _monster = StateObject.GetComponent<MonsterController>();
        _abilityHandler = StateObject.GetComponent<AbilityHandler>();

        _stateTimer = new Timer(_timeInState);
        _stateTimer.onTimerFinished += StopAttack;
    }

    private void StopAttack()
    {
        Machine.ChangeState(_neutralState);
    }

    public override void Enter()
    {
        _agent.SetDestination(_agent.transform.position);

        _monster.UpdateTarget();
        _monster.ResetDamageTable();

        if (_monster.Target == null) StopAttack();
        Debug.Log("Targetting Player for attacks!");
    }

    public override void Exit()
    {
        Debug.Log("Finished Attack State!");
        _abilityHandler.StopAllAbilities();
    }

    public override void Update()
    {
        _stateTimer.Step();

        if (!_stateTimer.IsFinished && !_abilityHandler.HasActiveAbility)
        {
            _abilityHandler.StartRandomAbility();
        }
    }

    public override void FixedUpdate()
    {
        
    }

    public override void LateUpdate()
    {
        
    }

    public override void OnGizmos()
    {
        
    }

    public override void OnGizmosSelected()
    {
        
    }
}
