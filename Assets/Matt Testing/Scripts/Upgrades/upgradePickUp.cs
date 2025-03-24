using UnityEngine;

public class upgradePickUp : MonoBehaviour
{

    public UpgradeScriptableOBJ objToPickUp;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerUpgradeManager playerUpgradeManager = other.GetComponent<playerUpgradeManager>();
            playerUpgradeManager.addToPlayerUpgrades(objToPickUp);
            Destroy(gameObject);
        }
    }
}
