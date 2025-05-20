using UnityEngine;
using Unity.Netcode;

public class flare : NetworkBehaviour
{

    [SerializeField] private GameObject airStrikeMissle;
    [SerializeField] private float timeToSpawn;
    private float elapsedTime;
    [SerializeField] private float spawnHeight;
    private bool spawned;
    void Start()
    {
        spawned = false;
        elapsedTime = 0;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        if(elapsedTime >= timeToSpawn && !spawned)
        {
            spawnAirSrtikeServerRPC();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void spawnAirSrtikeServerRPC()
    {
        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.x + spawnHeight, transform.position.z);
        Instantiate(airStrikeMissle, spawnPos, Quaternion.identity);
        spawned = true;
    }


}
