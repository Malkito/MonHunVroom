using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;



interface bullet {
    public void setDamageOrigin(GameObject DamageOrigin);

}

public class playerShooting : NetworkBehaviour
{
    /// <summary>
    /// 
    /// This script attached to the player
    /// and handles the bullet generation
    /// 
    /// reads the main attack and Alt attack inputs and generates the appropriate bullet at the appropriate barrel ends
    /// adds the speed in the forward direction
    /// 
    /// </summary>

    [Header("Main Attack")]
    public BulletSO mainBulletSO;
    public Transform[] mainBarrelEnds;
    private float MaintimeBetweenShots;
    private int currentBarrelNum;

    [Header("Alt Attack")]
    public BulletSO altBulletSO;
    public Transform altBarrelEnd;
    private float altTimeBetweenShots;


    [Header("test stuff")]
    [SerializeField] TMP_Text currentMainBuleltTest;
    [SerializeField] TMP_Text currentAltBuleltTest;

    [Header("Other")]
    public bool canShoot;
    public float damageDealt;

    private void Start()
    {
        canShoot = true;
        MaintimeBetweenShots = mainBulletSO.minTimeBetweenShots;
        altTimeBetweenShots = altBulletSO.minTimeBetweenShots;

    }
    void Update()
    {
        if (!IsOwner) return;

        if (!canShoot) return;

        if (GameInput.instance.getAttackInput() && MaintimeBetweenShots > mainBulletSO.minTimeBetweenShots)
        {
            shootServerRPC();
            MaintimeBetweenShots = 0;
        }
        if (GameInput.instance.getAltAttackInput() && altTimeBetweenShots > altBulletSO.minTimeBetweenShots)
        {
            AltShootServerRPC();
            altTimeBetweenShots = 0;
        }
        altTimeBetweenShots += Time.deltaTime;
        MaintimeBetweenShots += Time.deltaTime;
        currentMainBuleltTest.text = mainBulletSO.name;
        currentAltBuleltTest.text = altBulletSO.name;
    }


    [ServerRpc(RequireOwnership = true)]
    private void shootServerRPC()
    {
        shoot();
    }

    [ServerRpc(RequireOwnership = true)]
    private void AltShootServerRPC()
    {
        altShoot();
    }

    private void shoot()
    {
        //Play sound
        //play muzzle flash

        GameObject projectile = Instantiate(mainBulletSO.bulletPrefab, mainBarrelEnds[currentBarrelNum].transform.position, transform.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.linearVelocity = mainBarrelEnds[currentBarrelNum].transform.forward * mainBulletSO.bulletSpeed;

        NetworkObject networkProjectile = projectile.GetComponent<NetworkObject>();
        networkProjectile.Spawn(true);

        if (projectile.gameObject.TryGetComponent(out bullet bullet))
        {
            bullet.setDamageOrigin(gameObject);
        }

        Destroy(projectile, mainBulletSO.bulletLifetime);
        if (currentBarrelNum == 0) { currentBarrelNum = 1; }
        else { currentBarrelNum = 0; }
    }


    public void altShoot()
    {

        //Play sound
        //play muzzle flash

        GameObject projectile = Instantiate(altBulletSO.bulletPrefab, altBarrelEnd.transform.position, transform.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        NetworkObject networkProjectile = projectile.GetComponent<NetworkObject>();
        networkProjectile.Spawn(true);

        if (projectile.gameObject.TryGetComponent(out bullet bullet))
        {
            bullet.setDamageOrigin(gameObject);
        }
        rb.linearVelocity = mainBarrelEnds[0].forward * altBulletSO.bulletSpeed;
        Destroy(projectile, altBulletSO.bulletLifetime);

    }


    public void changeMainBullet(BulletSO newBullet)
    {
        mainBulletSO = newBullet;
        MaintimeBetweenShots = mainBulletSO.minTimeBetweenShots;
    }
    public void changeAltBullet(BulletSO newBullet)
    {
        altBulletSO = newBullet;
        altTimeBetweenShots = altBulletSO.minTimeBetweenShots;

    }
}
