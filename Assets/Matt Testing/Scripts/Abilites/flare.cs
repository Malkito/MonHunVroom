using UnityEngine;
using Unity.Netcode;
using Ami.BroAudio;


public class flare : NetworkBehaviour
{

    [SerializeField] private GameObject airStrikeMissle;
    [SerializeField] private float timeToSpawn;
    private float elapsedTime;
    [SerializeField] private float spawnHeight;
    private bool spawned;

    [SerializeField] private SoundID flareSFX;
    void Start()
    {
        spawned = false;
        elapsedTime = 0;
        BroAudio.Play(flareSFX);
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
        GameObject bomb = Instantiate(airStrikeMissle, spawnPos, Quaternion.identity);
        NetworkObject bombNetOBJ = bomb.GetComponent<NetworkObject>();
        bombNetOBJ.Spawn();
        spawnedClientRPC();
        print("Is spawned: " + spawned);

    }

    [ClientRpc]
    public void spawnedClientRPC()
    {
        spawned = true;
    }

}
