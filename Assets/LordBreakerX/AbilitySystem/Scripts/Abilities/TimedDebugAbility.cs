using LordBreakerX.AbilitySystem;
using UnityEngine;

namespace LordBreakerX.Utilities
{
    [CreateAssetMenu(menuName = "Abilities/Timed Debug Ability")]
    public class TimedDebugAbility : TimedAbility
    {
        public override bool CanUse()
        {
            return true;
        }

        public override void FinishAbility()
        {
            Debug.Log("Timed Ability Finished!");
        }

        public override void FixedUpdate()
        {
            
        }

        protected override void ActivateAbility()
        {
            Debug.Log("Timed Ability Started!");
        }

        protected override void OnInitilization()
        {
            
        }

        protected override void TimedUpdate()
        {
            Debug.Log("Timed Ability Update!");
        }
    }
}
