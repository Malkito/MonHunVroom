using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.Tables
{
    public class ScriptableWeightTable<T> : ScriptableObject, IWeightedEntryGetter<T> where T : class
    {
        [SerializeField]
        private List<WeightedEntry<T>> _weightedEntries = new List<WeightedEntry<T>>();

        [System.NonSerialized]
        private WeightTable<T> _table;

        protected IReadOnlyList<WeightedEntry<T>> WeightedEntries { get { return _weightedEntries; } }

        public virtual WeightTable<T> CreateTable()
        {
            return new WeightTable<T>(_weightedEntries);
        }

        public virtual T GetRandomEntry()
        {
            if (_table == null)
            {
                _table = CreateTable();
            }
            return _table.GetRandomEntry();
        }
    }

}