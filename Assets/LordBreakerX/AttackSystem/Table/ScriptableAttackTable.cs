using LordBreakerX.Tables;
using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    [CreateAssetMenu(menuName = "Attack System/Table")]
    public sealed class ScriptableAttackTable : ScriptableWeightTable<ScriptableAttack>
    {
        public struct ActiveTable
        {
            public int totalWeight;
            public List<WeightedEntry<ScriptableAttack>> availableAttacks;

            public ActiveTable(ScriptableAttackTable attackTable)
            {
                totalWeight = 0;
                availableAttacks = new List<WeightedEntry<ScriptableAttack>>();

                foreach(WeightedEntry<ScriptableAttack> entry in attackTable.Entries)
                {
                    if (entry.Value.CanUseAttack())
                    {
                        availableAttacks.Add(entry);
                        totalWeight += entry.Weight;
                    }
                }
            }
        }


        public ScriptableAttack GetRandomAttack()
        {
            ActiveTable activeAttackTable = new ActiveTable(this);

            int weight = Random.Range(0, activeAttackTable.totalWeight + 1);

            foreach (WeightedEntry<ScriptableAttack> entry in activeAttackTable.availableAttacks)
            {
                if (weight <= entry.Weight)
                {
                    return entry.Value;
                }

                weight -= entry.Weight;
            }
            return null;
        }
        

        public ScriptableAttackTable Clone(AttackController controller)
        {
            ScriptableAttackTable clonedTable = CreateInstance<ScriptableAttackTable>();

            foreach (WeightedEntry<ScriptableAttack> entry in Entries)
            {
                ScriptableAttack attack = ScriptableAttack.Clone(entry.Value, controller);

                WeightedEntry<ScriptableAttack> clonedEntry = new WeightedEntry<ScriptableAttack>(attack, entry.Weight);

                AddEntry(clonedEntry);
            }


            return clonedTable;
        }

    }

}