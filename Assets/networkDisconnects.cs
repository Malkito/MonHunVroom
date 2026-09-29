using UnityEngine;
using Unity.Netcode;
using System;

public class networkDisconnects : NetworkBehaviour
{

    private void Start()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetwrokManager_OnClientDisconnectCallback;
        }
    }

    private void NetwrokManager_OnClientDisconnectCallback(ulong clientID)
    {
        if(clientID == OwnerClientId)
        {
            GetComponent<playerAbilityManager>().dropAllUpgrades();
            Destroy(gameObject);
        }
    }
}
