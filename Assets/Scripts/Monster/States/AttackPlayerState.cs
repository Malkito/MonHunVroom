using LordBreakerX.States;
using System;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName ="Monster States/Attack")]
public class AttackPlayerState : State
{
    [SerializeField]
    [Min(0)]
    private float _timeInState = 30;

    private Timer _stateTimer;

    private NavMeshAgent _agent;

    protected override void OnInitilization()
    {
        _agent = StateObject.GetComponent<NavMeshAgent>();

        _stateTimer = new Timer(_timeInState);
        _stateTimer.onTimerFinished += StopAttack;
    }

    private void StopAttack()
    {
        Machine.ChangeState("Neutral");
    }

    // this script will need to be changed once the attacking system has been fully implemented
    public override void Enter()
    {
        _agent.SetDestination(_agent.transform.position);
        Debug.Log("Targetting Player for attacks!");
    }

    public override void Exit()
    {
        Debug.Log("Finished Attack State!");
    }

    public override void Update()
    {
        _stateTimer.Step();
    }

    public override void FixedUpdate()
    {
        
    }

    public override void LateUpdate()
    {
        
    }
}
