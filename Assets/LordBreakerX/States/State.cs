using UnityEngine;

namespace LordBreakerX.States
{
    public abstract class State : ScriptableObject
    {
        [SerializeField]
        private string _id;

        protected StateMachine StateController { get; private set; }
        protected GameObject StateObject { get; private set; }

        public string ID { get { return _id; } }

        public virtual void Init(GameObject machineObject)
        {
            StateController = machineObject.GetComponent<StateMachine>();
            StateObject = machineObject;
        }

        public abstract void Enter();

        public abstract void Exit();

        public abstract void Update();

        public abstract void FixedUpdate();

        public abstract void LateUpdate();

        public abstract void DrawGizmosSelected();

        public abstract void DrawGizmos();
    }
}
