using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private GameObject[] spawnedUpgrades;

    private bool upgradeSelected;
    private int[] availableUpgradeIndexes = new int[3];

    private List<GameObject> spawnedUpgradeObjects = new List<GameObject>();

    public static UpgradeManager Instance { get; private set; }

    // Shared upgrade pool (stores indexes of upgrades)
    [SerializeField] private NetworkList<int> sharedSpawnPool = new NetworkList<int>();

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


        if (IsOwner)
        {
            rollRandomUpgrade();
        }

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
        rollRandomUpgrade();
        spawnpoints = GameObject.FindGameObjectsWithTag("PowerSpawnPoints");
    }

    public void rollRandomUpgrade()
    {
        upgradeSelected = false;
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
        //if (!IsOwner) return;

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
        AddUpgradeToPoolServerRpc(upgradeIndex);

        upgradeChoiceUI.SetActive(false);
        upgradeSelected = true;
    }

    // Client asks server to add upgrade
    [ServerRpc(RequireOwnership = false)]
    private void AddUpgradeToPoolServerRpc(int upgradeIndex, ServerRpcParams rpcParams = default)
    {
        sharedSpawnPool.Add(upgradeIndex);

        Debug.Log($"Player added upgrade index {upgradeIndex} to shared pool.");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnUpgradesServerRpc()
    {
        if (!IsServer) return;

        foreach (int upgradeIndex in sharedSpawnPool)
        {
            UpgradeScriptableOBJ upgradeData = entireUpgradePool[upgradeIndex];

            GameObject newUpgrade = Instantiate(
                upgradeData.pickupObject,
                Vector3.zero,
                Quaternion.identity
            );

            NetworkObject netObj = newUpgrade.GetComponent<NetworkObject>();

            if (netObj != null)
            {
                netObj.Spawn();
            }

            spawnedUpgradeObjects.Add(newUpgrade);
        }

        spawnedUpgrades = spawnedUpgradeObjects.ToArray();

        ShuffleUpgradeArray();

        for (int i = 0; i < spawnedUpgrades.Length && i < spawnpoints.Length; i++)
        {
            spawnedUpgrades[i].transform.position =
                spawnpoints[i].transform.position;
        }
    }

    private void ShuffleUpgradeArray()
    {
        for (int i = 0; i < spawnedUpgrades.Length; i++)
        {
            int randomIndex = Random.Range(i, spawnedUpgrades.Length);

            GameObject temp = spawnedUpgrades[i];
            spawnedUpgrades[i] = spawnedUpgrades[randomIndex];
            spawnedUpgrades[randomIndex] = temp;
        }
    }
}