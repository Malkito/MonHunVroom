using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    public abstract class ScriptableAttack : ScriptableObject
    {
        public AttackController Controller { get; private set; }

        public AttackTarget Target { get => Controller.Target; }
        public Vector3 Position { get => Controller.transform.position; }

        public LayerMask IgnoredLayers { get => Controller.IgnoredLayers; }


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

        public static ScriptableAttack Clone(ScriptableAttack attack, AttackController controller)
        {
            ScriptableAttack attackInstance = Instantiate(attack);
            attackInstance.Controller = controller;
            attackInstance.OnAttackCreation();
            return attackInstance;
        }
    }
}
