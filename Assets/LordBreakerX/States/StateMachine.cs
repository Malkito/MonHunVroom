using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.States
{
    public class StateMachine : MonoBehaviour
    {
        [SerializeField]
        private State _startingState;

        [SerializeField]
        private List<State> _states = new List<State>();

        private State _currentState;

        private Dictionary<string, State> _registeredStates = new Dictionary<string, State>();

        protected virtual void Awake()
        {
            foreach (State state in _states) 
            {
                if (state != null) RegisterState(state);
            }

            if (_startingState != null) ChangeState(_startingState.ID);
        }

        protected virtual void Update()
        {
            if (_currentState != null) _currentState.Update();
        }

        protected virtual void FixedUpdate()
        {
            if (_currentState != null) _currentState.FixedUpdate();
        }

        protected virtual void LateUpdate()
        {
            if (_currentState != null) _currentState.LateUpdate();
        }

        //protected virtual void OnDrawGizmosSelected()
        //{
        //    if (_states.Count == 0) return;

        //    foreach (State state in _states)
        //    {
        //        if (state !=  null) state.DrawGizmosSelected();
        //    }
        //}

        //protected virtual void OnDrawGizmos()
        //{
        //    if (_states.Count == 0) return;

        //    foreach (State state in _states)
        //    {
        //        if (state != null) state.DrawGizmos();
        //    }
        //}

        public void RegisterState(State state)
        {
            if (state == null || _registeredStates.ContainsKey(state.ID)) return;

            State copiedState = Instantiate(state);

            copiedState.Init(gameObject);
            _registeredStates.Add(copiedState.ID, copiedState);
        }

        public bool IsCurrentState(string id)
        {
            return _registeredStates.ContainsKey(id) && _registeredStates[id] == _currentState;
        }

        public void ChangeState(string id)
        {
            if (!_registeredStates.ContainsKey(id) || _registeredStates[id] == _currentState) return;

            ChangeState(_registeredStates[id]);
        }

        public void ChangeState(State state)
        {
            if (_currentState != null) _currentState.Exit();
            _currentState = state;
            if (_currentState != null) _currentState.Enter();
        }

    }
}
