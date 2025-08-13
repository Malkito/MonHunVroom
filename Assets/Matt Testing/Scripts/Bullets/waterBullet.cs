using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class waterBullet : NetworkBehaviour, bullet
{
    //Goes onto water Bullet prefab, checks if it collides with anything, then instantiates the particle system
    //Particle system has putOutFire script to check collsions with fire

    [SerializeField] private float waterDuration;
    [SerializeField] private GameObject waterSplash;
    [SerializeField] private float sphereSize;
    [SerializeField] BulletSO bulletData;
    private GameObject BulletDamageOrigin;


    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionServerRpc();
        if (collision.gameObject.TryGetComponent(out dealDamage healthScript))
        {
            healthScript.dealDamage(bulletData.bulletDamage, Color.green, BulletDamageOrigin); // deals damage if collides with something that can be damaged
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnCollisionServerRpc()
    {
        GameObject water = Instantiate(waterSplash, transform.position, Quaternion.identity);
        NetworkObject splashNetworkOBJ = water.GetComponent<NetworkObject>();
        splashNetworkOBJ.Spawn();

        Destroy(water, waterDuration);
        Destroy(gameObject);
        foreach (GameObject fireOBj in findFireInArea())
        {
            Destroy(fireOBj);
        }
    }

    public void setDamageOrigin(GameObject damageOrigin)
    {
        BulletDamageOrigin = damageOrigin;
    }


    private GameObject[] findFireInArea()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, sphereSize);
        List<GameObject> fires = new List<GameObject>();

        foreach(Collider col in hitColliders)
        {
            if (col.gameObject.CompareTag("Fire"))
            {
                fires.Add(col.gameObject);
            }
        }
        return fires.ToArray();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sphereSize);
    }
}
