using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    public abstract class Attack
    {
        public AttackController Controller { get; private set; }

        public Attack(AttackController controller)
        {
            Controller = controller;
        }

        public abstract Attack Clone(AttackController controller);

        public virtual void OnAttackFixedUpdate() { }

        public virtual void OnAttackUpdate() { }

        public virtual void OnStart() { }

        public virtual void OnStop() { }

        public virtual bool HasAttackFinished() { return true; }

        public Vector3 GetTargetPosition()
        {
            return Controller.TargetPosition;
        }

        public Vector3 GetCenteredTargetPosition()
        {
            return Controller.CenteredTargetPosition;
        }

        public Vector3 GetStartPosition()
        {
            return Controller.transform.position;
        }

    }
}
