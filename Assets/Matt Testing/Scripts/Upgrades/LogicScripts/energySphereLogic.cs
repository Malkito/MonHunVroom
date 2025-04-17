using UnityEngine;

public class energySphereLogic : MonoBehaviour, useAbility
{
    [SerializeField] BulletSO energySphereSO;
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
        PS.changeAltBullet(energySphereSO);
        Invoke(nameof(resetBullet), abilityDuration);
    }



    private void resetBullet()
    {
        PS.changeAltBullet(defaultAltBullet);
    }
}
