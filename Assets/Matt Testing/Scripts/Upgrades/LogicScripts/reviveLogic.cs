using UnityEngine;
using Unity.Netcode;
public class reviveLogic : NetworkBehaviour, useAbility
{
    public void useAbility(Transform player, bool abilityPressed)
    {
        if (abilityPressed)
        {
            respawnManager.Instance.respawnPlayer(player);
        }
    }
}
