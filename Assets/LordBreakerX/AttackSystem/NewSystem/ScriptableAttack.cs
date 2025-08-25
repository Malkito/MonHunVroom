using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    public abstract class ScriptableAttack : ScriptableObject
    {
        public abstract void OnAttackFixedUpdate(AttackController attackHandler);

        public abstract void OnAttackUpdate(AttackController attackHandler);

        public abstract void OnPreperationFixedUpdate(AttackController attackHandler);

        public abstract void OnPreperationUpdate(AttackController attackHandler);

        public abstract void OnStart(AttackController attackHandler);

        public abstract void OnStop(AttackController attackHandler);

        public abstract AttackProgress GetAttackProgress(AttackController attackHandler);
    }
}
