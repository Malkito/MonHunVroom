using LordBreakerX.States;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Monster States/Neutral")]
public class NeutralState : State
{
    [SerializeField]
    [Min(1)]
    [Tooltip("The range that around the monster that the monster can select a wander point from")]
    private float _wanderRange = 10;

    private bool _isAttacking;

    private NavMeshAgent _agent;

    public override void Init(GameObject machineObject)
    {
        base.Init(machineObject);
        Debug.Log("Test 222");
        _agent = machineObject.GetComponent<NavMeshAgent>();
    }

    public override void DrawGizmosSelected()
    {
        if (StateObject != null) Gizmos.DrawWireSphere(StateObject.transform.position, _wanderRange);
    }

    public override void Update()
    {
        if (_agent.velocity.sqrMagnitude < 0.01f && !_agent.pathPending)
        {
            ChangeDestination();
        }
    }

    private void ChangeDestination()
    {
        Vector3 randomPoint = Random.insideUnitSphere * _wanderRange;
        randomPoint += _agent.transform.position;
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, _wanderRange, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
    }

    public override void Enter()
    {
        _isAttacking = false;
    }

    public override void Exit()
    {
        
    }

    public override void DrawGizmos()
    {
        
    }

    public override void FixedUpdate()
    {
        
    }

    public override void LateUpdate()
    {
        
    }
}
