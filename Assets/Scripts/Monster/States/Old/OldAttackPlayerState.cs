using LordBreakerX.AbilitySystem;
using LordBreakerX.States;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName ="Monster States/Attack")]
public class OldAttackPlayerState : BaseState
{
    [SerializeField]
    [Min(0)]
    private float _timeInState = 30;

    [SerializeField]
    private OldNeutralState _neutralState;

    private Timer _stateTimer;

    private MonsterControllerOld _monster;

    private AbilityHandler _abilityHandler;

    protected override void OnInitilization()
    {
        _monster = StateObject.GetComponent<MonsterControllerOld>();
        _abilityHandler = StateObject.GetComponent<AbilityHandler>();

        _stateTimer = new Timer(_timeInState);
        _stateTimer.onTimerFinished += StopAttack;
    }

    private void StopAttack()
    {
        Machine.RequestChangeState(_neutralState);
    }

    public override void Enter()
    {
        _monster.StopMovement();

        _monster.UpdateTarget();
        _monster.ResetDamageTable();

        if (_monster.Target == null) StopAttack();
        Debug.Log("Targetting Player for attacks!");
    }

    public override void Exit()
    {
        Debug.Log("Finished Attack State!");
        _abilityHandler.StopAllAbilities();
    }

    public override void Update()
    {
        _stateTimer.Step();

        if (!_stateTimer.IsFinished && !_abilityHandler.HasActiveAbility)
        {
            _abilityHandler.StartRandomAbility();
        }
    }
}
