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

    [SerializeField] private ParticleSystem muzzleFlash;


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

        // checks if the main attack (Left click) input and if enough time between shots has elapsed
        if (GameInput.instance.getAttackInput() && MaintimeBetweenShots > bulletSOarray[currentMainBulletSoIndex].minTimeBetweenShots)
        {
            shootServerRPC(currentMainBulletSoIndex);
            MaintimeBetweenShots = 0;
        }

        // checks if the alt attack (Right click) input and if enough time between shots has elapsed
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

    private void shoot(int BulletIndex) // the main shooting function. bulelt index refrences the position in static bullet array. The reason for the int is so that server RPCs can take the bullet as an aguement
    {
        //Play sound
        muzzleFlashClientRpc();

        if (onlyOneBarrel)
        {
            currentBarrelNum = 0;
        }

        // spawn the game object projectile related to the bullet index num
        GameObject projectile = Instantiate(bulletSOarray[BulletIndex].bulletPrefab, mainBarrelEnds[currentBarrelNum].transform.position, transform.rotation); 

        // launches the projectile on teh correct direction
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.linearVelocity = mainBarrelEnds[currentBarrelNum].transform.forward * bulletSOarray[BulletIndex].bulletSpeed;

        //spawns the bullet across the network
        NetworkObject networkProjectile = projectile.GetComponent<NetworkObject>(); 
        networkProjectile.Spawn(true);

        //applies the recoil to the tank
        applyRecoilClientRpc(bulletSOarray[BulletIndex].recoilForce);


        //gets the bullet script to assign the damage origin once the bullet deals damage to something
        if (projectile.gameObject.TryGetComponent(out bullet bullet))
        {
            bullet.setDamageOrigin(gameObject);
        }

        //if the bullet is the default bullet and the player has collected a "big shot" multplies the damage on the bullet.
        if(BulletIndex == 0 && bigShotLoaded)
        {
            projectile.GetComponent<defaultBullet>().isBigShot = true;
            bigShotLoaded = false;
        }

        //Destroy the projectile 
        Destroy(projectile, bulletSOarray[BulletIndex].bulletLifetime);
    }

    //Applies a force to the tank, based on the recoild force on the specified bulelt index 
    [ClientRpc]
    private void applyRecoilClientRpc(float recoilForce)
    {
        if (!IsOwner) return;
        Vector3 backforce = -mainBarrelEnds[0].transform.forward * recoilForce;
        Vector3 upforce = Vector3.up * (recoilForce / 2);
        tankRB.AddForce(backforce + upforce, ForceMode.VelocityChange);
    }
    [ClientRpc]
    private void muzzleFlashClientRpc()
    {
        muzzleFlash.Play();
    }



    public void altShoot(int BulletIndex) // Similar to the shoot function
    {

        //Play sound
        //play muzzle flash

        muzzleFlashClientRpc();

        // spawn the game object projectile related to the bullet index num
        GameObject projectile = Instantiate(bulletSOarray[BulletIndex].bulletPrefab, altBarrelEnd.transform.position, transform.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        //spawns the bullet across the network
        NetworkObject networkProjectile = projectile.GetComponent<NetworkObject>();
        networkProjectile.Spawn(true);

        //applies the recoil to the tank
        applyRecoilClientRpc(bulletSOarray[BulletIndex].recoilForce);


        //gets the bullet script to assign the damage origin once the bullet deals damage to something
        if (projectile.gameObject.TryGetComponent(out bullet bullet))
        {
            bullet.setDamageOrigin(gameObject);
        }


        rb.linearVelocity = mainBarrelEnds[0].forward * bulletSOarray[BulletIndex].bulletSpeed;

        //Destroy the projectile after 
        Destroy(projectile, bulletSOarray[BulletIndex].bulletLifetime);

    }

    [ServerRpc(RequireOwnership = false)] //changes the bullet type, un-used for the moment
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
