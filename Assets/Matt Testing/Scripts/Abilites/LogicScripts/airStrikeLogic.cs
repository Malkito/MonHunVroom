using UnityEngine;
using Unity.Netcode;
public class airStrikeLogic : NetworkBehaviour, useAbility, onAbilityPickedup, onAbilityDropped
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

    public void AbilityPickedup(Transform player)
    {

    }

    public void AbilityPickupDropped(Transform player)
    {

    }







}
