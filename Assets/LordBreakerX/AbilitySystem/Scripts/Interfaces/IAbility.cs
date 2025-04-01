using UnityEngine;

namespace LordBreakerX.AbilitySystem
{
    public interface IAbility
    {
        public string ID { get; }
        public void Initilize(AbilityHandler handler);
        public void BeginAbility();
        public void FinishAbility();

        public void Update();
        public void FixedUpdate();

        public bool CanUse();
    }
}
