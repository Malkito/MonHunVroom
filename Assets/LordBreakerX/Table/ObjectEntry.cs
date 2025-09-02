using UnityEngine;

namespace LordBreakerX.Tables
{
    [System.Serializable]
    public class ObjectEntry<T> where T : Object
    {
        [SerializeField]
        private T _prefab;

        [SerializeField]
        [Min(1)]
        private int _weight = 1;

        public T Prefab { get { return _prefab; } }
        public int Weight { get { return _weight; } }

        public ObjectEntry(T prefab, int weight)
        {
            _prefab = prefab;
            _weight = weight;
        }
    }
}
