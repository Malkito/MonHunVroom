using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class SyncFragments : NetworkBehaviour
{
    private void Awake()
    {
        NetworkObject netObj =  gameObject.AddComponent<NetworkObject>();

        gameObject.AddComponent<NetworkTransform>();

        netObj.Spawn();
    }




}
