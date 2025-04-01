using UnityEngine;

namespace LordBreakerX.AbilitySystem
{
   
    public abstract class TimedAbility : BaseAbility
    {
        [SerializeField]
        [Header("Timed Ability")]
        [Min(0f)]
        private float _duration;

        private float _durationLeft;

        public override void BeginAbility()
        {
            _durationLeft = _duration;
            ActivateAbility();
        }

        
        public override void Update()
        {
            TimedUpdate();
            _durationLeft -= Time.deltaTime;

            if (_durationLeft <= 0)
            {
                Handler.StopAbility(ID);
            }
        }

        protected abstract void TimedUpdate();

        protected abstract void ActivateAbility();
    }
}
