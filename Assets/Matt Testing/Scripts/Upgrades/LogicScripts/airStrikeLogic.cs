using UnityEngine;

public class airStrikeLogic : MonoBehaviour, useAbility
{
    [SerializeField] private BulletSO flareBulletSO;
    private BulletSO defaultAltBulletSO;
    playerShooting PS;
    public void useAbility(Transform transform, bool abiliyUsed)
    {
        if (!abiliyUsed) return;
        PS = transform.gameObject.GetComponent<playerShooting>();
        defaultAltBulletSO = PS.altBulletSO;
        PS.changeAltBullet(flareBulletSO);
        PS.altShoot();
        PS.changeAltBullet(defaultAltBulletSO);
    }




}
