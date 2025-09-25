using UnityEngine;

namespace LordBreakerX.Tables
{
    [System.Serializable]
    public class WeightedEntry<T> where T : class
    {
        [SerializeField]
        private T _value;

        [SerializeField]
        [Min(1)]
        private int _weight = 1;

        public T Value { get { return _value; } }
        public int Weight { get { return _weight; } }

        public WeightedEntry(T prefab, int weight)
        {
            _value = prefab;
            _weight = weight;
        }
    }
}
