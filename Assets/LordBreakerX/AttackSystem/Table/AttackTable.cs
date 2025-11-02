using LordBreakerX.Tables;
using System.Collections.Generic;

namespace LordBreakerX.AttackSystem
{
    public class AttackTable : WeightTable<Attack>
    {
        private List<WeightedEntry<Attack>> _useableAttacks = new List<WeightedEntry<Attack>>();

        public AttackTable(List<WeightedEntry<Attack>> attackEntries) : base(attackEntries)
        {

        }

        public override Attack GetRandomEntry()
        {
            UpdateUseableAttacks();
            UpdateTotalWeight();

            return base.GetRandomEntry();
        }

        public void UpdateUseableAttacks()
        {
            _useableAttacks.Clear();

            foreach (WeightedEntry<Attack> attackEntry in WeightedEntries)
            {
                if (attackEntry.Value.CanUseAttack())
                {
                    _useableAttacks.Add(attackEntry);
                }
            }
        }

        public override List<WeightedEntry<Attack>> GetEntries()
        {
            return _useableAttacks;
        }
    }
}
