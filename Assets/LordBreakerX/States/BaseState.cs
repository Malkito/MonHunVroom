using UnityEngine;

namespace LordBreakerX.States
{
    public abstract class BaseState: BaseState<IStateMachine>
    {
        
    }

    public abstract class BaseState<T> : ScriptableObject, IState<T> where T : IStateMachine
    {
        [SerializeField]
        private string _id;

        protected T Machine { get; private set; }
        protected GameObject StateObject { get; private set; }

        public string ID { get { return _id; } }

        public void Initilize(T stateMachine, GameObject stateObject)
        {
            Machine = stateMachine;
            StateObject = stateObject;
            OnInitilization();
        }

        protected abstract void OnInitilization();

        public abstract void Enter();

        public abstract void Exit();

        public abstract void Update();

        public abstract void FixedUpdate();

        public abstract void LateUpdate();

        
    }
}
