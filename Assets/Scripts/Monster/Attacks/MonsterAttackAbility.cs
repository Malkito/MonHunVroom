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

    public float TargetRange { get; private set; }

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
        if (Machine.IsCurrentState(_neutralState))
        {
            return RandomTargetPosition();
        }
        else if (Machine.IsCurrentState(_attackPlayerState))
        {
            return PlayerTargetPosition();
        }
        else
        {
            return _monsterTransform.position;
        }
    }

    public virtual Vector3 RandomTargetPosition()
    {
        return NavMeshUtility.GetRandomPosition(_monsterTransform.position, _targetRange);
    }

    public virtual Vector3 PlayerTargetPosition()
    {
        return Monster.Target.transform.position;
    }
}
