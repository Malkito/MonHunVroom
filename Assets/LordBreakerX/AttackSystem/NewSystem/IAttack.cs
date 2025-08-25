using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    public interface IAttack
    {
        //[SerializeField]
        //private float _randomPositionRange;

        //protected float RandomPositionRange { get { return _randomPositionRange; } }

        //public Vector3 TargetPosition { get => AttackHandler.TargetPosition; }
        //public Vector3 OffsettedTargetPosition { get => AttackHandler.OffsettedTargetPosition; }

        public AttackProgress Progress { get; set; }

        public void OnAttackUpdate(AttackController attackHandler);

        public void OnPreperationUpdate(AttackController attackHandler);

        public void OnAttackFixedUpdate(AttackController attackHandler);
        public void OnPreperationFixedUpdate(AttackController attackHandler);

        public void OnStart(AttackController attackHandler);

        public void OnStop(AttackController attackHandler);
    }
}
