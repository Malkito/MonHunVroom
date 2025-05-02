using LordBreakerX.States;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Monster States/Dead")]
public class OldDeadState : BaseState
{
    private NavMeshAgent _agent;

    protected override void OnInitilization()
    {
        _agent = StateObject.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        _agent.SetDestination(_agent.transform.position);
        Debug.Log("The <color=red>monster</color> died!");
    }

    public override void Exit()
    {

    }

    public override void FixedUpdate()
    {
        
    }

    public override void LateUpdate()
    {
        
    }

    public override void Update()
    {
        
    }

    public override void OnGizmos()
    {
        
    }

    public override void OnGizmosSelected()
    {
        
    }
}
