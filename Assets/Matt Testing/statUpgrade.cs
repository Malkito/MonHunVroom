using UnityEngine;
using Unity.Netcode;

public class statUpgrade : NetworkBehaviour
{
    // GOES ON STAT PICKUP
    [SerializeField] private playerStatUpgradeManager.UpgradeType upgradeType;
    [SerializeField] private float upgradeAmount = 5f;

    private bool hasBeenCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }


        if (hasBeenCollected) return;

        playerStats player = other.GetComponent<playerStats>();

        if (player == null) return;

        ulong clientId = player.OwnerClientId;

        // Only owner of the player sends request
        if (player.IsOwner)
        {
            CollectPickupServerRpc(player.OwnerClientId);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void CollectPickupServerRpc(ulong clientId)
    {
        if (hasBeenCollected) return;

        hasBeenCollected = true;

        playerStatUpgradeManager.Instance.AddUpgrade(clientId,upgradeType,upgradeAmount);

        // Find that player's current object and refresh stats
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId == clientId)
            {
                playerStats stats = client.PlayerObject.GetComponent<playerStats>();

                if (stats != null)
                {
                    stats.ApplyUpgrades();
                }

                break;
            }
        }

        GetComponent<NetworkObject>().Despawn();
    }
}
