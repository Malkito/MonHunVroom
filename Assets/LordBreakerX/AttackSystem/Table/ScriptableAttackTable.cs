using LordBreakerX.Tables;
using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    [CreateAssetMenu(menuName = "Attack System/Table")]
    public class ScriptableAttackTable : ScriptableObject
    {
        [SerializeField]
        private List<WeightedEntry<ScriptableAttack>> _weightedEntries = new List<WeightedEntry<ScriptableAttack>>();

        public AttackTable CreateTable(AttackController controller)
        {
            List<WeightedEntry<ScriptableAttack>> entries = new List<WeightedEntry<ScriptableAttack>>();

            foreach (WeightedEntry<ScriptableAttack> creatorEntry in _weightedEntries)
            {
                ScriptableAttack attack = ScriptableAttack.Clone(creatorEntry.Value, controller);
                entries.Add(new WeightedEntry<ScriptableAttack>(attack, creatorEntry.Weight));
            }

            return new AttackTable(entries);
        }

    }

}