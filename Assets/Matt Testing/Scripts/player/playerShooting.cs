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
    public int currentMainBulletSoIndex;
    public Transform[] mainBarrelEnds;
    private float MaintimeBetweenShots;
    private int currentBarrelNum;
    [SerializeField] bool onlyOneBarrel;
    [HideInInspector] public bool bigShotLoaded;

    [Header("Alt Attack")]
    public int currentAltBulletSoIndex;
    public Transform altBarrelEnd;
    private float altTimeBetweenShots;


    [Header("test stuff")]
    [SerializeField] TMP_Text currentMainBuleltTest;
    [SerializeField] TMP_Text currentAltBuleltTest;

    [Header("Other")]
    public bool canShoot;
    public float damageDealt;
    [SerializeField] public BulletSO[] bulletSOarray;
    private Rigidbody tankRB;


    private void Start()
    {
        tankRB = gameObject.GetComponent<Rigidbody>();
        canShoot = true;
        MaintimeBetweenShots = bulletSOarray[currentMainBulletSoIndex].minTimeBetweenShots;
        altTimeBetweenShots = bulletSOarray[currentAltBulletSoIndex].minTimeBetweenShots;

    }
    void Update()
    {
        if (!IsOwner) return;

        if (!canShoot) return;

        if (GameInput.instance.getAttackInput() && MaintimeBetweenShots > bulletSOarray[currentMainBulletSoIndex].minTimeBetweenShots)
        {
            shootServerRPC(currentMainBulletSoIndex);
            MaintimeBetweenShots = 0;
        }
        if (GameInput.instance.getAltAttackInput() && altTimeBetweenShots > bulletSOarray[currentAltBulletSoIndex].minTimeBetweenShots)
        {
            AltShootServerRPC(currentAltBulletSoIndex);
            altTimeBetweenShots = 0;
        }
        altTimeBetweenShots += Time.deltaTime;
        MaintimeBetweenShots += Time.deltaTime;
        currentMainBuleltTest.text = bulletSOarray[currentMainBulletSoIndex].name;
        currentAltBuleltTest.text = bulletSOarray[currentAltBulletSoIndex].name;
    }


    [ServerRpc(RequireOwnership = true)]
    public void shootServerRPC(int bulletIndex)
    {
        shoot(bulletIndex);
    }

    [ServerRpc(RequireOwnership = true)]
    public void AltShootServerRPC(int bulletIndex)
    {
        altShoot(bulletIndex);
    }

    private void shoot(int BulletIndex)
    {
        //Play sound
        //play muzzle flash

        if (onlyOneBarrel)
        {
            currentBarrelNum = 0;
        }

        GameObject projectile = Instantiate(bulletSOarray[BulletIndex].bulletPrefab, mainBarrelEnds[currentBarrelNum].transform.position, transform.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.linearVelocity = mainBarrelEnds[currentBarrelNum].transform.forward * bulletSOarray[BulletIndex].bulletSpeed;

        NetworkObject networkProjectile = projectile.GetComponent<NetworkObject>();
        networkProjectile.Spawn(true);

        applyRecoil(bulletSOarray[BulletIndex].recoilForce);

        if (projectile.gameObject.TryGetComponent(out bullet bullet))
        {
            bullet.setDamageOrigin(gameObject);
        }

        if(BulletIndex == 0 && bigShotLoaded)
        {
            projectile.GetComponent<defaultBullet>().isBigShot = true;
            bigShotLoaded = false;
        }

        Destroy(projectile, bulletSOarray[BulletIndex].bulletLifetime);
        if (currentBarrelNum == 0) { currentBarrelNum = 1; }
        else { currentBarrelNum = 0; }
    }

    private void applyRecoil(float recoilForce)
    {
        Vector3 backforce = -mainBarrelEnds[0].transform.forward * recoilForce;
        Vector3 upforce = Vector3.up * (recoilForce / 2);

        tankRB.AddForce(backforce + upforce, ForceMode.VelocityChange);
    }



    public void altShoot(int BulletIndex)
    {

        //Play sound
        //play muzzle flash

        GameObject projectile = Instantiate(bulletSOarray[BulletIndex].bulletPrefab, altBarrelEnd.transform.position, transform.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        NetworkObject networkProjectile = projectile.GetComponent<NetworkObject>();
        networkProjectile.Spawn(true);

        applyRecoil(bulletSOarray[BulletIndex].recoilForce);

        if (projectile.gameObject.TryGetComponent(out bullet bullet))
        {
            bullet.setDamageOrigin(gameObject);
        }
        rb.linearVelocity = mainBarrelEnds[0].forward * bulletSOarray[BulletIndex].bulletSpeed;
        Destroy(projectile, bulletSOarray[BulletIndex].bulletLifetime);

    }

    [ServerRpc(RequireOwnership = false)]
    public void changeBulletServerRpc(bool changeMainBullet, int NewBulletSOindex)
    {
        if(changeMainBullet)
        {
            currentMainBulletSoIndex = NewBulletSOindex;
        }
        else
        {
            currentAltBulletSoIndex = NewBulletSOindex;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 start = mainBarrelEnds[0].position;
        Vector3 end = start + mainBarrelEnds[0].transform.forward * 10;
        Gizmos.DrawLine(start, end);

    }
}
