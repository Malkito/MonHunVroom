using LordBreakerX.States;
using UnityEngine;

[CreateAssetMenu(fileName = MonsterStates.DEAD, menuName = MonsterStates.CreatePaths.DEAD)]
public class DeadState : BaseState
{
    public const string DEAD_ANIMATION_VARIABLE = "dead";

    private MonsterController _monster;
    private Animator _animator;

    public override string ID => MonsterStates.DEAD;

    protected override void OnInitilization()
    {
        _monster = StateObject.GetComponent<MonsterController>();
        _animator = StateObject.GetComponent<Animator>();
    }

    public override void Enter()
    {
        _monster.StopMovement();
        _animator.SetBool(DEAD_ANIMATION_VARIABLE, true);
        _animator.SetBool(MonsterController.WALK_ANIMATION_VARIABLE, false);
    }
}
