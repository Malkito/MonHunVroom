using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
public class defaultAltBullet : NetworkBehaviour, bullet
{

    /// <summary>
    /// 
    /// The deafauly alt fire (Right click)
    /// 
    /// When the bulelt collides with something, gets all the damageble objects in the radius and deals damage to it
    /// 
    /// </summary>

    [SerializeField] private BulletSO bulletData; // Alt bulelt data, set in inspector
    [SerializeField] private float damageRadius; // the radius of the damage, set in inspector
    [SerializeField] GameObject explosion; // paritcle system

    private GameObject BulletDamageOrigin;

    [SerializeField] private float explosonForce;


    private void OnCollisionEnter(Collision collision)
    {
        DealDamageToArea();

        spawnExplosionParticlesServerRpc();
        destroyBulletServerRpc();
    }

    public void setDamageOrigin(GameObject damageOrigin)
    {
        BulletDamageOrigin = damageOrigin;
    }

    [ServerRpc(RequireOwnership = false)]
    private void spawnExplosionParticlesServerRpc()
    {

        GameObject explosionParticle = Instantiate(explosion, transform.position, Quaternion.identity);  // starts the particles
        NetworkObject NetOBJ = explosionParticle.GetComponent<NetworkObject>();
        NetOBJ.Spawn();


        Destroy(explosionParticle, 1); // gets rid of particles after the duration
    }


    private void DealDamageToArea()
    {
        print("Colliders hit");
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius); // gets all the colliders in the area
        foreach (Collider col in hitColliders)
        {


            if (col.gameObject.TryGetComponent(out dealDamage healthScript) && col.gameObject.TryGetComponent<BuildingHealth>(out BuildingHealth buildingHealth)) // for each collider, checks if the object can be damaged
            {

                healthScript.dealDamage(bulletData.bulletDamage, Color.grey, BulletDamageOrigin); // damages the objects

            }else if (col.gameObject.TryGetComponent(out dealDamage script))
            {
                script.dealDamage(bulletData.bulletDamage, Color.grey, BulletDamageOrigin); // damages the objects
            }
            Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 launchDirection = (rb.transform.position - transform.position).normalized;
                float distance = Vector3.Distance(rb.transform.position, transform.position);
                //print("Name: " + rb.name + " Distance: " + Vector3.Distance(rb.transform.position, transform.position) + " Foce Applied: " + (launchDirection * (explosonFore - distance)));
                rb.AddForce(launchDirection * (explosonForce - distance), ForceMode.Impulse);
            }



        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void destroyBulletServerRpc()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
