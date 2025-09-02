using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    [System.Serializable]
    public struct AttackEntry
    {
        [SerializeField]
        private AttackCreator _attack;

        [SerializeField]
        [Min(1)]
        private int _weight;

        public AttackCreator Attack { get => _attack; }

        public int Weight { get => _weight; }

        public AttackEntry(AttackCreator attack, int weight)
        {
            _attack = attack;
            _weight = weight;
        }
    }
}
