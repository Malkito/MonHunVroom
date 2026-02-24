using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LordBreakerX.States.Networked
{
    public sealed class NetworkStateMachine : NetworkBehaviour
    {
        [SerializeField]
        private NetworkStateList _networkStateList;

        [SerializeField]
        private NetworkScriptableState _startingState;

        private Dictionary<string, NetworkScriptableState> _statesRegistry = new Dictionary<string, NetworkScriptableState>();

        public NetworkScriptableState CurrentState { get; private set; }

        public bool HasState { get => CurrentState != null; }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer && _startingState != null)
            {
                TransitionTo(_startingState);
            }

            if (IsClient)
                ChangeToCurrentStateRpc();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            foreach (NetworkScriptableState state in _statesRegistry.Values)
            {
                state.OnDestroyState();
            }
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void ChangeToCurrentStateRpc()
        {
            if (CurrentState != null) 
                TransitionToRpc(CurrentState.ID);
        }

        private void OnEnable()
        {
            foreach (NetworkScriptableState state in _statesRegistry.Values)
            {
                if (!state.IsEnabled)
                    state.IsEnabled = true;
            }
        }

        private void OnDisable()
        {
            foreach(NetworkScriptableState state in _statesRegistry.Values)
            {
                if (state.IsEnabled)
                    state.IsEnabled = false;
            }
        }

        private void Update()
        {
            if (HasState)
                CurrentState.OnUpdateState();
        }

        private void FixedUpdate()
        {
            if (HasState)
                CurrentState.OnFixedUpdateState();
        }

        public void RequestTransitionTo(NetworkScriptableState stateTemplate)
        {
            if (IsServer)
            {
                TransitionTo(stateTemplate);
                TransitionToRpc(stateTemplate.ID);
            }
        }

        [Rpc(SendTo.NotServer, RequireOwnership = true)]
        private void TransitionToRpc(string stateID)
        {
            NetworkScriptableState state = _networkStateList.GetState(stateID);

            if (state != null)
            {
                TransitionTo(state);
            }
        }

        private void TransitionTo(NetworkScriptableState stateTemplate)
        {
            if (!IsServer) return;

            if (HasState) 
                CurrentState.OnExitState();

            CurrentState = GetOrCreateState(stateTemplate);

            if (HasState) 
                CurrentState.OnEnterState();
        }

        private NetworkScriptableState GetOrCreateState(NetworkScriptableState stateTemplate)
        {
            if (stateTemplate == null) return null;

            if (!_statesRegistry.ContainsKey(stateTemplate.ID))
            {
                NetworkScriptableState stateInstance = NetworkScriptableState.CloneState(stateTemplate, this);

                stateInstance.OnCreateState();
                stateInstance.IsEnabled = gameObject.activeInHierarchy && enabled;
                _statesRegistry.Add(stateTemplate.ID, stateInstance);
            }

            return _statesRegistry[stateTemplate.ID];
        }

    }
}
