using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    public abstract class ScriptableAttack : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The max distance for the attack to be started")]
        [Min(0)]
        private float _startAttackDistance;

        [SerializeField]
        [Tooltip("When the target is farther then this distance the attack will be stopped")]
        [Min(0)]
        private float _stopAttackDistance;

        protected AttackController Controller { get; private set; }

        public float StartAttackRange { get => _startAttackDistance; }

        protected AttackTarget Target { get => Controller.Target; }
        protected Vector3 Position { get => Controller.transform.position; }

        protected LayerMask IgnoredLayers { get => Controller.IgnoredLayers; }

        protected bool IsServer { get => Controller.IsServer; }
        protected bool IsClient { get => Controller.IsClient; }
        protected bool IsHost { get => Controller.IsHost; }

        protected bool IsOwner { get => Controller.IsOwner; }

        public virtual void OnValidate()
        {
            _stopAttackDistance = Mathf.Max(_stopAttackDistance, _startAttackDistance + 0.01f);
        }

        public virtual void OnAttackCreation() 
        {
            
        }

        public virtual bool CanUseAttack()
        {
            Vector3 controllerPosition = Controller.transform.position;
            Vector3 targetPosition = Target.GetPosition();

            controllerPosition.y = 0;
            targetPosition.y = 0;

            float distance = Vector3.Distance(targetPosition, controllerPosition);
            return distance <= _startAttackDistance;
        }

        public virtual bool HasAttackFinished()
        {
            Vector3 controllerPosition = Controller.transform.position;
            Vector3 targetPosition = Target.GetPosition();

            controllerPosition.y = 0;
            targetPosition.y = 0;

            float distance = Vector3.Distance(targetPosition, controllerPosition);
            return distance >= _stopAttackDistance;
        }

        public virtual void OnAttackFixedUpdate()
        {
        }

        public virtual void OnAttackStarted()
        {
            
        }

        public virtual void OnAttackStopped()
        {
     
        }

        public virtual void OnAttackUpdate()
        {

        }

        internal static ScriptableAttack Clone(ScriptableAttack attack, AttackController controller)
        {
            ScriptableAttack attackInstance = Instantiate(attack);
            attackInstance.Controller = controller;
            attackInstance._startAttackDistance = attack._startAttackDistance;
            attackInstance._stopAttackDistance = attack._stopAttackDistance;
            attackInstance.OnAttackCreation();
            return attackInstance;
        }
    }
}
