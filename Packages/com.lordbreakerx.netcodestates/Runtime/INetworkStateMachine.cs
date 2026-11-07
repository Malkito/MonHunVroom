using LordBreakerX.States;
using UnityEngine;

namespace LordBreakerX.NetcodeStates
{
    public interface INetworkStateMachine : IStateMachine
    {
        public bool IsServer { get; }
        public bool IsHost { get; }
        public bool IsClient { get; }
        public bool IsOwner { get; }
    }
}
