using UnityEngine;

namespace LordBreakerX.AttackSystemNew
{
    public abstract class ScriptableAttack : ScriptableObject, IAttack
    {
        // TODO: add attack controller reference

        public virtual bool CanUseAttack(AttackNetworkContext context)
        {
            return true;
        }

        public virtual bool HasAttackFinished(AttackNetworkContext context)
        {
            return true;
        }

        public virtual void OnAttackFixedUpdate(AttackNetworkContext context)
        {
            
        }

        public virtual void OnAttackStart(AttackNetworkContext context)
        {
            
        }

        public virtual void OnAttackStop(AttackNetworkContext context)
        {
     
        }

        public virtual void OnAttackUpdate(AttackNetworkContext context)
        {

        }
    }
}
