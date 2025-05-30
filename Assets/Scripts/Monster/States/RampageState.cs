using LordBreakerX.States;
using UnityEngine;

[CreateAssetMenu(fileName = MonsterStates.RAMPAGE, menuName = MonsterStates.CreatePaths.RAMPAGE)]
public class RampageState : BaseState
{
    MonsterController _monster;

    public override string ID => MonsterStates.RAMPAGE;

    protected override void OnInitilization()
    {
        _monster = StateObject.GetComponent<MonsterController>();
    }

    public override void Enter()
    {
        _monster.StopMovement();
        _monster.RequestStartRandomAttack();
    }

    public override void Update()
    {
        _monster.PlayerAttackTimer.Update();

        if (!_monster.AttackHandler.IsAttacking && !_monster.AttackHandler.IsRequestingAttack)
        {
            Machine.RequestChangeState(MonsterStates.WANDER);
        }
    }
}
