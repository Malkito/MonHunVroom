using LordBreakerX.AttackSystem;
using UnityEngine;

public abstract class Attack
{
    public Attack(AttackController controller)
    {
        Controller = controller;
    }

    public AttackController Controller { get; private set; }

    public Vector3 TargetPosition { get => Controller.TargetPosition; }
    public Vector3 OffsettedTargetPosition { get => Controller.OffsettedTargetPosition; }

    public abstract void OnAttackFixedUpdate();

    public abstract void OnAttackUpdate();

    public abstract void OnStart();

    public abstract void OnStop();

    public abstract bool HasAttackFinished();
}
