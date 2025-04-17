using UnityEngine;

public class upgradePickUp : MonoBehaviour
{
    /// <summary>
    /// 
    /// This script is on the upgrade Pick up objects
    ///
    /// Various flags are used to ensure there is no pick up loop
    /// 
    /// </summary>


    public UpgradeScriptableOBJ objToPickUp;
    [HideInInspector] public bool canBePickedUp;
    [HideInInspector] public bool dropped = false;
    [HideInInspector] public GameObject playerThatPickedUpUpgrade;


    private void Start()
    {
        canBePickedUp = true;
        if (dropped) canBePickedUp = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canBePickedUp)
        {
            //PLayer has picked up the upgrade


            playerThatPickedUpUpgrade = other.gameObject; // Unique idenedifier used for the grapple hook upgrade

            playerUpgradeManager playerUpgradeManager = other.GetComponent<playerUpgradeManager>();
            playerUpgradeManager.addToPlayerUpgrades(objToPickUp); // Makes the upgrde avaible to the player

            Destroy(gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        canBePickedUp = true;
    }


}
