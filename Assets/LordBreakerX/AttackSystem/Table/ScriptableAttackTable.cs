using LordBreakerX.Tables;
using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    [CreateAssetMenu(menuName = "Attack System/Table")]
    public sealed class ScriptableAttackTable : ScriptableObject
    {
        [SerializeField]
        private WeightTable<ScriptableAttack> _attacks = new WeightTable<ScriptableAttack>();

        public AttackTable CreateTable(AttackController controller)
        {
            List<WeightedEntry<ScriptableAttack>> attackEntries = new List<WeightedEntry<ScriptableAttack>>();

            foreach (WeightedEntry<ScriptableAttack> entry in _attacks.WeightedEntries)
            {
                ScriptableAttack attack = ScriptableAttack.Clone(entry.Value, controller);
                attackEntries.Add(new WeightedEntry<ScriptableAttack>(attack, entry.Weight));
            }

            return new AttackTable(attackEntries);
        }

    }

}