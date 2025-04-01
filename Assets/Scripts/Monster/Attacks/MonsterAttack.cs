using LordBreakerX.AbilitySystem;
using LordBreakerX.States;
using LordBreakerX.Utilities.Math;
using UnityEngine;

public abstract class MonsterAttack : InstantAbility
{
    [SerializeField]
    [Header("Positioning")]
    [Min(0)]
    private float _randomAttackRange = 10f;

    private StateMachine _monsterMachine;

    private GameObject _player;

    public MonsterController Monster { get; private set; }

    public override void ActivateAbility()
    {
        if (_monsterMachine.IsCurrentState("PlayerAttack") && _player != null)
        {
            // temporary to be replaced with actual targetting
            TargetAttack(_player.transform.position);
        }
        else
        {
            Vector3 randomPosition = PositionUtility.GetRandomPositionInDisc(_randomAttackRange, Handler.transform.position);
            TargetAttack(randomPosition);
        }
    }

    public abstract void TargetAttack(Vector3 targetPosition);

    public override bool CanUse()
    {
        return true;
    }

    protected override void OnInitilization()
    {
        _monsterMachine = Handler.GetComponent<StateMachine>();
        Monster = Handler.GetComponent<MonsterController>();
        _player = GameObject.FindGameObjectWithTag("Player");
    }
}
