using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.States
{
    public class StateMachine : MonoBehaviour, IStateMachine
    {
        [SerializeField]
        private StatesAsset _statesAsset;

        private State _currentState;

        private Dictionary<string, State> _stateRegistry = new Dictionary<string, State>();

        public State CurrentState => _currentState;

        Transform IStateMachine.ParentTransform => transform;

        GameObject IStateMachine.ParentObject => gameObject;

        Vector3 IStateMachine.Position => transform.position;

        public bool HasState { get => _currentState != null; }

        private void Awake()
        {
            if (_statesAsset != null)
            {
                foreach (StateReference state in _statesAsset.States)
                {
                    AddState(state);
                }

                if (_statesAsset.StartingState != null)
                {
                    ChangeState(_statesAsset.StartingState);
                }
            }
        }

        private void OnDestroy()
        {
            foreach(State state in _stateRegistry.Values)
            {
                state.OnDestroy();
            }

            _stateRegistry.Clear();
        }

        private void Update()
        {
            _currentState?.OnUpdate();
        }

        private void FixedUpdate()
        {
            _currentState?.OnFixedUpdate();
        }

        public void AddState(StateReference stateRef)
        {
            if (!_stateRegistry.ContainsKey(stateRef.ID))
            {
                State state = stateRef.GetState(this);
                state.OnCreate();
                _stateRegistry.Add(stateRef.ID, state);
            }
#if UNITY_EDITOR
            else
            {
                Debug.LogError($"State {stateRef.ID} already exists!");
            }
#endif
        }

        public void RemoveState(StateReference stateRef) => RemoveState(stateRef.ID);

        public void RemoveState(string stateID)
        {
            if (_stateRegistry.ContainsKey(stateID))
            {
                State state = _stateRegistry[stateID];
                state.OnDestroy();
                _stateRegistry.Remove(stateID);
            }
#if UNITY_EDITOR
            else
            {
                Debug.LogError($"State {stateID} does not exist!");
            }
#endif
        }

        public void ChangeState(StateReference stateRef)
        {
            State state = GetCachedState(stateRef);
            ChangeState(state);
        }

        public void ChangeState(string stateID)
        {
            State state = GetCachedState(stateID);
            if (state != null)
            {
                ChangeState(state);
            }
#if UNITY_EDITOR
            else
            {
                Debug.LogError($"State {stateID} does not exist!");
            }
#endif
        }

        protected virtual void ChangeState(State state)
        {
            _currentState?.OnExit();
            _currentState = state;
            _currentState?.OnEnter();

        }

        private State GetCachedState(string stateID)
        {
            if (_stateRegistry.ContainsKey(stateID))
            {
                return _stateRegistry[stateID];
            }
            return null;
        }

        private State GetCachedState(StateReference stateRef)
        {
            State state = GetCachedState(stateRef.ID);

            if (state == null)
            {
                state = stateRef.GetState(this);
                state.OnCreate();
                _stateRegistry.Add(stateRef.ID, state);
            }

            return state;
        }

        public bool IsCurrentState(StateReference stateReference)
        {
            State state = stateReference.GetState(this);
            return IsCurrentState(state.ID);
        }

        public bool IsCurrentState(string stateID)
        {
            return _currentState != null && _currentState.ID == stateID;
        }

        public bool IsCurrentState<TState>() where TState : State
        {
            return _currentState is TState;
        }
    }

}