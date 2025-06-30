using UnityEngine;
using Unity.Netcode;

public class energyShieldLogic : NetworkBehaviour, useAbility
{

    [SerializeField] private GameObject shieldObject;
    private Transform spawnLocal;

    public void useAbility(Transform spawnLocation , bool abilityused)
    {
       if (!abilityused) return;
        spawnLocal = spawnLocation;
        spawnSheildServerRPC();
    }


    [ServerRpc(RequireOwnership = false)]
    private void spawnSheildServerRPC()
    {

        GameObject Shield = Instantiate(shieldObject, spawnLocal.position, Quaternion.identity);
        NetworkObject sheildNetworkOBj = Shield.GetComponent<NetworkObject>();
        sheildNetworkOBj.Spawn();

        Shield.transform.SetParent(spawnLocal);
        Destroy(Shield, 10);
    }

}
