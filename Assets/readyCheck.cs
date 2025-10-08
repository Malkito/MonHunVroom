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
    private int spawnIndex;
    [SerializeField] Animator ac;
    private bool ready;
    [SerializeField] private Material[] tankColors;
    [SerializeField] private GameObject[] playerObjects;
    private int colorindex;


    private void Awake()
    {
        readyButton.onClick.AddListener(() => {
            readyPressed();
        });
    }
    void Start()
    {
        spawnIndex = 0;
        playersSpawned = false;
        numOfPlayers = 0;
        foreach(NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            readyCheckImages[numOfPlayers].gameObject.SetActive(true);
            numOfPlayers++;
        }

        GameStateManager.Instance.setNewState(GameStateManager.State.WaitingToStart);
    }
    private void Update()
    {

        if (GameInput.instance.getJumpInput() && !ready)
        {
            readyPressed();
            ready = true;
        }


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
        readyCheckImagesClientRpc();
        numOfPlayersReady++;
        print("Ready server pressed Client RPC ran");
    }

    [ClientRpc]
    private void readyCheckImagesClientRpc()
    {
        readyCheckImages[numOfPlayersReady].color = Color.green;
    }

    private void spawnPlayers()
    {
        ac.SetBool("GameStarting", true);

        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
           
            Transform spawnPoint = respawnManager.Instance.respawnPoints[spawnIndex].transform;
            GameObject player = Instantiate(playerPrefabs, spawnPoint);


            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId, true);
            spawnIndex++;
        }

        setPlayerColorsClientRpc();
        turnOffReadyUIClientRpc();

        playersSpawned = true;

        GameStateManager.Instance.setNewState(GameStateManager.State.GamePlaying);

    }
    [ClientRpc]
    private void turnOffReadyUIClientRpc()
    {
        readyCanvas.SetActive(false);
    }

    [ClientRpc]
    private void setPlayerColorsClientRpc()
    {
        foreach (var playerObj in FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
        {
            if (playerObj.CompareTag("Player"))
            {
                MeshRenderer[] meshes = playerObj.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer mesh in meshes)
                {
                    mesh.materials[0] = tankColors[colorindex];
                }
                print("COLOR INDEX " + colorindex);
                colorindex++;
            }
        }
    }
}
