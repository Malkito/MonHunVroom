using LordBreakerX.Tables;
using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    [CreateAssetMenu(menuName = "Attack System/Table")]
    public class ScriptableAttackTable : ScriptableObject
    {
        [SerializeField]
        private List<WeightedEntry<AttackCreator>> _weightedEntries = new List<WeightedEntry<AttackCreator>>();

        public AttackTable CreateTable(AttackController controller)
        {
            List<WeightedEntry<Attack>> entries = new List<WeightedEntry<Attack>>();

            foreach (WeightedEntry<AttackCreator> creatorEntry in _weightedEntries)
            {
                Attack attack = creatorEntry.Value.Create(controller);
                entries.Add(new WeightedEntry<Attack>(attack, creatorEntry.Weight));
            }

            return new AttackTable(entries);
        }

    }

}