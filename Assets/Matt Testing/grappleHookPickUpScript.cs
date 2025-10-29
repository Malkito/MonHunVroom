using UnityEngine;

public class grappleHookPickUpScript : MonoBehaviour
{

    [SerializeField] GameObject grappleObject;
    private void OnDestroy()
    {
        upgradePickUp pickUpScript = GetComponent<upgradePickUp>();
        playerShooting PS = pickUpScript.playerThatPickedUpUpgrade.GetComponent<playerShooting>();
        GameObject GrappleInstance =  Instantiate(grappleObject, PS.mainBarrelEnds[0]);
        GrappleInstance.transform.SetParent(PS.mainBarrelEnds[0]);
    }







}
