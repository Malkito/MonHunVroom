using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using Unity.Collections;

public struct ReadyEntry : INetworkSerializable, IEquatable<ReadyEntry>
{
    public ulong clientId;
    public FixedString64Bytes name;
    public bool ready;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref ready);
    }

    public bool Equals(ReadyEntry other) => clientId == other.clientId && name.Equals(other.name) && ready == other.ready;
}

public class readyCheck : NetworkBehaviour
{
    private const string SavedNameKey = "Multiplayer.PlayerName";

    [SerializeField] private GameObject readyCanvas;
    [SerializeField] private Image[] readyCheckImages;
    [SerializeField] private Button readyButton;
    [SerializeField] private TMP_Text readyButtonLabel;
    [SerializeField] private GameObject playerPrefabs;
    [SerializeField] private Transform[] respawnPoints;

    private readonly HashSet<ulong> readyClientIds = new();
    private readonly NetworkList<ReadyEntry> roster = new();
    private bool ready;
    private bool readyCheckDone;
    private bool playersSpawned;

    private void Awake()
    {
        readyButton.onClick.AddListener(ReadyPressed);
        UpdateButtonLabel();
    }

    private void Start()
    {
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count && i < readyCheckImages.Length; i++)
            readyCheckImages[i].gameObject.SetActive(true);

        GameStateManager.Instance.setNewState(GameStateManager.State.WaitingToStart);
    }

    public NetworkList<ReadyEntry> Roster => roster;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            foreach (ulong id in NetworkManager.ConnectedClientsIds)
                AddEntry(id);
            NetworkManager.OnClientConnectedCallback += AddEntry;
            NetworkManager.OnClientDisconnectCallback += RemoveEntry;
        }

        RegisterNameServerRpc(new FixedString64Bytes(LocalName()));
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager != null)
        {
            NetworkManager.OnClientConnectedCallback -= AddEntry;
            NetworkManager.OnClientDisconnectCallback -= RemoveEntry;
        }
    }

    private static string LocalName()
    {
        string saved = PlayerPrefs.GetString(SavedNameKey, "").Trim();
        return saved.Length > 0 ? saved : "Player";
    }

    private int FindEntry(ulong clientId)
    {
        for (int i = 0; i < roster.Count; i++)
            if (roster[i].clientId == clientId)
                return i;
        return -1;
    }

    private void AddEntry(ulong clientId)
    {
        if (FindEntry(clientId) >= 0)
            return;
        roster.Add(new ReadyEntry { clientId = clientId, name = new FixedString64Bytes("Player"), ready = false });
    }

    private void RemoveEntry(ulong clientId)
    {
        int index = FindEntry(clientId);
        if (index >= 0)
            roster.RemoveAt(index);
        readyClientIds.Remove(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RegisterNameServerRpc(FixedString64Bytes playerName, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        AddEntry(clientId);
        int index = FindEntry(clientId);
        ReadyEntry entry = roster[index];
        entry.name = playerName;
        roster[index] = entry;
    }

    private void Update()
    {
        if (IsSpawned && !ready && GameInput.instance.getJumpInput())
            ReadyPressed();
    }

    private void ReadyPressed() => SetReady(!ready);

    private void SetReady(bool value)
    {
        if (!IsSpawned || ready == value)
            return;

        ready = value;
        UpdateButtonLabel();
        SetReadyServerRpc(value);
    }

    private void UpdateButtonLabel()
    {
        if (readyButtonLabel)
            readyButtonLabel.text = ready ? "Cancel" : "Ready";
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetReadyServerRpc(bool value, ServerRpcParams rpcParams = default)
    {
        if (readyCheckDone)
            return;

        ulong clientId = rpcParams.Receive.SenderClientId;
        bool changed = value ? readyClientIds.Add(clientId) : readyClientIds.Remove(clientId);
        if (!changed)
            return;

        int entryIndex = FindEntry(clientId);
        if (entryIndex >= 0)
        {
            ReadyEntry entry = roster[entryIndex];
            entry.ready = value;
            roster[entryIndex] = entry;
        }

        readyCheckImagesClientRpc(readyClientIds.Count);
        if (value && readyClientIds.Count == NetworkManager.Singleton.ConnectedClientsIds.Count)
        {
            readyCheckDone = true;
            readyCanvas.SetActive(false);
            turnOffReadyUIClientRpc();
            UpgradeManager.Instance.ShowUpgradeChoices();
        }
    }

    [ClientRpc]
    private void readyCheckImagesClientRpc(int readyCount)
    {
        for (int i = 0; i < readyCheckImages.Length; i++)
            readyCheckImages[i].color = i < readyCount ? Color.green : Color.white;
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