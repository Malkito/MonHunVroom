using LordBreakerX.States;
using UnityEngine;

namespace LordBreakerX.NetcodeStates
{
    [System.Serializable]
    public abstract class NetworkState : State
    {
        private INetworkStateMachine _networkMachine;

        protected bool IsServer 
        { 
            get 
            {
                if (_networkMachine != null) return _networkMachine.IsServer;
#if UNITY_EDITOR
                Debug.LogWarning("Cannot check server status: StateMachine is not an INetworkStateMachine.");
#endif
                return false;
            }
        }

        protected bool IsHost
        {
            get
            {
                if (_networkMachine != null) return _networkMachine.IsHost;
#if UNITY_EDITOR
                Debug.LogWarning("Cannot check host status: StateMachine is not an INetworkStateMachine.");
#endif
                return false;
            }
        }

        protected bool IsClient
        {
            get
            {
                if (_networkMachine != null) return _networkMachine.IsClient;
#if UNITY_EDITOR
                Debug.LogWarning("Cannot check client status: StateMachine is not an INetworkStateMachine.");
#endif

                return false;
            }
        }

        protected bool IsOwner
        {
            get
            {
                if (_networkMachine != null) return _networkMachine.IsOwner;
#if UNITY_EDITOR
                Debug.LogWarning("Cannot check host status: StateMachine is not an INetworkStateMachine.");
#endif

                return false;
            }
        }

        public NetworkState() : base()
        {

        }

        public NetworkState(IStateMachine machine, NetworkState toCopy) : base(machine, toCopy)
        {
            _networkMachine = (INetworkStateMachine)machine;

#if UNITY_EDITOR
            if (_networkMachine == null)
            {
                Debug.LogError($"{ID} is a Network State and requires an INetworkStateMachine.");
            }
#endif
        }

        public abstract override State CreateCopy(IStateMachine machine);
    }
}
