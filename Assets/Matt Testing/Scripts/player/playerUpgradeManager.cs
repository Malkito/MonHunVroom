using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 
/// This script handles the upgrades that the player can use.
/// 
/// Can add / remove upgrades to the current availble upgrades
/// 
/// Checks the Logic script on the upgrade ScriptableOBJ and uses the "useAbility" function
/// 
/// </summary>

interface useAbility
{ 
    public void useAbility(Transform position, bool abilityPressed);
}


public class playerUpgradeManager : MonoBehaviour
{
    public UpgradeScriptableOBJ[] currentUpgrades = new UpgradeScriptableOBJ[3]; // the array current upgrades availble to the player, with a max of3

    private int currentUpgradeCount; // the current number of upgrades the player has


    //The ability cooldowns
    public float abilityOneCooldown;
    public float abilityTwoCooldown;
    public float abilityThreeCooldown;

    public bool canUseUpgrade; // bool that allows the player to use upgrades. Turned off while player is dead

    private void Start()
    {
        canUseUpgrade = true;

        //Starts all cooldowns at 0
        abilityOneCooldown = 0;
        abilityTwoCooldown = 0;
        abilityThreeCooldown = 0;
    }
    public void addToPlayerUpgrades(UpgradeScriptableOBJ upgradeToAdd) // this function adds a new upgrades to the lsit. Called by "Upgrade Pick Up" script 
    {
        if(currentUpgradeCount < currentUpgrades.Length) // if the number of upgrades is less than the max allowed
        {
            
            currentUpgrades[currentUpgradeCount] = upgradeToAdd; // adds the upgrade to the oppropiate postion
            currentUpgradeCount++; //increse the number of upgrades the player has
        }
        else // if the numebr of upgrades is max, shuffles the upgrades forward on spot, then drops the "oldest" upgrade
        {
            GameObject pickUpObject = Instantiate(currentUpgrades[0].pickupObject, transform.position, Quaternion.identity); // drops the "oldest upgrade"

            upgradePickUp upgradePickUpScrit = pickUpObject.GetComponent<upgradePickUp>(); // sets certain conditons on the upgrade pick up script to ensure that no "pick up loops" occor. Pick up loops being when the upgrades are constanly switching,picking up and dropping
            upgradePickUpScrit.canBePickedUp = false;
            upgradePickUpScrit.dropped = true;

            currentUpgrades[0] = currentUpgrades[1]; // shuffles upgrade in second pos to first pos
            abilityOneCooldown = abilityTwoCooldown;

            currentUpgrades[1] = currentUpgrades[2];// shuffles upgrade in third pos to second pos
            abilityTwoCooldown = abilityThreeCooldown;

            currentUpgrades[2] = upgradeToAdd; // adds new upgrade to third pos
            abilityThreeCooldown = 0;

        }
    }

    private void Update()
    {
        if (!canUseUpgrade) return; // if the player cant use upgrades, ignore below


        /*
         * Explanations of Logic Scripts:
         * 
         * Each upgrade has an unique logic script, set in the inspector for that upgradeScriptableObject
         * 
         * Logic scripts handles the moment the ability is used, aswell as any logic that is required if the input is not being pressed.
         * 
         * the reason for logic scripts is because many ugrades must read the input in a unique way.
         * 
         * Example:
         *         Airstrike: If the input is pressed, fires a flare to call in an airstrike, starts cooldown
         *         Jet pack: If the input is held, apply the upward force and decrease the "fuel" of the jetpack, if the input is let go of, recharge the "Feul". The jet pack ignores the cooldown
         *         Energy Sphere: if the input is pressed, set the ALTattack Bullet Data to the Energy Sphere data. Then after a duration, revert the change. starts cooldown
         * 
         * 
         * Some upgrades uses the same logic script because they read the input the same.
         * 
         * Example:
         *           Airstrike: If the input is pressed, fires a flare to call in an airstrike
         *           Turret: If input is pressed, fires a turret
         *           
         *           Energy Sphere: if the input is pressed, set the ALTattack Bullet Data to the Energy Sphere data. Then after a duration, revert the change. starts cooldown
         *           Water Grenade: if the input is pressed, set the ALTattack Bullet Data to the Water Grenade data. Then after a duration, revert the change. starts cooldown
         *           
         * Most logic scripts act the same, so far, only the jetpack is super uniqie, and the grapple hook has a unique system for pickup explained in the "GrappleHookPickUpScript"  
         *           
         *       
         */


        /* Each block of code below checks the input for each position. Q checks pos 1, E checks pos 2, R checks pos 3. 
         * 
         * 
         * if the input for the repsective upgrade is postiive, it uses the "Use ability" function sending the arguement of true
         * if the input is negative, it uses the "Use ability" function sending the arguement of false
         * 
         * in either case, checks to see if there is even an upgrade in the postion in the first place
         * 
         * 
         * Also sets the the cooldown if the ability is used
         * 
         */



        ///////////Upgrade 1//////////////
        if (currentUpgrades[0] !=null && GameInput.instance.getAbilityOneInput() &&  currentUpgrades[0].logicScriptObject.TryGetComponent(out useAbility abilityOneScriptInUse) && abilityOneCooldown <= 0)
        {
            abilityOneScriptInUse.useAbility(transform, true);
            abilityOneCooldown = currentUpgrades[0].cooldown;
        }else if (currentUpgrades[0] != null && currentUpgrades[0].logicScriptObject.TryGetComponent(out useAbility abilityOneScriptNotInUse))
        {
            abilityOneScriptNotInUse.useAbility(transform, false);
        }


        ///////////Upgrade 2///////////////
        if (currentUpgrades[1] != null && GameInput.instance.getAbilityTwoInput() && currentUpgrades[1].logicScriptObject.TryGetComponent(out useAbility abilityTwoScriptInUse) && abilityTwoCooldown <= 0)
        {
            abilityTwoScriptInUse.useAbility(transform, true);
            abilityTwoCooldown = currentUpgrades[1].cooldown;
        }
        else if (currentUpgrades[1] != null && currentUpgrades[1].logicScriptObject.TryGetComponent(out useAbility abilityTwoScriptNotInUse))
        {
            abilityTwoScriptNotInUse.useAbility(transform, false);
        }



        ///////////Upgrade 3///////////////
        if (currentUpgrades[2] != null && GameInput.instance.getAbilityThreeInput() && currentUpgrades[2].logicScriptObject.TryGetComponent(out useAbility abilityThreeScriptInUse) && abilityThreeCooldown <= 0)
        {
            abilityThreeScriptInUse.useAbility(transform, true);
            abilityThreeCooldown = currentUpgrades[2].cooldown;
        }
        else if (currentUpgrades[2] != null && currentUpgrades[2].logicScriptObject.TryGetComponent(out useAbility abilityThreeScriptNotInUse))
        {
            abilityThreeScriptNotInUse.useAbility(transform, false);
        }

        //Countdowns cooldowns
        abilityOneCooldown -= Time.deltaTime;
        abilityTwoCooldown -= Time.deltaTime;
        abilityThreeCooldown -= Time.deltaTime;
    }

}
