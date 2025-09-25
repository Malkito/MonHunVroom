using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.Stats
{
    public abstract class StatHolder : ScriptableObject
    {
        public abstract List<Stat> GetStats();
    }
}
