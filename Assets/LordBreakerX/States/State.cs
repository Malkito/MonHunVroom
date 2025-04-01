using UnityEngine;

namespace LordBreakerX.States
{
    public abstract class State : ScriptableObject
    {
        [SerializeField]
        private string _id;

        protected StateMachine Machine { get; private set; }
        protected GameObject StateObject { get; private set; }

        public string ID { get { return _id; } }

        public void Init(GameObject machineObject)
        {
            Machine = machineObject.GetComponent<StateMachine>();
            StateObject = machineObject;
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
