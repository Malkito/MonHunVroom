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

            if (_startingState != null)
            {
                State copiedStartingState = Instantiate(_startingState);
                copiedStartingState.Init(gameObject);
                ChangeState(copiedStartingState);
            }
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

        public void RegisterState(State state)
        {
            if (state == null || _registeredStates.ContainsKey(state.ID)) return;

            State copiedState = Instantiate(state);

            copiedState.Init(gameObject);
            _registeredStates.Add(copiedState.ID, copiedState);
        }

        public bool IsCurrentState(string stateID)
        {
            return _registeredStates.ContainsKey(stateID) && _registeredStates[stateID] == _currentState;
        }

        public void ChangeState(string stateID)
        {
            if (!_registeredStates.ContainsKey(stateID) || _registeredStates[stateID] == _currentState) return;

            ChangeState(_registeredStates[stateID]);
        }

        public void ChangeState(State state)
        {
            if (_currentState != null) _currentState.Exit();
            _currentState = state;
            if (_currentState != null) _currentState.Enter();
        }

    }
}
