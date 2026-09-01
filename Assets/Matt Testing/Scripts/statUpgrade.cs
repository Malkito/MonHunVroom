using UnityEngine;
using Unity.Netcode;

public class statUpgrade : NetworkBehaviour
{
    // GOES ON STAT PICKUP
    [SerializeField] private playerStatUpgradeManager.UpgradeType upgradeType;
    [SerializeField] private float upgradeAmount = 5f;

    private bool hasBeenCollected = false;


    private void OnCollisionEnter(Collision collision)
    {
        if (hasBeenCollected) return;

        playerStats player = collision.gameObject.GetComponent<playerStats>();

        if (player == null) return;

        ulong clientId = player.OwnerClientId;

        // Only owner of the player sends request
        if (player.IsOwner)
        {
            CollectPickupServerRpc(clientId);
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



    private void Update()
    {
        if(transform.position.y < -10)
        {
            transform.position = new Vector3(transform.position.x, 10, transform.position.z);
        }
    }
}
