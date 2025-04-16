using UnityEngine;

namespace LordBreakerX.States
{
    public interface IState
    {
        public string ID { get; }

        public void Initilize(IStateMachine stateMachine, GameObject stateObject);

        public void Enter();

        public void Exit();

        public void Update();

        public void FixedUpdate();

        public void LateUpdate();
    }
}
