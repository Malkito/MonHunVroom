using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class energySphereBullet : NetworkBehaviour, bullet
{

    /// <summary>
    /// The energy Sphere bullet logic
    /// 
    /// Hnadles iof the bullet collides with an object
    /// if its damageable, then deal the damage
    /// If the energy sphere is hit by a fire bullet, it become a fire energy sphere, exploding on impact sending fire bullets everywhere
    /// 
    /// 
    /// </summary>

    private MeshRenderer MeshRenderer;
    [SerializeField] private BulletSO bulletSO; // the energy sphere bulelt data

    [Header("Fire Varibles")]
    [SerializeField] int numberOfBullets;//number of fire bullets sent when teh fire energy sphere explodes

    [SerializeField] private Material fireMat; // the material that is set when the energy sphere becomes a fire energy sphere

    private bool isEffectedByFire;

    [SerializeField] GameObject fireBullet; // the fire bullet prefab


    [SerializeField] float minLaucnhSpeed; // the min and max launch speeds the fire bullets are sent out 
    [SerializeField] float maxLaucnhSpeed;


    [SerializeField] float bulletSpawnRadius; // the radius the fire bullets spawn onto

    [Header("Growth Settings")]
    [SerializeField] private float delayBeforeGrowing;
    [SerializeField] private float growSpeed; // units per second
    private float maxScale;  // maximum uniform scale
    [SerializeField] private float scaleIncrease;  // maximum uniform scale


    private bool canGrow = false;



    public GameObject BulletDamageOrigin;

    private void Awake()
    {
        MeshRenderer = gameObject.GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        // Start the delayed growth
        Invoke(nameof(StartGrowing), delayBeforeGrowing);

        float percenatgeIncrease = scaleIncrease / 100;

        maxScale = transform.localScale.x + (transform.localScale.x * percenatgeIncrease);
        print(maxScale);

    }
    private void StartGrowing()
    {
        canGrow = true;
    }

    private void Update()
    {
        if (!canGrow) return;

        // Current scale magnitude (assuming uniform scale)
        float currentScale = transform.localScale.x;

        if (currentScale < maxScale)
        {
            float newScale = currentScale + growSpeed * Time.deltaTime;
            newScale = Mathf.Clamp(newScale, currentScale, maxScale);

            transform.localScale = new Vector3(newScale, newScale, newScale);
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (isEffectedByFire && !collision.gameObject.CompareTag("Fire"))// if the energy sphere is effected by fire and collides with something other then a fire bullet 
        {
            for (int i = 0; i < numberOfBullets; i++) //runs the spawn fire bullet function the determinted number of times
            {
                spawnAndLaunchServerRpc();
            }

            Destroy(gameObject);

        }
        else if(collision.gameObject.TryGetComponent(out dealDamage healthScript)) // if the collidied object can be damaged
        {
            healthScript.dealDamage(bulletSO.bulletDamage, Color.grey, BulletDamageOrigin); // deals damage to the collidied

            DestroyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]

    public void DestroyServerRpc()
    {
        Destroy(gameObject);
    }



    public void setFire() // sets the energy sphere on fire, called by the fire bullet
    {
        isEffectedByFire = true;
        MeshRenderer.material = fireMat;
    }


    public void setDamageOrigin(GameObject damageOrigin)
    {
        BulletDamageOrigin = damageOrigin;
    }


    [ServerRpc(RequireOwnership = false)]
    private void spawnAndLaunchServerRpc()
    {
        spawnAndLaunchFire();
    }

    private void spawnAndLaunchFire() // spawns a singlar fire bullet on sphere, picks a random direction then shoots the bullet with a randomized speed
    {
        Vector3 spawnPosition = transform.position + Random.onUnitSphere * bulletSpawnRadius; // picks a random point on the spawning sphere
        GameObject bullet = Instantiate(fireBullet, spawnPosition, Quaternion.identity); // spawns the bullet
        Rigidbody rb = bullet.GetComponent<Rigidbody>(); // sets the rigid body of the bullet

        if(rb != null)
        {
            Vector3 launchDirection = (spawnPosition - transform.position).normalized; //finds the outward direction
            float launchforce = Random.Range(minLaucnhSpeed, maxLaucnhSpeed); // sets the launch force
            rb.AddForce(launchDirection * launchforce, ForceMode.Impulse); // launches the bullet in the set direction, with the set speed
        }
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, bulletSpawnRadius);


    }

}
