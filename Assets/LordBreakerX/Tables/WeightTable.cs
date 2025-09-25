using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.Tables
{
    [System.Serializable]
    public class WeightTable<T> : IWeightedEntryGetter<T> where T : class
    {
        [SerializeField]
        private readonly List<WeightedEntry<T>> _weightedEntries = new List<WeightedEntry<T>>();


        private readonly int _totalWeight;

        public WeightTable(List<WeightedEntry<T>> weightedEntries) 
        {
            _weightedEntries = weightedEntries;
            _totalWeight = 0;

            foreach (WeightedEntry<T> entry in _weightedEntries)
            {
                _totalWeight += entry.Weight;
            }
        }

        public T GetRandomEntry()
        {
            int weight = Random.Range(0, _totalWeight + 1);

            foreach (WeightedEntry<T> entry in _weightedEntries)
            {
                if (weight <= entry.Weight)
                {
                    OnEntrySelected(entry.Value);
                    return entry.Value;
                }

                weight -= entry.Weight;
            }
            return null;
        }

        protected virtual void OnEntrySelected(T value) { }
    }
}
