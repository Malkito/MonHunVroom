using UnityEngine;

namespace LordBreakerX.States
{
    public interface IState : IState<IStateMachine>
    {

    }

    public interface IState<T> where T : IStateMachine
    {
        public string ID { get; }

        public void Initilize(T stateMachine, GameObject stateObject);

        public void Enter();

        public void Exit();

        public void Update();

        public void FixedUpdate();

        public void LateUpdate();
    }
}
