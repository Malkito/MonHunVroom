using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    [CreateAssetMenu(menuName = "AI Attacking/Attack Table")]
    public class AttackTable : ScriptableObject
    {
        [SerializeField]
        private AttackCondition _selectCondition;

        [SerializeField]
        private List<AttackEntry> _attacks = new List<AttackEntry>();

        private int _totalWeight;

        private void Awake()
        {
            SetTotalWeight();
            Debug.Log("Set Weight!");
        }

        public bool CanUse(AttackController controller)
        {
            return _selectCondition == null || _selectCondition.CanUse(controller);
        }

        public void DrawGizmos(AttackController controller)
        {
            if (_selectCondition != null) _selectCondition.DrawGizmos(controller);
        }

        public void DrawGizmosSelected(AttackController controller)
        {
            if (_selectCondition != null) _selectCondition.DrawGizmosSelected(controller);
        }

        public ScriptableAttack GetRandomAttack()
        {
            int weight = Random.Range(0, _totalWeight);

            foreach (AttackEntry entry in _attacks)
            {
                if (weight <= entry.Weight)
                {
                    return entry.Attack;
                }
                else
                {
                    weight -= entry.Weight;
                }
            }

            return _attacks[0].Attack;
        }

        private void SetTotalWeight()
        {
            _totalWeight = 0;

            foreach (AttackEntry entry in _attacks)
            {
                _totalWeight += entry.Weight;
            }
        }

    }
}
