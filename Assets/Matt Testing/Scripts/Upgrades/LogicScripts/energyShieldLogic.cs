using UnityEngine;
using Unity.Netcode;

public class energyShieldLogic : NetworkBehaviour, useAbility
{

    [SerializeField] private GameObject shieldObject;
    private Transform spawnLocal;

    public void useAbility(Transform transform , bool abilityused)
    {
        if (!abilityused) return;
        spawnLocal = transform;
        spawnSheildServerRPC();
    }


    [ServerRpc(RequireOwnership = false)]
    private void spawnSheildServerRPC()
    {
        print("RPC runs");
        GameObject Shield = Instantiate(shieldObject, spawnLocal.position, Quaternion.identity);
        NetworkObject sheildNetworkOBj = Shield.GetComponent<NetworkObject>();
        sheildNetworkOBj.Spawn();

        Shield.transform.SetParent(spawnLocal);
        Destroy(Shield, 10);
    }

}
