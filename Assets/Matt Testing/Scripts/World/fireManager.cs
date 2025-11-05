using UnityEngine;
using Unity.Netcode;

public class fireManager : NetworkBehaviour
{

    //Created when a fire bulelt hits an object
    //Attached to fireEffect
    //If the fireEffect this script is attached to is destroyed, either by water or the duration, decrease the number of fires


    public GameObject objectFireIsAttachedTo;
    private NetworkObject netOBJ;

    private ParticleSystem fireParticle;

    private float currentTime;

    private void Awake()
    {
        fireParticle = transform.GetChild(0).GetComponent<ParticleSystem>();
        netOBJ = GetComponent<NetworkObject>();
    }

    
    private void Start()
    {
        currentTime = 0;
        if (objectFireIsAttachedTo.TryGetComponent(out dealDamage healthScript)){
            healthScript.increaseFireNumber();
        }
    }


    private void endFire()
    {
        if (objectFireIsAttachedTo.TryGetComponent(out dealDamage healthScript))
        {
            healthScript.decreaseFireNumber();
        }
    }

    private void Update()
    {
        if(objectFireIsAttachedTo == null)
        {
            destroyFireServerRpc();
        }

        currentTime += Time.deltaTime;
        if(currentTime >= fireParticle.main.duration)
        {
            endFireServerRpc();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void endFireServerRpc()
    {
        endFire();
        netOBJ.Despawn();
        Destroy(gameObject);
    }


    [ServerRpc(RequireOwnership = false)]
    private void destroyFireServerRpc()
    {
        netOBJ.Despawn();
        Destroy(gameObject);
    }






}
