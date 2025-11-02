using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    public abstract class AttackCreator<T> : AttackCreator where T : Attack
    {
        [SerializeField]
        private T _attackTemplate;

        public override Attack Create(AttackController controller)
        {
            return _attackTemplate.Clone(controller);
        }
    }

    public abstract class AttackCreator : ScriptableObject
    {
        public abstract Attack Create(AttackController controller);
    }
}
