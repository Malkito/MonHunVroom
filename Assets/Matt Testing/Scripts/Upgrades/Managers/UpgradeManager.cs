using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UpgradeManager : MonoBehaviour
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

    private List<UpgradeScriptableOBJ> objectsToSpawn = new List<UpgradeScriptableOBJ>(); //private list used to edit the array of spawn points



    public void rollRandomUpgrade() // Main logic, rolls the upgreades and displays them to the player
    {
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

    public void FirstUpgrade() // runs when first upgrade button is clicked
    {
        addObjectToSpawnPool(availbleUpgrades[0]); // adds first upgrade to spawn pool
    }
    public void SecondUpgrade() // runs when second upgrade button is clicked
    {
        addObjectToSpawnPool(availbleUpgrades[1]); // adds second upgrade to spawn pool
    }

    public void ThirdUpgrade()// runs when third upgrade button is clicked
    {
        addObjectToSpawnPool(availbleUpgrades[2]);// adds third upgrade to spawn pool
    }

    private void addObjectToSpawnPool(UpgradeScriptableOBJ obj) // add Scriptabel OBJ to the spawn pool
    {
        objectsToSpawn.Add(obj); // adds the object to the internal list
        spawnPool = (changeListIntoArray(objectsToSpawn)); // restruns the list in array form
        upgradeChoiceUI.SetActive(false); // turns off UI
    }

    private UpgradeScriptableOBJ[] changeListIntoArray(List<UpgradeScriptableOBJ> objList) // returns given list as an array
    {
        return objList.ToArray();
    }




}
