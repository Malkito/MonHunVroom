using UnityEngine;

public class fireManager : MonoBehaviour
{
    //goes onto fireEffect object, starts the damage over time, 
    //stops the damage over time, destroy object

    public BuildingHealth buildingHealthFireManager;

    public void startDamageOverTime(float damage, float duration)
    {
        buildingHealthFireManager.applyDamageOverTime(damage, duration);
    }


    public void stopDamageOverTime()
    {
        buildingHealthFireManager.stopDamageOverTime();
    }


}
