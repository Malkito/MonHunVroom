using UnityEngine;
using Unity.Netcode;
using System;

public interface useAbility
{
    void useAbility(Transform position, bool abilityPressed);
}

public interface onAbilityDropped
{
    void AbilityPickupDropped(Transform player);
}

public interface onAbilityPickedup
{
    void AbilityPickedup(Transform player);
}

[Serializable]
public class EquippedAbility
{
    public int abilityID;                 // INT ID ONLY
    public NetworkObject logicInstance;
    public useAbility logicScript;
    public float cooldownRemaining;

    public bool IsValid => logicInstance != null;
}

public class playerAbilityManager : NetworkBehaviour
{
    [Header("Runtime Slots")]
    public EquippedAbility[] equippedAbilities = new EquippedAbility[3];

    [Header("Cooldowns")]
    public float abilityOneCooldown;
    public float abilityTwoCooldown;
    public float abilityThreeCooldown;

    public bool canUseAbilites;

    playerStats PlayerStats;

    public override void OnNetworkSpawn()
    {
        PlayerStats = GetComponent<playerStats>();
    }

    private void Awake()
    {
        for (int i = 0; i < equippedAbilities.Length; i++)
            equippedAbilities[i] = new EquippedAbility();
    }

    private void Start()
    {
        abilityOneCooldown = abilityTwoCooldown = abilityThreeCooldown = 0f;
        canUseAbilites = true;
    }

    // CLIENT ENTRY
    public void AddToPlayerAbilites(int abilityID)
    {
        if (!IsOwner) return;
        RequestSpawnAbilityServerRpc(abilityID);
    }

    // SERVER: SPAWN UPGRADE
    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnAbilityServerRpc(int abilityID, ServerRpcParams rpcParams = default)
    {
        ulong requester = rpcParams.Receive.SenderClientId;

        int slot = FindFirstAvailableSlotOrShift();

        AbilityScriptableOBJ def = AbilityDatabase.Instance.Get(abilityID);
        if (def == null) return;

        GameObject prefab = def.logicScriptObject;
        Transform parent = transform;

        GameObject instance = Instantiate(prefab, parent);
        NetworkObject netObj = instance.GetComponent<NetworkObject>();

        // Inject runtime identity BEFORE spawn
        var runtime = instance.GetComponent<UpgradeRuntime>();
        runtime.Initialize(slot, abilityID);

        netObj.SpawnWithOwnership(requester);

        // Update SERVER state
        equippedAbilities[slot].abilityID = abilityID;
        equippedAbilities[slot].logicInstance = netObj;
        equippedAbilities[slot].cooldownRemaining = 0f;

        RegisterUpgrade(slot, abilityID, netObj);

        SyncSlots();
    }


    // SLOT MANAGEMENT
    private int FindFirstAvailableSlotOrShift()
    {
        for (int i = 0; i < equippedAbilities.Length; i++)
        {
            if (equippedAbilities[i].logicInstance == null)
            {
                SyncSlots();
                return i;
            }

        }

        // full → shift
        RemoveUpgradeServerServerRpc(0);

        for (int i = 0; i < equippedAbilities.Length - 1; i++)
        {
            equippedAbilities[i] = equippedAbilities[i + 1];
        }

        equippedAbilities[equippedAbilities.Length - 1] = new EquippedAbility();
        SyncSlots();
        return equippedAbilities.Length - 1;

    }


    // SERVER ONLY: REMOVE UPGRADE
    [ServerRpc(RequireOwnership = false)]
    private void RemoveUpgradeServerServerRpc(int slot)
    {
        if (slot < 0 || slot >= equippedAbilities.Length) return;

        if (equippedAbilities[slot].logicInstance != null)
        {
            SpawnUpgradePickup(slot);
            equippedAbilities[slot].logicInstance.Despawn();
        }

        equippedAbilities[slot] = new EquippedAbility();

        SyncSlots();
    }

    public void RegisterUpgrade(int slot, int upgradeID, NetworkObject netObj)
    {
        if (!IsOwner) return;

        var entry = equippedAbilities[slot];

        entry.abilityID = upgradeID;
        entry.logicInstance = netObj;
        entry.cooldownRemaining = 0f;

        if (netObj.TryGetComponent<useAbility>(out var ua))
            entry.logicScript = ua;

        if (netObj.TryGetComponent<onAbilityPickedup>(out var pickup))
        {
            print("onUpgradePickedup trying to be used");
            pickup.AbilityPickedup(transform);
        }
    }

    // PICKUP SPAWN
    private void SpawnUpgradePickup(int slot)
    {
        int id = equippedAbilities[slot].abilityID;

        var def = AbilityDatabase.Instance.Get(id);
        if (def == null || def.pickupObject == null) return;

        GameObject obj = Instantiate(def.pickupObject, transform.position, Quaternion.identity);
        obj.GetComponent<NetworkObject>().Spawn();

        var pickup = obj.GetComponent<AbilityPickUp>();
        if (pickup != null)
        {
            pickup.canBePickedUp = false;
            pickup.dropped = true;
        }
    }

    // INPUT
    private void Update()
    {
        if (!IsOwner || !canUseAbilites) return;

        abilityOneCooldown -= Time.deltaTime;
        abilityTwoCooldown -= Time.deltaTime;
        abilityThreeCooldown -= Time.deltaTime;

        HandleSlot(0, GameInput.instance.getAbilityOneInput(), ref abilityOneCooldown);
        HandleSlot(1, GameInput.instance.getAbilityTwoInput(), ref abilityTwoCooldown);
        HandleSlot(2, GameInput.instance.getAbilityThreeInput(), ref abilityThreeCooldown);
    }

    private void HandleSlot(int slot, bool pressed, ref float cooldown)
    {
        if (equippedAbilities[slot].logicInstance == null) return;

        // Always ensure we have the script
        if (equippedAbilities[slot].logicScript == null)
        {
            equippedAbilities[slot].logicScript =
                equippedAbilities[slot].logicInstance.GetComponent<useAbility>();
        }

        // still null? then abort safely
        if (equippedAbilities[slot].logicScript == null) return;

        if (equippedAbilities[slot].logicScript == null)
        {
            // resolve after spawn (safety fallback)
            var def = AbilityDatabase.Instance.Get(equippedAbilities[slot].abilityID);
            if (def != null)
            {
                equippedAbilities[slot].logicScript =
                    equippedAbilities[slot].logicInstance.GetComponent<useAbility>();
            }
        }

        if (equippedAbilities[slot].logicScript == null) return;

        if (pressed && cooldown <= 0f)
        {
            equippedAbilities[slot].logicScript.useAbility(transform, true);

            var def = AbilityDatabase.Instance.Get(equippedAbilities[slot].abilityID);
            cooldown = def.cooldown / PlayerStats.currentCooldownReduction.Value;
        }
        else
        {
            equippedAbilities[slot].logicScript.useAbility(transform, false);
        }
    }


    private void SyncSlots()
    {
        int[] ids = new int[equippedAbilities.Length];
        ulong[] netIds = new ulong[equippedAbilities.Length];

        for (int i = 0; i < equippedAbilities.Length; i++)
        {
            if (equippedAbilities[i].logicInstance == null)
            {
                ids[i] = -1;
                netIds[i] = 0;
            }
            else
            {
                ids[i] = equippedAbilities[i].abilityID;
                netIds[i] = equippedAbilities[i].logicInstance.NetworkObjectId;
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

        for (int i = 0; i < equippedAbilities.Length; i++)
        {
            if (ids[i] == -1)
            {
                equippedAbilities[i] = new EquippedAbility();
                continue;
            }

            var netObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[netIds[i]];

            bool isNew = equippedAbilities[i].logicInstance != netObj;

            equippedAbilities[i].abilityID = ids[i];
            equippedAbilities[i].logicInstance = netObj;

            if (netObj.TryGetComponent<useAbility>(out var ua))
                equippedAbilities[i].logicScript = ua;

            // ONLY initialize if it's NEW
            if (isNew && netObj.TryGetComponent<onAbilityPickedup>(out var pickup))
            {
                pickup.AbilityPickedup(transform);
            }
        }
    }


    public void dropAllUpgrades()
    {
        for(int i = 0; i < 3; i++)
        {
            if (equippedAbilities[i] != null)
            {
                RemoveUpgradeServerServerRpc(i);
            }
        }

    }
}