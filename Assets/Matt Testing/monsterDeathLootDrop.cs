using UnityEngine;
using Unity.Netcode;

public class monsterDeathLootDrop : NetworkBehaviour
{
    [SerializeField] private GameObject[] objectToBeSpawned;

    [SerializeField] private int spawnRadiusSphere = 5;

    [SerializeField] private float minLaunchSpeed;
    [SerializeField] private float maxLaunchSpeed;

    [SerializeField] private int ObjectsSpawnMultiplicationPerPlayer = 1;

    [SerializeField] private Transform MonsterDeathLocation;

    private bool objectsSpawned;

    private void Update()
    {
        if (MonsterDeathLocation != null)
        {
            transform.position = MonsterDeathLocation.position;
        }
    }

    public void spawnobjects()
    {
        // ONLY SERVER SPAWNS OBJECTS
        if (!IsServer) return;

        if (objectsSpawned) return;

        objectsSpawned = true;

        int timesToSpawnObj =NetworkManager.Singleton.ConnectedClients.Count *ObjectsSpawnMultiplicationPerPlayer;

        for (int i = 0; i < timesToSpawnObj; i++)
        {
            SpawnLoot();
        }

        Destroy(gameObject, 1f);
    }

    private void SpawnLoot()
    {
        Vector3 spawnPosition = transform.position + Random.onUnitSphere * spawnRadiusSphere;

        int objectArrayPos =Random.Range(0, objectToBeSpawned.Length);

        GameObject spawnedObject =Instantiate(objectToBeSpawned[objectArrayPos],spawnPosition,Quaternion.identity);

        NetworkObject netObj = spawnedObject.GetComponent<NetworkObject>();

        if (netObj != null)
        {
            netObj.Spawn();
        }

        Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 launchDirection =(spawnPosition - transform.position).normalized;

            float launchForce =Random.Range(minLaunchSpeed, maxLaunchSpeed);

            rb.AddForce( launchDirection * launchForce,ForceMode.Impulse);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadiusSphere);
    }
}