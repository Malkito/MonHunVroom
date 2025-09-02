using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    public abstract class AttackCreator : ScriptableObject
    {
        public abstract Attack Create(AttackController controller);
    }
}
