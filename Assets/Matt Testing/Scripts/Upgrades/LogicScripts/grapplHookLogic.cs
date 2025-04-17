using UnityEngine;

public class grapplHookLogic : MonoBehaviour, useAbility
{

    public void useAbility(Transform transform, bool abilityUsed)
    {   
        playerShooting PS = transform.gameObject.GetComponent<playerShooting>();
        grapplHook hookScript = PS.mainBarrelEnds[0].GetComponentInChildren<grapplHook>();

        if (abilityUsed)
        {
            hookScript.deployGrapple(transform);
        }
    }
}
