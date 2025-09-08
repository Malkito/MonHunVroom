using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    public abstract class AttackFactory : ScriptableObject
    {
        public abstract Attack CreateAttack(AttackController attackController, AttackType attackType);
    }
}
