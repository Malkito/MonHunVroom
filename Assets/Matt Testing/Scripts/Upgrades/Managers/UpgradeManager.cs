using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;
public class UpgradeManager : NetworkBehaviour
{

    /// Handles rollign random upgrades, displaying upgrades to UI, adds chosen upgrades to spawn pool
    /// idk how this will work with multiplayer though :)

    [Header("Displayer Arrays")]
    [SerializeField] private Image[] IconSprites; // Icons for each upgrade
    [SerializeField] private TMP_Text[] upgradeNames; // Names for each availbe upgrades


    [Header("Public varibles")]
    public UpgradeScriptableOBJ[] spawnPool; // upgrades to be spawned on the map, can be added to by all players
    public UpgradeScriptableOBJ[] entireUpgradePool; // the enitre pool of upgrades 

    [Header("Other")]
    [SerializeField] private UpgradeScriptableOBJ[] availbleUpgrades; // upgrades available to the player for them to pick one from
    [SerializeField] private GameObject upgradeChoiceUI; // the upgrade UI. Has 3 icons, names and buttons, one for each availble upgrade
    [SerializeField] int amountOfUpgradesToBeAvailble;// Number of upgrades to be availble. Set in editor, set to 3

    [SerializeField] private GameObject[] spawnpoints;
    [SerializeField] private GameObject[] upgrades;

    private bool upgradeSelected;


    private List<UpgradeScriptableOBJ> objectsToSpawn = new List<UpgradeScriptableOBJ>(); //private list used to edit the array of spawn points

    private List<GameObject> spawnPoolPickUpObjects = new List<GameObject>();

    public static UpgradeManager Instance { get; private set; }

    public override void OnNetworkSpawn()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Awake()
    {
        rollRandomUpgrade();
    }
    void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    void OnActiveSceneChanged(Scene previousScene, Scene newScene)
    {
        rollRandomUpgrade();
        spawnpoints = GameObject.FindGameObjectsWithTag("PowerSpawnPoints");
    }

    public void rollRandomUpgrade() // Main logic, rolls the upgreades and displays them to the player
    {
        upgradeSelected = false;
        upgradeChoiceUI.SetActive(true); // turns on the UI
        availbleUpgrades = new UpgradeScriptableOBJ[amountOfUpgradesToBeAvailble]; // sets the length of the availble upgrages 

        for (int i = 0; i < amountOfUpgradesToBeAvailble; i++)
        {
            int randomUpgrade = Random.Range(0, entireUpgradePool.Length); // chooses a random upgrade from the list

            availbleUpgrades[i] = entireUpgradePool[randomUpgrade]; // sets the current avaible upgrade to the randomly chosen upgrade

            IconSprites[i].sprite = availbleUpgrades[i].IconImage; //displays the upgrade image to the appropriate upgrade
            upgradeNames[i].text = availbleUpgrades[i].name; //displays the upgrade name to the appropriate upgrade
        }
    }

    private void Update()
    {

        if (GameInput.instance.getSelectUpgradeOneInput() && !upgradeSelected)
        {
            addObjectToSpawnPool(availbleUpgrades[0]);
        }

        if (GameInput.instance.getSelectUpgradeTwoInput() && !upgradeSelected)
        {
            addObjectToSpawnPool(availbleUpgrades[1]);
        }

        if (GameInput.instance.getSelectUpgradeThreeInput() && !upgradeSelected)
        {
            addObjectToSpawnPool(availbleUpgrades[2]);
        }

    }

    private void addObjectToSpawnPool(UpgradeScriptableOBJ obj) // add Scriptabel OBJ to the spawn pool
    {
        objectsToSpawn.Add(obj); // adds the object to the internal list
        spawnPool = (changeListIntoArray(objectsToSpawn)); // restruns the list in array form
        upgradeChoiceUI.SetActive(false); // turns off UI
        upgradeSelected = true;
    }

    private UpgradeScriptableOBJ[] changeListIntoArray(List<UpgradeScriptableOBJ> objList) // returns given list as an array
    {
        return objList.ToArray();
    }

    [ServerRpc]
    public void spawnUpgradesServerRpc()
    {
        foreach(UpgradeScriptableOBJ pickupObject in spawnPool){
            GameObject newUpgrade = Instantiate(pickupObject.pickupObject);

            spawnPoolPickUpObjects.Add(newUpgrade);

            upgrades = spawnPoolPickUpObjects.ToArray();

            NetworkObject netOBj = pickupObject.pickupObject.GetComponent<NetworkObject>();
            netOBj.Spawn();
        }

        shuffleUpgradeArray();


        for (int i = 0; i < upgrades.Length; i++)
        {
            upgrades[i].transform.position = spawnpoints[i].transform.position;
        }
    }
    public void shuffleUpgradeArray()
    {
        for (int i = 0; i < upgrades.Length; i++)
        {
            int randomIndex = Random.Range(i, upgrades.Length);

            GameObject temp = upgrades[i];
            upgrades[i] = upgrades[randomIndex];
            upgrades[randomIndex] = temp;
        }
    }
}
