using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LordBreakerX.States
{
    public class StateMachineNetworked : NetworkBehaviour
    {
        [SerializeField]
        private BaseState _startingState;

        [SerializeField]
        private List<BaseState> _statesToRegister = new List<BaseState>();

        private Dictionary<string, BaseState> _registeredStates = new Dictionary<string, BaseState>();

        private BaseState _currentState;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            foreach (BaseState state in _statesToRegister)
            {
                if (state != null) RegisterState(state);
            }

            if (IsServer && _startingState != null)
            {
                ChangeState(_startingState);
            }

            if (IsClient)
            {
                CurrentStateServerRpc();
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

        private void OnDrawGizmos()
        {
            if (_registeredStates == null || _registeredStates.Count == 0) return;

            foreach (BaseState state in _registeredStates.Values)
            {
                state.OnGizmos();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_registeredStates == null || _registeredStates.Count == 0) return;

            foreach (BaseState state in _registeredStates.Values)
            {
                state.OnGizmosSelected();
            }
        }

        public void RegisterState(BaseState state)
        {
            if (CanRegisterState(state))
            {
                BaseState copiedState = Instantiate(state);
                state.Initilize(this, gameObject);
                _registeredStates.Add(state.ID, state);
            }
        }

        private bool CanRegisterState(BaseState state)
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

        public bool IsCurrentState(BaseState state)
        {
            return _registeredStates.ContainsKey(state.ID) && state == _currentState;
        }

        public void ChangeState(BaseState state)
        {
            if (IsServer)
            {
                ChangeState(state.ID);
                OnChangeStateClientRpc(state.ID);
            }
        }

        [ClientRpc(RequireOwnership = false)]
        public void OnChangeStateClientRpc(string stateID)
        {
            ChangeState(stateID);
        }

        private void ChangeState(string stateID)
        {
            if (CanChangeState(stateID))
            {
                if (_currentState != null) _currentState.Exit();
                _currentState = _registeredStates[stateID];
                if (_currentState != null) _currentState.Enter();
            }
        }

        private bool CanChangeState(string stateID)
        {
            if (!_registeredStates.ContainsKey(stateID))
            {
                Debug.LogWarning($"[StateMachine] Could not change state: ID {stateID} has not been registered.");
                return false;
            }
            return _currentState != _registeredStates[stateID];
        }

        [ServerRpc(RequireOwnership = false)]
        private void CurrentStateServerRpc()
        {
            OnChangeStateClientRpc(_currentState.ID);
        }

    }
}
