using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PowerUpManager : NetworkBehaviour
{
    private const int MAX_POWERUPS = 3;
    private readonly List<PowerUpBase> activePowerUps = new();

    private GameInput input;

    private void Start()
    {
        input = GameInput.instance;
    }

    public void AddPowerUp(PowerUpBase newPowerUp)
    {
        if (!IsServer)
        {
            AddPowerUpServerRpc(newPowerUp.NetworkObject);
            return;
        }

        AddPowerUpInternal(newPowerUp);
        AddPowerUpClientRpc(newPowerUp.NetworkObjectId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddPowerUpServerRpc(NetworkObjectReference powerUpRef)
    {
        if (powerUpRef.TryGet(out NetworkObject obj))
        {
            AddPowerUpInternal(obj.GetComponent<PowerUpBase>());
            AddPowerUpClientRpc(obj.NetworkObjectId);
        }
    }

    [ClientRpc]
    private void AddPowerUpClientRpc(ulong powerUpID)
    {
        if (IsServer) return;

        var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[powerUpID];
        AddPowerUpInternal(obj.GetComponent<PowerUpBase>());
    }

    private void AddPowerUpInternal(PowerUpBase powerUp)
    {
        // If full, drop oldest
        if (activePowerUps.Count == MAX_POWERUPS)
        {
            PowerUpBase removed = activePowerUps[0];
            removed.OnRemoved();
            activePowerUps.RemoveAt(0);
        }

        activePowerUps.Add(powerUp);
        powerUp.Initialize(this);
    }
    private void Update()
    {
        if (!IsOwner) return;
        if (input == null) return;

        // Slot 0 → Ability 1
        if (activePowerUps.Count > 0)
        {
            bool held = input.getAbilityOneInput();
            bool pressed = input.getAbilityOneInput();
            activePowerUps[0].HandleInput(held, pressed);
        }

        // Slot 1 → Ability 2
        if (activePowerUps.Count > 1)
        {
            bool held = input.getAbilityTwoInput();
            bool pressed = input.getAbilityOneInput();
            activePowerUps[1].HandleInput(held, pressed);
        }

        // Slot 2 → Ability 3
        if (activePowerUps.Count > 2)
        {
            bool held = input.getAbilityThreeInput();
            bool pressed = input.getAbilityOneInput();
            activePowerUps[2].HandleInput(held, pressed);
        }
    }
}
