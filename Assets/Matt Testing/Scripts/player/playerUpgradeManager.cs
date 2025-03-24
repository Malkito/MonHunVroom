using UnityEngine;
using System.Collections.Generic;
using System;
public class playerUpgradeManager : MonoBehaviour
{
    public UpgradeScriptableOBJ[] currentUpgrades;
    List<UpgradeScriptableOBJ> currentUpgradesList = new List<UpgradeScriptableOBJ>();

    public void addToPlayerUpgrades(UpgradeScriptableOBJ upgradeToAdd)
    {
        currentUpgradesList.Add(upgradeToAdd);
        currentUpgrades = currentUpgradesList.ToArray();

    }

    public void deletePlayerUpgrade(UpgradeScriptableOBJ upgradeToDelete)
    {
        currentUpgradesList.Remove(upgradeToDelete);
        currentUpgrades = currentUpgradesList.ToArray();
    }


}
