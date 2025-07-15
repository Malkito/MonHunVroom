using UnityEngine;

public class projectileLaunchLogic : MonoBehaviour, useAbility
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
}
