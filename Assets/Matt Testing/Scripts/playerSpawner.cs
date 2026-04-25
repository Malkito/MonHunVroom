using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


public class playerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject Player;



    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoaded;
    }

    private void SceneLoaded(string sceneName, LoadSceneMode loadScneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        print("SCENE LAODEADEAWDEAAED ");

        if (IsHost && (sceneName == "DayGameScene" || sceneName == "NightGameScene")){

            foreach(ulong id in clientsCompleted)
            {
                GameObject player = Instantiate(Player);
                player.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
            }
        }
    }
}
