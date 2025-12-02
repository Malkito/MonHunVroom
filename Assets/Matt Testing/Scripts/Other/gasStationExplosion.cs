using UnityEngine;
using Unity.Netcode;

public class gasStationExplosion : NetworkBehaviour
{
    BuildingHealth BH;


    [SerializeField] GameObject fireBullet; // the fire bullet prefab


    [SerializeField] float minLaunchSpeed; // the min and max launch speeds the fire bullets are sent out 
    [SerializeField] float maxLaunchSpeed;


    [SerializeField] float bulletSpawnRadius; // the radius the fire bullets spawn onto

    [SerializeField] int numberOfBullets;

    [SerializeField] GameObject explosionParticles;

    private void Awake()
    {
        BH = GetComponent<BuildingHealth>();
    }

    private void Update()
    {
        if(BH.currentHealth <= (BH.maxHealth * 0.8))
        {
            spawnAndLaunchServerRpc();

            playExplosionParticlesServerRpc();
            DestroyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyServerRpc()
    {
        NetworkObject.Despawn();
    }


    [ServerRpc(RequireOwnership = false)]
    private void spawnAndLaunchServerRpc()
    {
        spawnAndLaunchFire();
    }
    private void spawnAndLaunchFire() // spawns fire bullets on sphere, picks a random direction then shoots the bullet with a randomized speed
    {
        for (int i = 0; i < numberOfBullets; i++) //runs the spawn fire bullet function the determinted number of times
        {
            Vector3 spawnPosition = transform.position + Random.onUnitSphere * bulletSpawnRadius; // picks a random point on the spawning sphere
            GameObject bullet = Instantiate(fireBullet, spawnPosition, Quaternion.identity); // spawns the bullet

            NetworkObject netOBJ = bullet.GetComponent<NetworkObject>();
            netOBJ.Spawn();

            Rigidbody rb = bullet.GetComponent<Rigidbody>(); // sets the rigid body of the bullet

            if (rb != null)
            {
                Vector3 launchDirection = (spawnPosition - transform.position).normalized; //finds the outward direction
                float launchforce = Random.Range(minLaunchSpeed, maxLaunchSpeed); // sets the launch force
                rb.AddForce(launchDirection * launchforce, ForceMode.Impulse); // launches the bullet in the set direction, with the set speed
            }
        }

    }

    [ServerRpc(RequireOwnership = false)]
    private void playExplosionParticlesServerRpc()
    {
        GameObject particles = Instantiate(explosionParticles);
        NetworkObject netobj = particles.GetComponent<NetworkObject>();
        netobj.Spawn();

        Destroy(particles, particles.GetComponent<ParticleSystem>().main.duration);
    }

}
