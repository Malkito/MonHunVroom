using UnityEngine;

public class upgradeRandomPosition : MonoBehaviour
{

    [SerializeField] private Transform[] spawnpoints;
    [SerializeField] private GameObject[] upgrades;

    void Start()
    {
        shuffleUpgradeArray();

        for(int i = 0; i < upgrades.Length; i++)
        {
            upgrades[i].transform.position = spawnpoints[i].position;
        }
    }



    private void shuffleUpgradeArray()
    {
        for (int i = 0; i < upgrades.Length; i++)
        {
            int randomIndex = Random.Range(i, upgrades.Length);

            GameObject temp = upgrades[i];
            upgrades[i] = upgrades[randomIndex];
            upgrades[randomIndex] = temp;
        }
    }
}
