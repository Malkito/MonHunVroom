using LordBreakerX.States;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Monster States/Dead")]
public class DeadState : State
{
    private NavMeshAgent _agent;

    public override void Init(GameObject machineObject)
    {
        base.Init(machineObject);
        _agent = machineObject.GetComponent<NavMeshAgent>();
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
}
