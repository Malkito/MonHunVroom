using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class playerStatUpgradeManager : MonoBehaviour
{
    // GOES IN THE SCENE


    public static playerStatUpgradeManager Instance;

    private Dictionary<ulong, playerUpgradeData> playerUpgrades = new Dictionary<ulong, playerUpgradeData>();

    public enum UpgradeType
    {
        Damage,
        Speed,
        Health,
        fireRate,
        cooldownReduction,
        powerUpBoost,
        specialBoost
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public playerUpgradeData GetPlayerData(ulong clientId)
    {
        if (!playerUpgrades.ContainsKey(clientId))
        {
            playerUpgrades[clientId] = new playerUpgradeData();
        }

        return playerUpgrades[clientId];
    }

    public void AddUpgrade(ulong clientId, UpgradeType type, float amount)
    {
        playerUpgradeData data = GetPlayerData(clientId);

        switch (type)
        {
            case UpgradeType.Damage:
                data.damageBonus += amount;
                break;
            case UpgradeType.Speed:
                data.speedBonus += amount;
                break;
            case UpgradeType.Health:
                data.healthBonus += amount;
                break;
            case UpgradeType.fireRate:
                data.fireRateReduction += amount;
                break;
            case UpgradeType.cooldownReduction:
                data.cooldownReduction += amount;
                break;
            case UpgradeType.powerUpBoost:
                data.powerUpBoost += amount;
                break;
            case UpgradeType.specialBoost:
                data.specialBoost += amount;
                break;
        }
    }
}
