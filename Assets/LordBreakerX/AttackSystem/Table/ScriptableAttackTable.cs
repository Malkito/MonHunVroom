using LordBreakerX.Tables;
using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    [CreateAssetMenu(menuName = "Attack System/Table")]
    public class ScriptableAttackTable : ScriptableWeightTable<AttackCreator>
    {
        public override WeightTable<AttackCreator> CreateTable()
        {
            return base.CreateTable();
        }

        public Attack GetRandomAttack(AttackController controller)
        {
            Attack attack = GetRandomEntry().Create(controller);
            attack.Initilize(controller);
            return attack;
        }
    }

}