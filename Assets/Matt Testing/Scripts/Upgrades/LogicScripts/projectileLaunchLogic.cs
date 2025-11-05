using UnityEngine;

public class projectileLaunchLogic : MonoBehaviour, useAbility, onUpgradePickedup, onUpgradeDropped
{
    [SerializeField] private playerShooting PS;
    [SerializeField] private int bulletSOIndex;
    public void useAbility(Transform transform, bool abiliyUsed)
    {
        PS = transform.gameObject.GetComponent<playerShooting>();
        if (!abiliyUsed) return;
        PS.AltShootServerRPC(bulletSOIndex);     
    }



    public void onUpgradePickedup(Transform player)
    {

    }

    public void onUpgradeDropped(Transform player)
    {

    }
}
