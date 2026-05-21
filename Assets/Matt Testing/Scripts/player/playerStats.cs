using UnityEngine;
using Unity.Netcode;

public class playerStats : NetworkBehaviour
{
    // GOES ON PLAYER

    [Header("Base Stats")]
    private float BaseDamageBonus = 1;
    private float BaseSpeedBonus = 1;
    private float BaseHealthBonus = 1;
    private float BaseFireRateReduction = 1;
    private float BaseCooldownReduction = 1;
    private float BasePowerUpBoost = 0;
    private float BaseSpecialBoost = 0;

    [Header("Current Networked Stats")]
    public NetworkVariable<float> currentDamage = new NetworkVariable<float>();

    public NetworkVariable<float> currentSpeed = new NetworkVariable<float>();

    public NetworkVariable<float> currentHealth = new NetworkVariable<float>();

    public NetworkVariable<float> currentFireRateReduction = new NetworkVariable<float>();

    public NetworkVariable<float> currentCooldownReduction = new NetworkVariable<float>();

    public NetworkVariable<float> currentPowerUpBoost =  new NetworkVariable<float>();

    public NetworkVariable<float> currentSpecialBoost = new NetworkVariable<float>();

    public override void OnNetworkSpawn()
    {
        // ONLY server should apply authoritative stats
        if (!IsServer) return;

        ApplyUpgrades();
    }

    public void ApplyUpgrades()
    {
        // ONLY server should modify NetworkVariables
        if (!IsServer) return;

        ulong clientId = OwnerClientId;

        playerUpgradeData upgrades = playerStatUpgradeManager.Instance.GetPlayerData(clientId);



        currentDamage.Value = BaseDamageBonus + upgrades.damageBonus;

        currentSpeed.Value = BaseSpeedBonus + upgrades.speedBonus;

        currentHealth.Value = BaseHealthBonus + upgrades.healthBonus;

        currentFireRateReduction.Value = BaseFireRateReduction + upgrades.fireRateReduction;

        currentCooldownReduction.Value = BaseCooldownReduction + upgrades.cooldownReduction;

        currentPowerUpBoost.Value = BasePowerUpBoost + upgrades.powerUpBoost;

        currentSpecialBoost.Value = BaseSpecialBoost + upgrades.specialBoost;

        GetComponent<playerHealth>().applyHealthChanged();

        Debug.Log($"Applied upgrades for player {clientId}");
    }
}