using UnityEngine;
using Unity.Netcode;


public class fireBulletLogic : NetworkBehaviour, useAbility
{

    [SerializeField] private float maxCharge;
    [SerializeField] private float chargeDeplationRate;
    [SerializeField] private float chargeRegenAmout;
    private float currentCharge;
    private bool canFire;
    [SerializeField] private int bulletSoIndex;
    [SerializeField] private playerShooting PS;

    [SerializeField] float timeBetweenShots;


    public void useAbility(Transform transform, bool abilityPressed)
    {
        PS = transform.gameObject.GetComponent<playerShooting>();

        if (currentCharge <= 0)
        {
            currentCharge = 0;
            canFire = false;
        }
       
        if(abilityPressed && currentCharge >= 0 && canFire)
        {
            if(timeBetweenShots > PS.bulletSOarray[bulletSoIndex].minTimeBetweenShots)
            {
                PS.AltShootServerRPC(bulletSoIndex);
                timeBetweenShots = 0;
            }
            else
            {
                timeBetweenShots += Time.deltaTime;
            }
            currentCharge -= Time.deltaTime * chargeDeplationRate;
            print(currentCharge);
        }
        else
        {
            isNotfiring();
            print(currentCharge);
        }
    }


    private void isNotfiring()
    {
        currentCharge += Time.deltaTime * chargeRegenAmout;
        if (currentCharge >= (maxCharge * 0.2)) canFire = true;
        if (currentCharge >= maxCharge) currentCharge = maxCharge;

    }


}
