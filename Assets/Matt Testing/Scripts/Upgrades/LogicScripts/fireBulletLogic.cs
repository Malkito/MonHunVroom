using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;


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
    private Slider fireSlider;
    private Transform fireMeterUI;

    public void useAbility(Transform transform, bool abilityPressed)
    {
        PS = transform.gameObject.GetComponent<playerShooting>();

        fireMeterUI = FindFireUI(transform, "FireMeter");
        fireMeterUI.gameObject.SetActive(true);
        fireSlider = fireMeterUI.GetChild(0).GetComponent<Slider>();




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



        fireSlider.value = currentCharge / maxCharge;
       
    }


    private void isNotfiring()
    {
        currentCharge += Time.deltaTime * chargeRegenAmout;
        if (currentCharge >= (maxCharge * 0.2)) canFire = true;
        if (currentCharge >= maxCharge) currentCharge = maxCharge;

    }

    private Transform FindFireUI(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindFireUI(child, name);
            if (found != null)
                return found;
        }
        return null;

    }


}
