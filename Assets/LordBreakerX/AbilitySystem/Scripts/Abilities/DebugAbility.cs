using UnityEngine;

namespace LordBreakerX.AbilitySystem
{
    [CreateAssetMenu(menuName = "Abilities/Instant Debug Ability")]
    public class DebugAbility : InstantAbility
    {
        public override bool CanUse()
        {
            return true;
        }

        public override void FinishAbility()
        {
            Debug.Log($"{name} ability has finished!");
        }

        public override void Update()
        {

        }

        protected override void OnInitilization()
        {

        }

        public override void FixedUpdate()
        {

        }

        public override void ActivateAbility()
        {
            Debug.Log($"{name} ability is currently active!");
        }
    }
}
