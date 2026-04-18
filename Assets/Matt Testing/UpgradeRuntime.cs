using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class UpgradeRuntime : NetworkBehaviour
{
    private NetworkVariable<int> upgradeID = new NetworkVariable<int>();
    private NetworkVariable<int> slot = new NetworkVariable<int>();

    public void Initialize(int slotIndex, int id)
    {
        if (!IsServer) return;

        slot.Value = slotIndex;
        upgradeID.Value = id;
    }

    public override void OnNetworkSpawn()
    {

    }
}