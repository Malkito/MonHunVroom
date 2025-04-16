using LordBreakerX.AbilitySystem;
using LordBreakerX.States;
using LordBreakerX.Utilities.Math;
using UnityEngine;

public abstract class MonsterAttackAbility : BaseAbility
{
    [Header("Monster Attack Properties")]
    [SerializeField]
    [Min(0)]
    private float _targetRange;

    [SerializeField]
    private NeutralState _neutralState;

    [SerializeField]
    private AttackPlayerState _attackPlayerState;

    private Transform _monsterTransform;

    public MonsterController Monster { get; private set; }
    public StateMachine Machine { get; private set; }

    public override bool CanUse()
    {
        return true;
    }

    protected override void OnInitilization()
    {
        _monsterTransform = Handler.transform;
        Monster = Handler.GetComponent<MonsterController>();
        Machine = Handler.GetComponent<StateMachine>();
    }

    public Vector3 GetTargetPosition()
    {
        Debug.Log($"{Machine} -- {_neutralState}");
        if (Machine.IsCurrentState(_neutralState))
        {
            Vector3 randomPosition = PositionUtility.GetRandomPositionInFrontHalfSquare(_targetRange, _monsterTransform.position + Monster.MonsterBottom, _monsterTransform.forward, _monsterTransform.right);
            return randomPosition;
        }
        else if (Machine.IsCurrentState(_attackPlayerState) && Monster.Target != null)
        {
            return Monster.Target.transform.position;
        }
        else
        {
            return _monsterTransform.position;
        }
    }
}
