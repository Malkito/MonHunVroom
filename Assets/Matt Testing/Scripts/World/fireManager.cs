using UnityEngine;

public class fireManager : MonoBehaviour
{

    //Created when a fire bulelt hits an object
    //Attached to fireEffect
    //If the fireEffect this script is attached to is destroyed, either by water or the duration, decrease the number of fires


    private GameObject parent;
    private void OnDestroy()
    {
        parent = transform.parent.gameObject;
        if (parent.TryGetComponent(out dealDamage healthScript))
        {
            healthScript.decreaseFireNumber();
        }
    }

}
