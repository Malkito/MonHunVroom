using UnityEngine;
using Unity.Netcode;
public class airStrikeLogic : NetworkBehaviour, useAbility, onUpgradePickedup, onUpgradeDropped
{
    [SerializeField] private playerShooting PS;
    [SerializeField] private int bulletSOIndex;
    public void useAbility(Transform transform, bool abiliyUsed)
    {
        PS = transform.gameObject.GetComponent<playerShooting>();
        if (!abiliyUsed) return;
        PS.AltShootServerRPC(bulletSOIndex);
        print("Air strike used");      
    }

    public void onUpgradePickedup(Transform player)
    {

    }

    public void onUpgradeDropped(Transform player)
    {

    }







}
