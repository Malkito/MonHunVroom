using UnityEngine;
using Unity.Netcode;
public class airStrikeLogic : NetworkBehaviour, useAbility
{
    [SerializeField] private BulletSO bulletSO;
    [SerializeField] private playerShooting PS;
    BulletSO defaultBullet;
    public void useAbility(Transform transform, bool abiliyUsed)
    {
        PS = transform.gameObject.GetComponent<playerShooting>();

        if (!abiliyUsed) return;
        defaultBullet = PS.altBulletSO;
        changeBulletServerRpc();
        PS.AltShootServerRPC();
        revertBulletServerRpc();
        print("Air strike used");      
    }

    [ServerRpc(RequireOwnership = false)]
    public void changeBulletServerRpc()
    {
        PS.changeAltBullet(bulletSO);
    }


    [ServerRpc(RequireOwnership = false)]
    public void revertBulletServerRpc()
    {
        PS.changeAltBullet(defaultBullet);

    }
}
