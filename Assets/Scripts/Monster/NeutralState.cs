using LordBreakerX.States;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Monster States/Neutral")]
public class NeutralState : State
{
    // remove later once attacks are in
    private string[] _attacks = new string[3] { "Laser Eyes", "Stomp", "Tail Swipe" };


    [SerializeField]
    [Min(1)]
    [Tooltip("The range that around the monster that the monster can select a wander point from")]
    private float _wanderRange = 10;

    [SerializeField]
    [Min(0f)]
    private float _timeBetweenPlayerAttacks = 30;

    [SerializeField]
    [Min(0f)]
    private float _timeBetweenRandomAttacks = 10;

    private bool _isAttacking;

    private NavMeshAgent _agent;

    private Timer _playerAttackTimer;
    private Timer _randomAttackTimer;

    public override void Init(GameObject machineObject)
    {
        base.Init(machineObject);
        _agent = machineObject.GetComponent<NavMeshAgent>();
        _playerAttackTimer = new Timer(_timeBetweenPlayerAttacks);
        _playerAttackTimer.onTimerFinished += OnPlayerAttack;
        _randomAttackTimer = new Timer(_timeBetweenRandomAttacks);
        _randomAttackTimer.onTimerFinished += OnRandomAttack;
    }

    private void OnRandomAttack()
    {
        _isAttacking = true;

        // change this to doing the actual attacks later

        int attackIndex = Random.Range(0, _attacks.Length);
        Debug.Log($"Monster Attacked Random Location with {_attacks[attackIndex]}");

        _isAttacking = false;
    }

    private void OnPlayerAttack()
    {
        Machine.ChangeState("PlayerAttack");
    }

    public override void Update()
    {
        _playerAttackTimer.Step();
        _randomAttackTimer.Step();

        if (!_isAttacking && _agent.velocity.sqrMagnitude < 0.01f && !_agent.pathPending)
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
        _randomAttackTimer.Reset();
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
}
