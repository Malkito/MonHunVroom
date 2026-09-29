using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System;

public class HostDisconnectUI : MonoBehaviour
{


    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallbak;
        gameObject.SetActive(false);
    }

    private void NetworkManager_OnClientDisconnectCallbak(ulong clietnID)
    {
        if(clietnID == NetworkManager.ServerClientId)
        {
            gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;

        }


    }
}
