using UnityEngine;
using Unity.Netcode;

public class fireManager : NetworkBehaviour
{

    //Created when a fire bulelt hits an object
    //Attached to fireEffect
    //If the fireEffect this script is attached to is destroyed, either by water or the duration, decrease the number of fires


    public GameObject objectFireIsAttachedTo;

    private void Start()
    {
        if (objectFireIsAttachedTo.TryGetComponent(out dealDamage healthScript)){
            healthScript.increaseFireNumber();
        }
    }


    private void OnDestroy()
    {
        if (objectFireIsAttachedTo == null) return;
        if (objectFireIsAttachedTo.TryGetComponent(out dealDamage healthScript))
        {
            healthScript.decreaseFireNumber();
        }
    }

    private void Update()
    {
        if(objectFireIsAttachedTo = null)
        {
            destroyFireServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void destroyFireServerRpc()
    {
        Destroy(gameObject);
    }






}
