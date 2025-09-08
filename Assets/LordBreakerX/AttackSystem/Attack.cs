using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    public abstract class Attack
    {
        public AttackController Controller { get; private set; }

        public Vector3 TargetPosition { get => Controller.Target.TargetPosition; }

        public void Initilize(AttackController attackController)
        {
            Controller = attackController;
            OnInitilize(attackController);
        }

        public Vector3 GetCenteredTargetPosition()
        {
            return Controller.Target.GetCenteredTargetPosition();
        }

        public abstract Attack Copy(AttackController attackController);

        public virtual void OnInitilize(AttackController attackController) { }

        public virtual void OnAttackFixedUpdate() { }

        public virtual void OnAttackUpdate() { }

        public virtual void OnStart() { }

        public virtual void OnStop() { }

        public virtual bool HasAttackFinished() { return true; }

    }
}
