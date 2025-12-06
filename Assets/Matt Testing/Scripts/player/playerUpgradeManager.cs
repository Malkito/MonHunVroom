using UnityEngine;
using Unity.Netcode;
using System;

// --- Interfaces required by logic scripts ---
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
    public UpgradeScriptableOBJ definition;
    public NetworkObject logicInstance;    // The spawned runtime instance (networked)
    public useAbility logicScript;         // Cached interface on the runtime instance
    public float cooldownRemaining;
}

public class playerUpgradeManager : NetworkBehaviour
{
    [Header("Definitions")]
    [SerializeField] private UpgradeScriptableOBJ[] upgradeArray; // all possible upgrades (ScriptableObjects)
    [SerializeField] private Transform[] upgradePlaceHolders;     // where runtime logic objects are parented

    [Header("Player Slots")]
    public EquippedUpgrade[] equipped = new EquippedUpgrade[3];

    [Header("Cooldowns")]
    public float abilityOneCooldown;
    public float abilityTwoCooldown;
    public float abilityThreeCooldown;

    public bool canUseUpgrade;

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

    // --- Public API called by pickup scripts on the OWNER ---
    // This method should be called on the client who picks up the upgrade.
    public void AddToPlayerUpgrades(int upgradeArrayIndex)
    {
        if (!IsOwner) return; // only the owning client requests adding an upgrade
        RequestSpawnUpgradeServerRpc(upgradeArrayIndex);
    }

    // --- Server: spawn the runtime logic object and assign ownership to the caller ---
    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnUpgradeServerRpc(int arrayIndex, ServerRpcParams rpcParams = default)
    {
        ulong requester = rpcParams.Receive.SenderClientId;

        // find first available slot or shift if full
        int slot = FindFirstAvailableSlotOrShift();

        // instantiate on server and spawn with ownership
        GameObject prefab = upgradeArray[arrayIndex].logicScriptObject;
        Transform parent = upgradePlaceHolders[Mathf.Clamp(slot, 0, upgradePlaceHolders.Length - 1)];
        GameObject instance = Instantiate(prefab, parent);
        NetworkObject netObj = instance.GetComponent<NetworkObject>();

        // spawn and hand ownership to the player who requested
        netObj.SpawnWithOwnership(requester);

        // If the slot already had an instance (in case of forced shift), despawn that instance on the server
        if (equipped[slot].logicInstance != null)
        {
            NetworkObject old = equipped[slot].logicInstance;

            // spawn pickup using the server-side data and position
            SpawnUpgradePickupServerRpc(slot);

            // despawn old instance
            old.Despawn();

            equipped[slot] = new EquippedUpgrade();
        }

        // Tell only the owner to assign this spawned instance locally
        var parms = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { requester } } };
        OwnerAssignSpawnedUpgradeClientRpc(netObj.NetworkObjectId, arrayIndex, slot, parms);
    }

    // Called only on the owner client to cache references and run onUpgradePickedup
    [ClientRpc]
    private void OwnerAssignSpawnedUpgradeClientRpc(ulong spawnedNetId, int arrayIndex, int slot, ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return; // safety

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(spawnedNetId, out NetworkObject netObj))
        {
            Debug.LogError("Spawned object not found on client: " + spawnedNetId);
            return;
        }

        // assign definition and runtime instance
        equipped[slot].definition = upgradeArray[arrayIndex];
        equipped[slot].logicInstance = netObj;
        equipped[slot].cooldownRemaining = 0f;

        // cache the interface
        if (netObj.TryGetComponent<useAbility>(out var ua))
            equipped[slot].logicScript = ua;
        else
            equipped[slot].logicScript = null;

        // call onUpgradePickedup if present
        if (netObj.TryGetComponent<onUpgradePickedup>(out var pickup))
        {
            pickup.onUpgradePickedup(transform);
        }
    }

    // Helper: spawn a world pickup for a dropped upgrade (server-only)
    // NOTE: This RPC now takes the slot index to avoid sending ScriptableObjects over the network.
    [ServerRpc(RequireOwnership = false)]
    private void SpawnUpgradePickupServerRpc(int slot)
    {
        
        if (slot < 0 || slot >= equipped.Length) return;

        var def = equipped[slot].definition;
        if (def == null || def.pickupObject == null) return;

        Vector3 spawnPos = Vector3.zero;
        if (equipped[slot].logicInstance != null)
            spawnPos = equipped[slot].logicInstance.transform.position;
        else
            spawnPos = transform.position;

        GameObject pickUpObject = Instantiate(def.pickupObject, spawnPos, Quaternion.identity);
        NetworkObject n = pickUpObject.GetComponent<NetworkObject>();
        n.Spawn();

        var pickupScript = pickUpObject.GetComponent<upgradePickUp>();
        if (pickupScript != null)
        {
            pickupScript.canBePickedUp = false;
            pickupScript.dropped = true;
        }
    }

    // --- Called by input loop on the owner (same pattern as before) ---
    private void Update()
    {


        if (!IsOwner) return;

        if (!canUseUpgrade) return;

        // countdowns
        abilityOneCooldown -= Time.deltaTime;
        abilityTwoCooldown -= Time.deltaTime;
        abilityThreeCooldown -= Time.deltaTime;

        // slot 0
        HandleSlotInput(0, GameInput.instance.getAbilityOneInput(), ref abilityOneCooldown);
        // slot 1
        HandleSlotInput(1, GameInput.instance.getAbilityTwoInput(), ref abilityTwoCooldown);
        // slot 2
        HandleSlotInput(2, GameInput.instance.getAbilityThreeInput(), ref abilityThreeCooldown);

    }

    private void HandleSlotInput(int slot, bool inputPressed, ref float abilityCooldown)
    {

        if (slot < 0 || slot >= equipped.Length) return;

        if (equipped[slot].definition == null) return; // no upgrade
        // ensure we have a runtime instance cached; attempt to refresh if missing
        if (equipped[slot].logicInstance == null)
        {
            // try to find any spawned child under placeholder (helps after reconnects)
            Transform parent = upgradePlaceHolders[Mathf.Clamp(slot, 0, upgradePlaceHolders.Length - 1)];
            if (parent.childCount > 0)
            {
                var child = parent.GetChild(0).GetComponent<NetworkObject>();
                if (child != null)
                {
                    equipped[slot].logicInstance = child;
                    child.TryGetComponent<useAbility>(out var ua);
                    equipped[slot].logicScript = ua;
                }
            }
        }

        // call useAbility on the runtime instance (if available) - pressed or not pressed
        if (equipped[slot].logicScript != null)
        {
            // If cooldown applies, only allow when abilityCooldown <= 0
            if (inputPressed && abilityCooldown <= 0f)
            {
                print("Ability being used");
                equipped[slot].logicScript.useAbility(transform, true);
                abilityCooldown = equipped[slot].definition.cooldown;
            }
            else
            {
                equipped[slot].logicScript.useAbility(transform, false);
            }
        }
    }

    // Helper: finds first empty slot, or shifts everything left and returns last slot (server chooses)
    private int FindFirstAvailableSlotOrShift()
    {
        for (int i = 0; i < equipped.Length; i++)
        {
            if (equipped[i].definition == null) return i;
        }

        // all slots full: shift left on server and return last slot index
        // drop oldest (slot 0) as pickup
        for (int i = 0; i < equipped.Length - 1; i++)
        {
            equipped[i] = equipped[i + 1];
        }
        equipped[equipped.Length - 1] = new EquippedUpgrade();
        return equipped.Length - 1;
    }

    // Public: remove an equipped upgrade from a specific slot (owner requests)
    public void RemoveUpgrade(int slot)
    {
        if (!IsOwner) return;
        RequestRemoveUpgradeServerRpc(slot);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestRemoveUpgradeServerRpc(int slot)
    {
        if (slot < 0 || slot >= equipped.Length) return;

        if (equipped[slot].logicInstance != null)
        {
            // spawn pickup
            SpawnUpgradePickupServerRpc(slot);

            // despawn instance
            equipped[slot].logicInstance.Despawn();
        }

        equipped[slot] = new EquippedUpgrade();

        // notify owner to clear UI / call onUpgradeDropped
        var parms = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { OwnerClientId } } };
        OwnerNotifyDroppedClientRpc(slot, parms);
    }

    [ClientRpc]
    private void OwnerNotifyDroppedClientRpc(int slot, ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;

        // clear local state and call onUpgradeDropped on the instance if we still have it
        if (equipped[slot].logicInstance != null && equipped[slot].logicInstance.TryGetComponent<onUpgradeDropped>(out var drop))
        {
            drop.onUpgradeDropped(transform);
        }

        equipped[slot] = new EquippedUpgrade();
    }
}
