using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    public abstract class ScriptableAttack : ScriptableObject
    {
        protected AttackController Controller { get; private set; }

        protected AttackTarget Target { get => Controller.Target; }
        protected Vector3 Position { get => Controller.transform.position; }

        protected LayerMask IgnoredLayers { get => Controller.IgnoredLayers; }

        protected bool IsServer { get => Controller.IsServer; }
        protected bool IsClient { get => Controller.IsClient; }
        protected bool IsHost { get => Controller.IsHost; }

        protected bool IsOwner { get => Controller.IsOwner; }


        public virtual void OnAttackCreation() 
        {
            
        }

        public virtual bool CanUseAttack()
        {
            return true;
        }

        public virtual bool HasAttackFinished()
        {
            return true;
        }

        public virtual void OnAttackFixedUpdate()
        {
        }

        public virtual void OnAttackStarted()
        {
            
        }

        public virtual void OnAttackStopped()
        {
     
        }

        public virtual void OnAttackUpdate()
        {

        }

        internal static ScriptableAttack Clone(ScriptableAttack attack, AttackController controller)
        {
            ScriptableAttack attackInstance = Instantiate(attack);
            attackInstance.Controller = controller;
            attackInstance.OnAttackCreation();
            return attackInstance;
        }
    }
}
