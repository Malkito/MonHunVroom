using UnityEngine;
using System.Collections.Generic;
public class defaultAltBullet : MonoBehaviour, bullet
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
    [SerializeField] ParticleSystem explosion; // paritcle system

    private GameObject BulletDamageOrigin;

    private void OnCollisionEnter(Collision collision)
    {
        DealDamageToArea();

        GameObject explosionParticle = Instantiate(explosion.gameObject, transform.position, Quaternion.identity);  // starts the particles
        Destroy(explosionParticle, explosion.main.duration); // gets rid of particles after the duration

        Destroy(gameObject);
    }

    public void setDamageOrigin(GameObject damageOrigin)
    {
        BulletDamageOrigin = damageOrigin;
    }

    private void DealDamageToArea() 
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius); // gets all the colliders in the area


        foreach (Collider col in hitColliders) 
        {
            if(col.gameObject.TryGetComponent(out dealDamage healthScript)) // for each collider, checks if the object can be damaged
            {
                healthScript.dealDamage(bulletData.bulletDamage, Color.grey, BulletDamageOrigin); // damages the objects
            }

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
