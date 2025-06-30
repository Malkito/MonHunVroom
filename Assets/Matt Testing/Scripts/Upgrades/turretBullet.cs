using UnityEngine;
using Unity.Netcode;

public class turretBullet : NetworkBehaviour, bullet 
{

    [SerializeField] private LayerMask allowedLayers;
    private bool isAttached;
    private Rigidbody RB;


    [Header("Shooting varibles")]
    public BulletSO[] bulletData;
    private float timeBetweenShots;
    [SerializeField] private float fireRate;
    private Vector3 hemisphereUp;
    [SerializeField] private Transform barrelEnd;

    public GameObject turretOwner;

    private void Start()
    {
        RB = gameObject.GetComponent<Rigidbody>();
    }

    public void setDamageOrigin(GameObject damageOrigin)
    {
        turretOwner = damageOrigin;
    }


    private void Update()
    {
        if(timeBetweenShots > fireRate && isAttached)
        {
            shootServerRPC();
            timeBetweenShots = 0;
        }
        timeBetweenShots += Time.deltaTime;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (isAttached) return;

        ContactPoint contact = collision.contacts[0];
        transform.position = contact.point;
        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, contact.normal);
        transform.rotation = targetRotation;
        transform.SetParent(collision.transform);
        hemisphereUp = contact.normal;
        isAttached = true;

        RB.constraints = RigidbodyConstraints.FreezeAll;
    }


    private void shoot()
    {
        int randomNum = Random.Range(0, bulletData.Length);


        Vector3 randomDir = Random.onUnitSphere;
        if(Vector3.Dot(randomDir, hemisphereUp.normalized) < 0)
        {
            randomDir = -randomDir;
        }

        GameObject projectile = Instantiate(bulletData[randomNum].bulletPrefab, barrelEnd.position, Quaternion.LookRotation(randomDir));
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        NetworkObject networkProjectile = projectile.GetComponent<NetworkObject>();
        networkProjectile.Spawn(true);

        if (projectile.gameObject.TryGetComponent(out bullet bullet))
        {
            bullet.setDamageOrigin(turretOwner);
        }
        if (rb != null)
        {
            rb.AddForce(randomDir * bulletData[randomNum].bulletSpeed, ForceMode.VelocityChange);
            Destroy(projectile, bulletData[randomNum].bulletLifetime);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void shootServerRPC()
    {
        shoot();
    }
}
