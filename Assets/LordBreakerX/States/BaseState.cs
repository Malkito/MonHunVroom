using UnityEngine;

namespace LordBreakerX.States
{
    public abstract class BaseState : ScriptableObject, IState
    {
        [SerializeField]
        private string _id;

        protected IStateMachine Machine { get; private set; }
        protected GameObject StateObject { get; private set; }

        public string ID { get { return _id; } }

        public void Initilize(IStateMachine machine, GameObject stateObject)
        {
            Machine = machine;
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
