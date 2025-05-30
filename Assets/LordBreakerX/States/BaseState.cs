using UnityEngine;

namespace LordBreakerX.States
{
    public abstract class BaseState : ScriptableObject
    {
        protected StateMachineNetworked Machine { get; private set; }
        protected GameObject StateObject { get; private set; }

        public abstract string ID { get; }

        public void Initilize(StateMachineNetworked machine, GameObject stateObject)
        {
            Machine = machine;
            StateObject = stateObject;
            OnInitilization();
        }

        protected virtual void OnInitilization() { }

        public virtual void Enter() { }

        public virtual void Exit() { }

        public virtual void Update() { }

        public virtual void FixedUpdate() { }

        public virtual void LateUpdate() { }

        public virtual void OnGizmos() { }
        public virtual void OnGizmosSelected() { }
    }
}
