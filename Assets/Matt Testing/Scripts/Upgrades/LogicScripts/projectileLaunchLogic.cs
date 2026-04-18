using UnityEngine;
using Unity.Netcode;

public class projectileLaunchLogic : NetworkBehaviour, useAbility, onUpgradePickedup, onUpgradeDropped
{
    [Header("Settings")]
    public int bulletArrayIndex;

    private playerShooting shootingScript;
    private Transform player;

    // Called when the upgrade is picked up
    // (PlayerUpgradeManager tells us)
    public void onUpgradePickedup(Transform playerTransform)
    {
        player = playerTransform;
        shootingScript = player.GetComponent<playerShooting>();

        if (shootingScript == null)
        {
            Debug.LogError("PlayerShooting script NOT found on player!");
        }
    }

    // Called when upgrade is dropped
    public void onUpgradeDropped(Transform playerTransform)
    {
        shootingScript = null;
        player = null;
    }

    // Called every frame by PlayerUpgradeManager
    // abilityPressed = input
    // cooldownReady = PlayerUpgradeManager controls fire rate
    public void useAbility(Transform playerTransform, bool abilityPressed)
    {
        Debug.Log("Ability triggered: " + name);

        if (!abilityPressed)
            return;

        if (shootingScript == null)
        {
            Debug.LogError("shootingScript is NULL - upgrade not initialized");
        }
        // Fire on server
        shootingScript.AltShootServerRPC(bulletArrayIndex);

    }
}
