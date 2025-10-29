using UnityEngine;
using Unity.Netcode;

public class upgradeRandomPosition : NetworkBehaviour
{

    [SerializeField] private Transform[] spawnpoints;
    [SerializeField] private GameObject[] upgrades;


    private void Start()
    {
        spawnUpgradesServerRpc();
    }
    private void shuffleUpgradeArray()
    {
        upgrades = GameObject.FindGameObjectsWithTag("Upgrade");

        for (int i = 0; i < upgrades.Length; i++)
        {
            int randomIndex = Random.Range(i, upgrades.Length);

            GameObject temp = upgrades[i];
            upgrades[i] = upgrades[randomIndex];
            upgrades[randomIndex] = temp;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void spawnUpgradesServerRpc()
    {
        shuffleUpgradeArray();

        for (int i = 0; i < upgrades.Length; i++)
        {
            upgrades[i].transform.position = spawnpoints[i].position;
        }
    }

}
