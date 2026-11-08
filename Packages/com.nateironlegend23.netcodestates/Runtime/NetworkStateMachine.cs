using LordBreakerX.States;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LordBreakerX.NetcodeStates
{
    public class NetworkStateMachine : NetworkBehaviour, INetworkStateMachine
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

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (_statesAsset != null)
            {
                foreach (StateReference state in _statesAsset.States)
                {
                    AddState(state);
                }

                if (_statesAsset.StartingState != null)
                {
                    if (IsServer)
                        ChangeState(_statesAsset.StartingState);

                    if (IsClient)
                        ChangeToCurrentStateRpc();
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            foreach (State state in _stateRegistry.Values)
            {
                state.OnDestroy();
            }

            _stateRegistry.Clear();
        }

        [Rpc(SendTo.Server, RequireOwnership = true)]
        //[Rpc(SendTo.NotServer, InvokePermission = RpcInvokePermission.Everyone)]
        private void ChangeToCurrentStateRpc()
        {
            if (_currentState != null)
                ChangeStateRpc(_currentState.ID);
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
            if (IsServer)
            {
                State state = GetCachedState(stateRef);
                ChangeState(state);
            }
        }

        public void ChangeState(string stateID)
        {
            if (IsServer)
            {
                State state = GetCachedState(stateID);
                if (state != null)
                {
                    ChangeState(state);
                    ChangeStateRpc(stateID);
                }
#if UNITY_EDITOR
                else
                {
                    Debug.LogError($"State {stateID} does not exist!");
                }
#endif
            }
        }

        private void ChangeState(State state)
        {
            _currentState?.OnExit();
            _currentState = state;
            _currentState?.OnEnter();
        }

        [Rpc(SendTo.NotServer, RequireOwnership = true)]
        //[Rpc(SendTo.NotServer, InvokePermission = RpcInvokePermission.Server)]
        private void ChangeStateRpc(string stateID)
        {
            State state = GetCachedState(stateID);

            if (state != null)
            {
                ChangeState(state);
            }
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
