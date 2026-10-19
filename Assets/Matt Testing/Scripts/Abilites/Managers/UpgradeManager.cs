using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class UpgradeManager : NetworkBehaviour
{
    [Header("Displayer Arrays")]
    [SerializeField] private Image[] IconSprites;
    [SerializeField] private TMP_Text[] upgradeNames;

    [Header("Upgrade Pools")]
    public AbilityScriptableOBJ[] entireUpgradePool;

    [Header("Other")]
    [SerializeField] private AbilityScriptableOBJ[] availableUpgrades;
    [SerializeField] private GameObject upgradeChoiceUI;
    [SerializeField] private int amountOfUpgradesToBeAvailable = 3;
    [SerializeField] private GameObject[] spawnpoints;

    private bool upgradeSelected;
    private int[] availableUpgradeIndexes = new int[3];
    private readonly List<GameObject> spawnedUpgradeObjects = new();
    private readonly HashSet<ulong> clientsWhoSelected = new();

    public static UpgradeManager Instance { get; private set; }

    [SerializeField] private NetworkList<int> sharedSpawnPool = new();

    private void Awake()
    {
        upgradeChoiceUI.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        availableUpgradeIndexes = new int[amountOfUpgradesToBeAvailable];
        StartCoroutine(RefreshSpawnPoints());
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene previousScene, Scene newScene)
    {
        upgradeChoiceUI.SetActive(false);
        StartCoroutine(RefreshSpawnPoints());
    }

    private IEnumerator RefreshSpawnPoints()
    {
        yield return null;
        spawnpoints = GameObject.FindGameObjectsWithTag("PowerSpawnPoints");
    }

    public void ShowUpgradeChoices()
    {
        if (!IsServer)
            return;

        clientsWhoSelected.Clear();
        sharedSpawnPool.Clear();
        upgradeSelected = false;
        for (int i = 0; i < amountOfUpgradesToBeAvailable; i++)
            availableUpgradeIndexes[i] = Random.Range(0, entireUpgradePool.Length);

        ShowUpgradeChoicesClientRpc(availableUpgradeIndexes);
    }

    [ClientRpc]
    private void ShowUpgradeChoicesClientRpc(int[] upgradeIndexes)
    {
        availableUpgradeIndexes = upgradeIndexes;
        upgradeSelected = false;
        for (int i = 0; i < upgradeIndexes.Length; i++)
        {
            AbilityScriptableOBJ upgrade = entireUpgradePool[upgradeIndexes[i]];
            IconSprites[i].sprite = upgrade.IconImage;
            IconSprites[i].color = upgrade.abilityColor;
            upgradeNames[i].text = upgrade.name;
        }

        upgradeChoiceUI.SetActive(true);
    }

    private void Update()
    {
        if (!IsSpawned || upgradeSelected)
            return;

        if (GameInput.instance.getSelectUpgradeOneInput()) SelectUpgrade(0);
        else if (GameInput.instance.getSelectUpgradeTwoInput()) SelectUpgrade(1);
        else if (GameInput.instance.getSelectUpgradeThreeInput()) SelectUpgrade(2);
    }

    // Assign this method to a button and pass the displayed upgrade slot (0-based).
    public void SelectUpgrade(int slot)
    {
        if (!IsSpawned || upgradeSelected || slot < 0 || slot >= availableUpgradeIndexes.Length)
            return;

        upgradeSelected = true;
        upgradeChoiceUI.SetActive(false);
        SelectUpgradeServerRpc(slot);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectUpgradeServerRpc(int slot, ServerRpcParams rpcParams = default)
    {
        if (slot < 0 || slot >= availableUpgradeIndexes.Length)
            return;

        ulong clientId = rpcParams.Receive.SenderClientId;
        if (!clientsWhoSelected.Add(clientId))
            return;

        sharedSpawnPool.Add(availableUpgradeIndexes[slot]);
        if (clientsWhoSelected.Count == NetworkManager.Singleton.ConnectedClientsIds.Count)
            FindAnyObjectByType<readyCheck>().SpawnPlayersAfterChoices();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnUpgradesServerRpc()
    {
        if (!IsServer)
            return;

        if (spawnpoints == null || spawnpoints.Length == 0)
        {
            Debug.LogWarning("No spawn points found! Cannot spawn upgrades.");
            return;
        }

        spawnedUpgradeObjects.Clear();
        ShuffleSpawnPoints();

        int spawnCount = Mathf.Min(sharedSpawnPool.Count, spawnpoints.Length);
        for (int i = 0; i < spawnCount; i++)
        {
            AbilityScriptableOBJ upgradeData = entireUpgradePool[sharedSpawnPool[i]];
            Vector3 spawnPos = spawnpoints[i].transform.position;
            GameObject newUpgrade = Instantiate(upgradeData.pickupObject, spawnPos, Quaternion.identity);
            NetworkObject netObj = newUpgrade.GetComponent<NetworkObject>();
            if (netObj != null)
                netObj.Spawn();

            spawnedUpgradeObjects.Add(newUpgrade);
        }
    }

    private void ShuffleSpawnPoints()
    {
        for (int i = 0; i < spawnpoints.Length; i++)
        {
            int randomIndex = Random.Range(i, spawnpoints.Length);
            (spawnpoints[i], spawnpoints[randomIndex]) = (spawnpoints[randomIndex], spawnpoints[i]);
        }
    }
}