using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    public abstract class AttackCondition : ScriptableObject
    {
        public abstract bool CanUse(AttackController controller);

        public abstract void DrawGizmos(AttackController controller);

        public abstract void DrawGizmosSelected(AttackController controller);
    }
}
