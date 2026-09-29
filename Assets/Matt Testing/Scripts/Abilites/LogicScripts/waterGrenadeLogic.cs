using UnityEngine;
using System.Collections;

public class waterGrenadeLogic : MonoBehaviour, useAbility
{
    [SerializeField] float abilityDuration;
    [SerializeField] int waterGrendadeBulletIndex;

    private playerShooting PS;
    private Transform player;
    private int currentBulletIndex;
    public void useAbility(Transform transform, bool abilityUsed)
    {
        if (!abilityUsed) return;
        player = transform;
        PS = transform.gameObject.GetComponent<playerShooting>();
        currentBulletIndex = PS.currentAltBulletSoIndex;
        PS.changeBulletServerRpc(false, waterGrendadeBulletIndex);
        Invoke(nameof(resetBullet), abilityDuration);
    }



    private void resetBullet()
    {
        PS.changeBulletServerRpc(false, currentBulletIndex);
    }

}
