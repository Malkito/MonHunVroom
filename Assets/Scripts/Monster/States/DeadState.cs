using LordBreakerX.States;
using UnityEngine;

[CreateAssetMenu(menuName = "Monster/States/Dead")]
public class DeadState : BaseState
{
    private MonsterController _monster;
    private Animator _animator;

    protected override void OnInitilization()
    {
        _monster = StateObject.GetComponent<MonsterController>();
        _animator = StateObject.GetComponent<Animator>();
    }

    public override void Enter()
    {
        _monster.StopMovement();
        _animator.SetBool("dead", true);
        _animator.SetBool("walk", false);
    }
}
