using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    public struct AttackNetworkContext
    {
        public bool IsServer { get; private set; }
        public bool IsClient { get; private set; }
        public bool IsOwner { get; private set; }

        public AttackNetworkContext(bool isServer, bool isClient, bool isOwner)
        {
            IsServer = isServer;
            
            IsClient = isClient;

            IsOwner = isOwner;
        }
    }
}
