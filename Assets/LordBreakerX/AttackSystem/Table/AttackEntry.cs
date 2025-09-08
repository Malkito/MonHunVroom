using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    [System.Serializable]
    public class AttackEntry
    {
        [SerializeField]
        private AttackType _type;

        [SerializeField]
        [Min(1)]
        private int _weight = 1;

        public AttackType Type { get { return _type; } }
        public int Weight { get { return _weight; } }

        public AttackEntry(AttackType type, int weight)
        {
            _type = type;
            _weight = weight;
        }
    }
}
