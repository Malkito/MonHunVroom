using UnityEngine;
using Unity.Netcode;

public class upgradePickUp : NetworkBehaviour
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
    [SerializeField] private int objectArrayIndex;


    private void Start()
    {
        canBePickedUp = true;
        if (dropped) canBePickedUp = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canBePickedUp)
        {
            //Player has picked up the upgrade

            playerThatPickedUpUpgrade = other.gameObject; // Unique identifier used for the grapple hook upgrade

            playerUpgradeManager playerUpgradeManager = other.GetComponent<playerUpgradeManager>();
            playerUpgradeManager.addToPlayerUpgrades(objectArrayIndex); // Makes the upgrde avaible to the player
            print("Added upgrade to array");
            destroyPickUpClientRpc();
        }
    }

    [ClientRpc()]
    private void destroyPickUpClientRpc()
    {
        print("Destroying object pick up");
        Destroy(gameObject);
    }

    

    private void OnTriggerExit(Collider other)
    {
        canBePickedUp = true;
    }


}
