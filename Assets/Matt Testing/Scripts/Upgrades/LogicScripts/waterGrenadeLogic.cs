using UnityEngine;
using System.Collections;

public class waterGrenadeLogic : MonoBehaviour, useAbility
{
    [SerializeField] BulletSO waterGrenade;
    [SerializeField] float abilityDuration;

    private playerShooting PS;
    private Transform player;
    private BulletSO defaultAltBullet;
    public void useAbility(Transform transform, bool abilityUsed)
    {
        if (!abilityUsed) return;
        player = transform;
        PS = transform.gameObject.GetComponent<playerShooting>();
        defaultAltBullet = PS.altBulletSO;
        PS.changeAltBullet(waterGrenade);
        Invoke(nameof(resetBullet), abilityDuration);
    }



    private void resetBullet()
    {
        PS.changeAltBullet(defaultAltBullet);
    }

}
