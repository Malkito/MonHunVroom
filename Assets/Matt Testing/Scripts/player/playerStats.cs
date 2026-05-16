using UnityEngine;
using Unity.Netcode;


public class playerStats : NetworkBehaviour
{
    // GOES ON PLAYER


    public float BaseDamageBonus = 0;
    public float BaseSpeedBonus = 0;
    public float BaseHealthBonus = 0;
    public float BaseFireRateReduction = 1;
    public float BaseCooldownReduction = 1;
    public float BasePowerUpBoost = 0;
    public float BaseSpecialBoost = 0;

    public float currentDamage;
    public float currentSpeed;
    public float currentHealth;
    public float currentFireRateReduction;
    public float currentCooldownReduction;
    public float currentPowerUpBoost;
    public float currentSpecialBoost;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        ApplyUpgrades();
    }

    public void ApplyUpgrades()
    {
        ulong clientId = OwnerClientId;

        playerUpgradeData upgrades = playerStatUpgradeManager.Instance.GetPlayerData(clientId);

        currentDamage = BaseDamageBonus + upgrades.damageBonus;
        currentSpeed = BaseSpeedBonus + upgrades.speedBonus;
        currentHealth = BaseHealthBonus + upgrades.healthBonus;
        currentFireRateReduction = BaseFireRateReduction *= upgrades.fireRateReduction;
        currentCooldownReduction = BaseCooldownReduction *= upgrades.cooldownReduction;
        currentPowerUpBoost = BasePowerUpBoost += upgrades.powerUpBoost;
        currentSpecialBoost = BaseSpecialBoost += upgrades.specialBoost;




        Debug.Log($"Applied upgrades for player {clientId}");
    }
}
