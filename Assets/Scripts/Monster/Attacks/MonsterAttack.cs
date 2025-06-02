using LordBreakerX.States;
using UnityEngine;

public abstract class MonsterAttack : Attack
{
    public MonsterController Monster { get; private set; }

    public Vector3 TargetPosition { get { return Monster.TargetPosition; } }

    protected override void OnInilization()
    {
        Monster = AttackHandler.GetComponent<MonsterController>();
    }

    public Vector3 GetAttackPosition()
    {
        if (Monster.Machine.IsCurrentState("Rampage"))
        {
            return GetRandomPosition();
        }
        else if (Monster.Machine.IsCurrentState("TargetPlayer"))
        {
            return GetTargetPosition();
        }
        else
        {
            return AttackHandler.transform.position;
        }
    }
}
