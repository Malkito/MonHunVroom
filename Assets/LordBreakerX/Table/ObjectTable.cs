using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.Tables
{
    public class ObjectTable<T> : ScriptableObject where T : Object
    {
        [SerializeField]
        private List<ObjectEntry<T>> _objectEntries = new List<ObjectEntry<T>>();

        private int _totalWeight = 0;

        public void Initlize()
        {
            _totalWeight = 0;

            foreach (ObjectEntry<T> entry in _objectEntries)
            {
                _totalWeight += entry.Weight;
            }
        }

        public T GetRandomObject()
        {
            int weight = Random.Range(0, _totalWeight);

            foreach (ObjectEntry<T> entry in _objectEntries)
            {
                if (weight <= entry.Weight)
                {
                    return entry.Prefab;
                }

                weight -= entry.Weight;
            }

            return null;
        }
    }

}