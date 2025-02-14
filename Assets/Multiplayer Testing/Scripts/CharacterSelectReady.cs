using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSelectReady : NetworkBehaviour
{

    public static CharacterSelectReady Instance { get; private set; }

    private Dictionary<ulong, bool> playerReadyDisctionary;

    private void Awake()
    {
        Instance = this;
        playerReadyDisctionary = new Dictionary<ulong, bool>();

    }

    public void SetPLayerReady()
    {

        SetPLayerReadyServerRpc();


    }


    [ServerRpc(RequireOwnership = false)]
    private void SetPLayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDisctionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if(!playerReadyDisctionary.ContainsKey(clientId) || !playerReadyDisctionary[clientId])
            {
                //ths player is NOT ready
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            Loader.LoadNetwork(Loader.Scene.PVPScene);
        }


    }




}
