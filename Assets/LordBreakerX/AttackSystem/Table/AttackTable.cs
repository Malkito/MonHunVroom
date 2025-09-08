using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    [CreateAssetMenu(menuName = "Monster/Attack Table")]
    public class AttackTable : ScriptableObject
    {
        [SerializeField]
        private AttackFactory _attackFactory;

        [SerializeField]
        private List<AttackEntry> _objectEntries = new List<AttackEntry>();

        private int _totalWeight = 0;

        public void Initlize()
        {
            _totalWeight = 0;

            foreach (AttackEntry entry in _objectEntries)
            {
                _totalWeight += entry.Weight;
            }
        }

        public Attack GetRandomAttack(AttackController attackController)
        {
            int weight = Random.Range(0, _totalWeight);

            foreach (AttackEntry entry in _objectEntries)
            {
                if (weight <= entry.Weight)
                {
                    AttackType attackType = entry.Type;
                    return _attackFactory.CreateAttack(attackController, attackType);
                }

                weight -= entry.Weight;
            }

            return null;
        }
    }

}