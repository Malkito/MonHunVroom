using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;

public class readyCheck : NetworkBehaviour
{
    [SerializeField] private GameObject readyCanvas;
    [SerializeField] private Image[] readyCheckImages;
    [SerializeField] private Button readyButton;
    [SerializeField] private GameObject playerPrefabs;
    [SerializeField] private Transform[] respawnPoints;

    private readonly HashSet<ulong> readyClientIds = new();
    private bool ready;
    private bool playersSpawned;

    private void Awake()
    {
        readyButton.onClick.AddListener(ReadyPressed);
    }

    private void Start()
    {
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count && i < readyCheckImages.Length; i++)
            readyCheckImages[i].gameObject.SetActive(true);

        GameStateManager.Instance.setNewState(GameStateManager.State.WaitingToStart);
    }

    private void Update()
    {
        if (IsSpawned && !ready && GameInput.instance.getJumpInput())
            ReadyPressed();
    }

    private void ReadyPressed()
    {
        if (!IsSpawned || ready)
            return;

        ready = true;
        readyButton.interactable = false;
        ReadyPressedServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReadyPressedServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (!readyClientIds.Add(clientId))
            return;

        readyCheckImagesClientRpc(readyClientIds.Count - 1);
        if (readyClientIds.Count == NetworkManager.Singleton.ConnectedClientsIds.Count)
        {
            readyCanvas.SetActive(false);
            turnOffReadyUIClientRpc();
            UpgradeManager.Instance.ShowUpgradeChoices();
        }
    }

    [ClientRpc]
    private void readyCheckImagesClientRpc(int index)
    {
        if (index < readyCheckImages.Length)
            readyCheckImages[index].color = Color.green;
    }

    [ClientRpc]
    private void turnOffReadyUIClientRpc()
    {
        readyCanvas.SetActive(false);
    }

    public void SpawnPlayersAfterChoices()
    {
        if (!IsServer || playersSpawned)
            return;

        playersSpawned = true;
        int spawnIndex = 0;
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (spawnIndex >= respawnPoints.Length)
            {
                Debug.LogError("Not enough player respawn points.");
                return;
            }

            GameObject player = Instantiate(playerPrefabs, respawnPoints[spawnIndex].position, respawnPoints[spawnIndex].rotation);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId, true);
            spawnIndex++;
        }

        setGameStateClientRpc();
        UpgradeManager.Instance.SpawnUpgradesServerRpc();
    }

    [ClientRpc]
    private void setGameStateClientRpc()
    {
        GameStateManager.Instance.setNewState(GameStateManager.State.GamePlaying);
    }
}