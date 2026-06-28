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
    public UpgradeScriptableOBJ[] entireUpgradePool;

    [Header("Other")]
    [SerializeField] private UpgradeScriptableOBJ[] availableUpgrades;
    [SerializeField] private GameObject upgradeChoiceUI;
    [SerializeField] private int amountOfUpgradesToBeAvailable = 3;

    [SerializeField] private GameObject[] spawnpoints;

    private bool upgradeSelected;
    private int[] availableUpgradeIndexes = new int[3];

    private readonly List<GameObject> spawnedUpgradeObjects = new();

    public static UpgradeManager Instance { get; private set; }

    [SerializeField] private NetworkList<int> sharedSpawnPool = new();

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

        rollRandomUpgradeClientRPC();
    }

    [ClientRpc]
    private void rollRandomUpgradeClientRPC()
    {
        rollRandomUpgrade();
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
        Debug.Log($"Scene changed on {OwnerClientId}: {newScene.name}");

        Debug.Log($"Local:{NetworkManager.Singleton.LocalClientId} " +$"Owner:{OwnerClientId} " +$"IsOwner:{IsOwner}");
        
        rollRandomUpgrade();

        StartCoroutine(RefreshSpawnPoints());
    }

    private IEnumerator RefreshSpawnPoints()
    {
        // Wait one frame so the new scene can finish loading
        yield return null;

        spawnpoints = GameObject.FindGameObjectsWithTag("PowerSpawnPoints");

        //Debug.Log($"UpgradeManager found {spawnpoints.Length} spawn points.");
    }

    public void rollRandomUpgrade()
    {
        Debug.Log("Entered rollRandomUpgrade");
        upgradeSelected = false;
        Debug.Log($"upgradeSelected reset to {upgradeSelected}");
        upgradeChoiceUI.SetActive(true);

        availableUpgrades = new UpgradeScriptableOBJ[amountOfUpgradesToBeAvailable];

        for (int i = 0; i < amountOfUpgradesToBeAvailable; i++)
        {
            int randomUpgrade = Random.Range(0, entireUpgradePool.Length);

            availableUpgrades[i] = entireUpgradePool[randomUpgrade];
            availableUpgradeIndexes[i] = randomUpgrade;

            IconSprites[i].sprite = availableUpgrades[i].IconImage;
            upgradeNames[i].text = availableUpgrades[i].name;
        }
    }

    private void Update()
    {

        Debug.Log($"upgradeSelected = {upgradeSelected}");

        if (GameInput.instance.getSelectUpgradeOneInput() && !upgradeSelected)
        {
            SelectUpgrade(availableUpgradeIndexes[0]);
        }

        if (GameInput.instance.getSelectUpgradeTwoInput() && !upgradeSelected)
        {
            SelectUpgrade(availableUpgradeIndexes[1]);
        }

        if (GameInput.instance.getSelectUpgradeThreeInput() && !upgradeSelected)
        {
            SelectUpgrade(availableUpgradeIndexes[2]);
        }
    }

    private void SelectUpgrade(int upgradeIndex)
    {
        //Debug.Log($"Selecting upgrade {upgradeIndex}");

        AddUpgradeToPoolServerRpc(upgradeIndex);

        upgradeChoiceUI.SetActive(false);
        upgradeSelected = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddUpgradeToPoolServerRpc(int upgradeIndex)
    {
        sharedSpawnPool.Add(upgradeIndex);

        Debug.Log($"Player added upgrade index {upgradeIndex} to shared pool.");
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

        // Clear references from previous round
        spawnedUpgradeObjects.Clear();

        // Shuffle spawn points instead of spawned objects
        ShuffleSpawnPoints();

        int spawnCount = Mathf.Min(sharedSpawnPool.Count, spawnpoints.Length);

        for (int i = 0; i < spawnCount; i++)
        {
            int upgradeIndex = sharedSpawnPool[i];
            UpgradeScriptableOBJ upgradeData = entireUpgradePool[upgradeIndex];

            Vector3 spawnPos = spawnpoints[i].transform.position;

            GameObject newUpgrade = Instantiate(upgradeData.pickupObject,spawnPos, Quaternion.identity);

            NetworkObject netObj = newUpgrade.GetComponent<NetworkObject>();

            if (netObj != null)
            {
                // Spawn AFTER position is set
                netObj.Spawn();
            }

            spawnedUpgradeObjects.Add(newUpgrade);

            Debug.Log($"Spawned {newUpgrade.name} at {spawnPos}");
        }
    }

    private void ShuffleSpawnPoints()
    {
        for (int i = 0; i < spawnpoints.Length; i++)
        {
            int randomIndex = Random.Range(i, spawnpoints.Length);

            GameObject temp = spawnpoints[i];
            spawnpoints[i] = spawnpoints[randomIndex];
            spawnpoints[randomIndex] = temp;
        }
    }
}