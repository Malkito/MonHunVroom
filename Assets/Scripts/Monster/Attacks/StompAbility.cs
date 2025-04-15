using LordBreakerX.AbilitySystem;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Abilities/Monster/Stomp")]
public class StompAbility : BaseAbility
{
    [SerializeField]
    [Header("Properties")]
    [Min(0)]
    private float _maxStompDistance = 1;

    [SerializeField]
    [Min(0)]
    private float _stompRadius = 1;

    [SerializeField]
    private MonsterAbilityUtility _utility = new MonsterAbilityUtility();

    private Vector3 _targetPosition;

    private Vector3 _checkPosition;

    private NavMeshAgent _agent;

    protected override void OnInitilization()
    {
        _utility.Initilize(Handler);
        _agent = Handler.GetComponent<NavMeshAgent>();
    }

    public override void BeginAbility()
    {
    }

    public override void FinishAbility()
    {

    }

    public override bool CanUse()
    {
        return true;
    }

    public override void FixedUpdate()
    {

    }

    public override void Update()
    {
        _targetPosition = _utility.GetTargetPosition();
        _checkPosition = new Vector3(_targetPosition.x, Handler.transform.position.y, _targetPosition.z);
        _agent.SetDestination(_targetPosition);

        if (Vector3.Distance(Handler.transform.position, _checkPosition) < _maxStompDistance)
        {
            _agent.SetDestination(_agent.transform.position);
            _utility.Monster.Stomp();
            Handler.StopAbility(ID);
        }
    }
}
