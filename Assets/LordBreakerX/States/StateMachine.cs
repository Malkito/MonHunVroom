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
            if (_startingState != null)
            {
                ChangeState(_startingState);
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
            if (CanRegisterState(state))
            {
                IState copiedState = Instantiate(state);
                state.Initilize(this, gameObject);
                _registeredStates.Add(state.ID, state);
            }
        }

        public void RegisterState(IState state)
        {
            if (CanRegisterState(state))
            {
                state.Initilize(this, gameObject);
                _registeredStates.Add(state.ID, state);
            }
        }

        private bool CanRegisterState(IState state)
        {
#if UNITY_EDITOR
            if (state == null)
            {
                Debug.LogWarning("[StateMachine] Could not register state: no state provided!");
                return false;
            }

            if (_registeredStates.ContainsKey(state.ID))
            {
                Debug.LogWarning($"[StateMachine] Could not register state: ID {state.ID} is already taken!");
                return false;
            }

            return true;
#else
            return state != null && !_registeredStates.ContainsKey(state.ID);
#endif
        }

        // created so that can easily use it with unity events in the inspector
        public bool IsCurrentState(BaseState state)
        {
            return IsCurrentState((IState)state);
        }

        public bool IsCurrentState(IState state)
        {
            return _registeredStates.ContainsKey(state.ID) && _registeredStates[state.ID] == _currentState;
        }

        // created so that can easily use it with unity events in the inspector
        public void ChangeState(BaseState state)
        {
            ChangeState((IState)state);
        }

        public void ChangeState(IState state)
        {
            if (CanChangeState(state))
            {
                if (_currentState != null) _currentState.Exit();
                _currentState = state;
                if (_currentState != null) _currentState.Enter();
            }
        }

        private bool CanChangeState(IState state)
        {
#if UNITY_EDITOR
            if (!_registeredStates.ContainsKey(state.ID))
            {
                Debug.LogWarning($"[StateMachine] Could not change state: ID {state.ID} has not been registered.");
                return false;
            }
#endif
            return _currentState != state;
        }

    }
}
