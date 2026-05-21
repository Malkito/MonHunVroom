using UnityEngine;
using Unity.Netcode;
using System;

public interface useAbility
{
    void useAbility(Transform position, bool abilityPressed);
}

public interface onUpgradeDropped
{
    void onUpgradeDropped(Transform player);
}

public interface onUpgradePickedup
{
    void onUpgradePickedup(Transform player);
}

[Serializable]
public class EquippedUpgrade
{
    public int upgradeID;                 // INT ID ONLY
    public NetworkObject logicInstance;
    public useAbility logicScript;
    public float cooldownRemaining;

    public bool IsValid => logicInstance != null;
}

public class playerUpgradeManager : NetworkBehaviour
{
    [Header("Runtime Slots")]
    public EquippedUpgrade[] equipped = new EquippedUpgrade[3];

    [Header("Cooldowns")]
    public float abilityOneCooldown;
    public float abilityTwoCooldown;
    public float abilityThreeCooldown;

    public bool canUseUpgrade;

    playerStats PlayerStats;

    public override void OnNetworkSpawn()
    {
        PlayerStats = GetComponent<playerStats>();
    }

    private void Awake()
    {
        for (int i = 0; i < equipped.Length; i++)
            equipped[i] = new EquippedUpgrade();
    }

    private void Start()
    {
        abilityOneCooldown = abilityTwoCooldown = abilityThreeCooldown = 0f;
        canUseUpgrade = true;
    }

    // CLIENT ENTRY
    public void AddToPlayerUpgrades(int upgradeID)
    {
        if (!IsOwner) return;
        RequestSpawnUpgradeServerRpc(upgradeID);
    }

    // SERVER: SPAWN UPGRADE
    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnUpgradeServerRpc(int upgradeID, ServerRpcParams rpcParams = default)
    {
        ulong requester = rpcParams.Receive.SenderClientId;

        int slot = FindFirstAvailableSlotOrShift();

        UpgradeScriptableOBJ def = UpgradeDatabase.Instance.Get(upgradeID);
        if (def == null) return;

        GameObject prefab = def.logicScriptObject;
        Transform parent = transform; // or upgradePlaceHolders[slot] if you still use them

        GameObject instance = Instantiate(prefab, parent);
        NetworkObject netObj = instance.GetComponent<NetworkObject>();

        // Inject runtime identity BEFORE spawn
        var runtime = instance.GetComponent<UpgradeRuntime>();
        runtime.Initialize(slot, upgradeID);

        netObj.SpawnWithOwnership(requester);

        // Update SERVER state
        equipped[slot].upgradeID = upgradeID;
        equipped[slot].logicInstance = netObj;
        equipped[slot].cooldownRemaining = 0f;

        RegisterUpgrade(slot, upgradeID, netObj);

        SyncSlots();
    }


    // SLOT MANAGEMENT
    private int FindFirstAvailableSlotOrShift()
    {
        for (int i = 0; i < equipped.Length; i++)
        {
            if (equipped[i].logicInstance == null)
            {
                SyncSlots();
                return i;
            }

        }

        // full → shift
        RemoveUpgradeServerServerRpc(0);

        for (int i = 0; i < equipped.Length - 1; i++)
        {
            equipped[i] = equipped[i + 1];
        }

        equipped[equipped.Length - 1] = new EquippedUpgrade();
        SyncSlots();
        return equipped.Length - 1;

    }


    // SERVER ONLY: REMOVE UPGRADE
    [ServerRpc(RequireOwnership = false)]
    private void RemoveUpgradeServerServerRpc(int slot)
    {
        if (slot < 0 || slot >= equipped.Length) return;

        if (equipped[slot].logicInstance != null)
        {
            SpawnUpgradePickup(slot);
            equipped[slot].logicInstance.Despawn();
        }

        equipped[slot] = new EquippedUpgrade();

        SyncSlots();
    }

    public void RegisterUpgrade(int slot, int upgradeID, NetworkObject netObj)
    {
        if (!IsOwner) return;

        var entry = equipped[slot];

        entry.upgradeID = upgradeID;
        entry.logicInstance = netObj;
        entry.cooldownRemaining = 0f;

        if (netObj.TryGetComponent<useAbility>(out var ua))
            entry.logicScript = ua;

        if (netObj.TryGetComponent<onUpgradePickedup>(out var pickup))
        {
            print("onUpgradePickedup trying to be used");
            pickup.onUpgradePickedup(transform);
        }
    }

    // PICKUP SPAWN
    private void SpawnUpgradePickup(int slot)
    {
        int id = equipped[slot].upgradeID;

        var def = UpgradeDatabase.Instance.Get(id);
        if (def == null || def.pickupObject == null) return;

        GameObject obj = Instantiate(def.pickupObject, transform.position, Quaternion.identity);
        obj.GetComponent<NetworkObject>().Spawn();

        var pickup = obj.GetComponent<upgradePickUp>();
        if (pickup != null)
        {
            pickup.canBePickedUp = false;
            pickup.dropped = true;
        }
    }

    // INPUT
    private void Update()
    {
        if (!IsOwner || !canUseUpgrade) return;

        abilityOneCooldown -= Time.deltaTime;
        abilityTwoCooldown -= Time.deltaTime;
        abilityThreeCooldown -= Time.deltaTime;

        HandleSlot(0, GameInput.instance.getAbilityOneInput(), ref abilityOneCooldown);
        HandleSlot(1, GameInput.instance.getAbilityTwoInput(), ref abilityTwoCooldown);
        HandleSlot(2, GameInput.instance.getAbilityThreeInput(), ref abilityThreeCooldown);
    }

    private void HandleSlot(int slot, bool pressed, ref float cooldown)
    {
        if (equipped[slot].logicInstance == null) return;

        // Always ensure we have the script
        if (equipped[slot].logicScript == null)
        {
            equipped[slot].logicScript =
                equipped[slot].logicInstance.GetComponent<useAbility>();
        }

        // still null? then abort safely
        if (equipped[slot].logicScript == null) return;

        if (equipped[slot].logicScript == null)
        {
            // resolve after spawn (safety fallback)
            var def = UpgradeDatabase.Instance.Get(equipped[slot].upgradeID);
            if (def != null)
            {
                equipped[slot].logicScript =
                    equipped[slot].logicInstance.GetComponent<useAbility>();
            }
        }

        if (equipped[slot].logicScript == null) return;

        if (pressed && cooldown <= 0f)
        {
            equipped[slot].logicScript.useAbility(transform, true);

            var def = UpgradeDatabase.Instance.Get(equipped[slot].upgradeID);
            cooldown = def.cooldown / PlayerStats.currentCooldownReduction.Value;
        }
        else
        {
            equipped[slot].logicScript.useAbility(transform, false);
        }
    }


    private void SyncSlots()
    {
        int[] ids = new int[equipped.Length];
        ulong[] netIds = new ulong[equipped.Length];

        for (int i = 0; i < equipped.Length; i++)
        {
            if (equipped[i].logicInstance == null)
            {
                ids[i] = -1;
                netIds[i] = 0;
            }
            else
            {
                ids[i] = equipped[i].upgradeID;
                netIds[i] = equipped[i].logicInstance.NetworkObjectId;
            }
        }

        var parms = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { OwnerClientId }
            }
        };

        SyncSlotsClientRpc(ids, netIds);
    }

    [ClientRpc]
    private void SyncSlotsClientRpc(int[] ids, ulong[] netIds)
    {
        if (!IsOwner) return;

        for (int i = 0; i < equipped.Length; i++)
        {
            if (ids[i] == -1)
            {
                equipped[i] = new EquippedUpgrade();
                continue;
            }

            var netObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[netIds[i]];

            bool isNew = equipped[i].logicInstance != netObj;

            equipped[i].upgradeID = ids[i];
            equipped[i].logicInstance = netObj;

            if (netObj.TryGetComponent<useAbility>(out var ua))
                equipped[i].logicScript = ua;

            // ONLY initialize if it's NEW
            if (isNew && netObj.TryGetComponent<onUpgradePickedup>(out var pickup))
            {
                pickup.onUpgradePickedup(transform);
            }
        }
    }



}