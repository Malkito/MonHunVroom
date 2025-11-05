using UnityEngine;
using Unity.Netcode;

public class fireBullet : NetworkBehaviour, bullet
{
    /// <summary>
    /// 
    /// The fire bullet script sets things on fire, works with fire Manager
    /// 
    /// </summary>


    [SerializeField] GameObject fireEffect; //The object that is created when a fire bullet hits any object

    private ParticleSystem fireParticle; // the particle system 

    [SerializeField] BulletSO fireBulletSO; // the fire bullet date, set in inspector
    [SerializeField] float maxBurnTime;

    private GameObject DamageOrigin;

    Transform collisionTransform;

    public void setDamageOrigin(GameObject damageOrigin)
    {
        DamageOrigin = damageOrigin;

    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Fire")) return;

        if (collision.gameObject.CompareTag("energySphere"))
        {
            // if the collided object is the energy sphere then is sets teh energysphere on fire
            energySphereBullet EnergyBullet = collision.gameObject.GetComponent<energySphereBullet>();
            EnergyBullet.setFire();
            Destroy(gameObject);
            return;
        }

        collisionTransform = collision.transform;

        spawnfireServerRPC();

        Destroy(gameObject);
    }


    [ServerRpc(RequireOwnership = false)]
    private void spawnfireServerRPC()
    {
        GameObject fire = Instantiate(fireEffect, transform.position, Quaternion.Euler(-90, 0, 0)); // creates the fire object
        NetworkObject fireNetOBj = fire.GetComponent<NetworkObject>();
        fireNetOBj.Spawn();
        fire.GetComponent<fireManager>().objectFireIsAttachedTo = collisionTransform.gameObject;




        fireParticle = fire.transform.GetChild(0).GetComponent<ParticleSystem>();
        Destroy(fire, fireParticle.main.duration);// reads the duration of the particle system and drestoys the created fire object based off the duration


    }

}
