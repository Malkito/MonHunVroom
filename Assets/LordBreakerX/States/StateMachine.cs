using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.States
{
    public class StateMachine : MonoBehaviour, IStateMachine
    {
        [SerializeField]
        private BaseState _startingState;

        [SerializeField]
        private List<BaseState> _statesToRegister = new List<BaseState>();

        private Dictionary<string, IState> _registeredStates = new Dictionary<string, IState>();

        private IState _currentState;

        private void Awake()
        {
            foreach (BaseState state in _statesToRegister) 
            {
                if (state != null) RegisterState(state);
            }
        }

        private void Start()
        {
            //Note: Done this way so that for a state thats only used as the starting state
            //      and not wanting to be able to change to the state.
            if (_startingState != null)
            {
                if (_registeredStates.ContainsKey(_startingState.ID))
                {
                    ChangeState(_startingState.ID);
                }
                else
                {
                    BaseState copiedStartingState = Instantiate(_startingState);
                    copiedStartingState.Initilize(this, gameObject);
                    ChangeState(copiedStartingState);
                }
            }
        }

        private void Update()
        {
            if (_currentState != null) _currentState.Update();
        }

        private void FixedUpdate()
        {
            if (_currentState != null) _currentState.FixedUpdate();
        }

        private void LateUpdate()
        {
            if (_currentState != null) _currentState.LateUpdate();
        }

        public void RegisterState(BaseState state)
        {
            if (state == null || _registeredStates.ContainsKey(state.ID)) return;

            IState copiedState = Instantiate(state);
            RegisterState(copiedState);
        }

        public void RegisterState(IState state)
        {
            if (state == null || _registeredStates.ContainsKey(state.ID)) return;
            state.Initilize(this, gameObject);
            _registeredStates.Add(state.ID, state);
        }

        public bool IsCurrentState(string stateID)
        {
            return _registeredStates.ContainsKey(stateID) && _registeredStates[stateID] == _currentState;
        }

        public bool IsCurrentState(IState state)
        {
            return _currentState == state;
        }

        public void ChangeState(string stateID)
        {
            if (!_registeredStates.ContainsKey(stateID)) return;

            ChangeState(_registeredStates[stateID]);
        }

        public void ChangeState(IState state)
        {
            if (_currentState == state) return;

            if (_currentState != null) _currentState.Exit();
            _currentState = state;
            if (_currentState != null) _currentState.Enter();
        }

    }
}
