using UnityEngine;

public class energyShieldLogic : MonoBehaviour, useAbility
{

    [SerializeField] private GameObject shieldObject;


    public void useAbility(Transform spawnLocation , bool abilityused)
    {
        if (!abilityused) return;
       GameObject Shield = Instantiate(shieldObject, spawnLocation.position, Quaternion.identity);
       Shield.transform.SetParent(spawnLocation);
       Destroy(Shield, 10);
    }
}
