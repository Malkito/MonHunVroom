using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName ="Abilities/Monster/Tail Swipe")]
public class TailSwipe : MonsterAttackAbility
{
    [SerializeField]
    [Header("Properties")]
    [Min(0)]
    private float _maxTailSwipeDistance = 1;

    private Vector3 _targetPosition;

    private Vector3 _checkPosition;

    private NavMeshAgent _agent;

    private bool _started = false;

    public override void BeginAbility()
    {
        _started = false;
    }

    public override bool CanUse()
    {
        return true;
    }

    public override void FinishAbility()
    {
        
    }

    public override void FixedUpdate()
    {
        
    }

    public override void Update()
    {
        _targetPosition = GetTargetPosition();
        _checkPosition = new Vector3(_targetPosition.x, Handler.transform.position.y, _targetPosition.z);
        _agent.SetDestination(_targetPosition);

        if (!_started && Vector3.Distance(Handler.transform.position, _checkPosition) < _maxTailSwipeDistance)
        {
            _agent.SetDestination(_agent.transform.position);
            Monster.TailSwipe();
            _started = true;
        }

        if (_started && Monster.TailSwipeFinished())
        {
            Handler.StopAbility(ID);
        }
    }

    protected override void OnInitilization()
    {
        base.OnInitilization();
        _agent = Handler.GetComponent<NavMeshAgent>();
    }
}
