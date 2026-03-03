using LordBreakerX.Tables;
using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    public sealed class AttackTable : WeightTable<ScriptableAttack>
    {
        private List<WeightedEntry<ScriptableAttack>> _useableAttacks = new List<WeightedEntry<ScriptableAttack>>();

        public AttackTable(List<WeightedEntry<ScriptableAttack>> attackEntries) : base(attackEntries)
        {

        }

        public override ScriptableAttack GetRandomEntry()
        {
            UpdateUseableAttacks();
            UpdateTotalWeight();

            Debug.Log("UseableAttacks: " + _useableAttacks.Count);

            return base.GetRandomEntry();
        }

        public void UpdateUseableAttacks()
        {
            _useableAttacks.Clear();

            foreach (WeightedEntry<ScriptableAttack> attackEntry in WeightedEntries)
            {
                if (attackEntry.Value.CanUseAttack())
                {
                    _useableAttacks.Add(attackEntry);
                }
            }
        }

        public override List<WeightedEntry<ScriptableAttack>> GetEntries()
        {
            return _useableAttacks;
        }
    }
}
