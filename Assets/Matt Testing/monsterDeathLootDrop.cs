using UnityEngine;
using Unity.Netcode;

public class monsterDeathLootDrop : NetworkBehaviour
{
    [SerializeField] private GameObject objectToBeSpawned;
    private int timesToSpawnObj;
    [SerializeField] private int spawnRadiusSphere;
    [SerializeField] float minLaunchSpeed; // the min and max launch speeds the fire bullets are sent out 
    [SerializeField] float maxLaunchSpeed;
    private bool objectsSpawned;




   // [ClientRpc]


    public void spawnobjects()
    {
        if (objectsSpawned) return;
        timesToSpawnObj = NetworkManager.Singleton.ConnectedClients.Count;
        for(int i = 0; i < timesToSpawnObj; i++)
        {
            launchObjectsServerRpc();
        }

        objectsSpawned = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void launchObjectsServerRpc()
    {
        Vector3 spawnPosition = transform.position + Random.onUnitSphere * spawnRadiusSphere;
        GameObject spawnedProjectile = Instantiate(objectToBeSpawned, spawnPosition, Quaternion.identity);

        NetworkObject netobj = spawnedProjectile.GetComponent<NetworkObject>();
        netobj.Spawn();

        Rigidbody rb = spawnedProjectile.GetComponent<Rigidbody>();

        if(rb != null)
        {
            Vector3 launchDirection = (spawnPosition - transform.position).normalized;
            float launchforce = Random.Range(minLaunchSpeed, maxLaunchSpeed);
            rb.AddForce(launchDirection * launchforce, ForceMode.Impulse);
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadiusSphere);
    }



}
