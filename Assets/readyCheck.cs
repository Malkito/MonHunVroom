using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class readyCheck : NetworkBehaviour
{
    [SerializeField] private GameObject readyCanvas;
    [SerializeField] private Image[] readyCheckImages;
    [SerializeField] private Button readyButton;
    private int numOfPlayers;
    private int numOfPlayersReady;
    [SerializeField] GameObject playerPrefabs;
    private bool playersSpawned;



    private void Awake()
    {
        readyButton.onClick.AddListener(() => {
            readyPressed();
        });
    }

    void Start()
    {
        playersSpawned = false;
        numOfPlayers = 0;
        foreach(NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            readyCheckImages[numOfPlayers].gameObject.SetActive(true);
            numOfPlayers++;
        }
    }


    private void Update()
    {
        if(numOfPlayersReady == NetworkManager.Singleton.ConnectedClientsIds.Count && !playersSpawned)
        {
            spawnPlayers();
        }
    }

    private void readyPressed()
    {
        readyButton.interactable = false;
        readyPressedServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void readyPressedServerRpc()
    {
        readyCheckImages[numOfPlayersReady].color = Color.green;
        numOfPlayersReady++;
    }

    private void spawnPlayers()
    {

        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            Transform spawnPoint = respawnManager.Instance.respawnPoints[Random.Range(0, respawnManager.Instance.respawnPoints.Length)].transform;
            GameObject player = Instantiate(playerPrefabs, spawnPoint);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId, true);
        }
        playersSpawned = true;
        readyCanvas.SetActive(false);
    }


}
