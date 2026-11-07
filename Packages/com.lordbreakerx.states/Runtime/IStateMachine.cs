using UnityEngine;

namespace LordBreakerX.States
{
    public interface IStateMachine
    {
        public State CurrentState { get; }

        public Transform ParentTransform { get; }

        public GameObject ParentObject { get; }

        public Vector3 Position { get; }

        public bool HasState { get; }

        public void AddState(StateReference stateRef);

        public void RemoveState(StateReference stateRef);

        public void RemoveState(string stateID);

        public void ChangeState(StateReference stateReference);

        public void ChangeState(string stateID);

        public bool IsCurrentState(StateReference stateReference);

        public bool IsCurrentState(string stateID);

        public bool IsCurrentState<TState>() where TState : State;
    } 
}
