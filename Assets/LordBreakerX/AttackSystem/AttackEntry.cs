using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    [System.Serializable]
    public struct AttackEntry
    {
        [SerializeField]
        private ScriptableAttack _attack;

        [SerializeField]
        [Min(1)]
        private int _weight;

        public ScriptableAttack Attack { get => _attack; }

        public int Weight { get => _weight; }

        public AttackEntry(ScriptableAttack attack, int weight)
        {
            _attack = attack;
            _weight = weight;
        }
    }
}
