using UnityEngine;

namespace LordBreakerX.Tables
{
    public interface IWeightedEntryGetter<T> where T : class
    {
        public T GetRandomEntry();
    }
}
