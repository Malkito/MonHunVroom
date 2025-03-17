using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class playerShooting : MonoBehaviour
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


    [SerializeField] private GameInput gameInput;


    [Header("Main Attack")]
    public BulletSO mainBulletSO;
    [SerializeField] private Transform[] mainBarrelEnds;
    private float MaintimeBetweenShots;
    private int currentBarrelNum;

    [Header("Alt Attack")]
    public BulletSO altBulletSO;
    [SerializeField] private Transform altBarrelEnd;
    private float altTimeBetweenShots;


    [Header("test stuff")]
    [SerializeField] private BulletSO fireBullet;
    [SerializeField] private BulletSO waterBullet;
    [SerializeField] private BulletSO electricBullet;
    private bool fireActive = false;
    [SerializeField] ParticleSystem flameThrower;
    [SerializeField] TMP_Text currentMainBuleltTest;
    [SerializeField] TMP_Text currentAltBuleltTest;


    void Update()
    {

        if (gameInput.getAttackInput() && MaintimeBetweenShots > mainBulletSO.minTimeBetweenShots)
        {
            shoot();
            MaintimeBetweenShots = 0;
        }
        if (gameInput.getAltAttackInput() && altTimeBetweenShots > altBulletSO.minTimeBetweenShots)
        {
            altShoot();
            altTimeBetweenShots = 0;
        }
        if (gameInput.getAbilityOneInput())
        {
            changeMainBullet(electricBullet);
        }
        if (gameInput.getAbilityTwoInput())
        {
            changeMainBullet(fireBullet);
        }

        if (gameInput.getAbilityThreeInput())
        {
            changeAltBullet(waterBullet);
        }
        altTimeBetweenShots += Time.deltaTime;
        MaintimeBetweenShots += Time.deltaTime;
        currentMainBuleltTest.text = mainBulletSO.name;
        currentAltBuleltTest.text = altBulletSO.name;
    }


    private void shoot()
    {
        //Play sound
        //play muzzle flash

        GameObject projectile = Instantiate(mainBulletSO.bulletPrefab, mainBarrelEnds[currentBarrelNum].transform.position, transform.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.linearVelocity = mainBarrelEnds[currentBarrelNum].transform.forward * mainBulletSO.bulletSpeed;
        Destroy(projectile, mainBulletSO.bulletLifetime);
        if (currentBarrelNum == 0) { currentBarrelNum = 1; }
        else { currentBarrelNum = 0; }
    }

    private void altShoot()
    {

        //Play sound
        //play muzzle flash

        GameObject projectile = Instantiate(altBulletSO.bulletPrefab, altBarrelEnd.transform.position, transform.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.linearVelocity = altBarrelEnd.transform.forward * altBulletSO.bulletSpeed;
        Destroy(projectile, altBulletSO.bulletLifetime);

    }


    public void changeMainBullet(BulletSO newBullet)
    {
        mainBulletSO = newBullet;
    }
    public void changeAltBullet(BulletSO newBullet)
    {
        altBulletSO = newBullet;
    }
}
