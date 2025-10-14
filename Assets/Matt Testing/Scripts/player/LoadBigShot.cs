using UnityEngine;
using Unity.Netcode;    

public class LoadBigShot : NetworkBehaviour
{


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<playerShooting>().bigShotLoaded = true;
            destroyObjectClientRpc();
        }
    }

    [ClientRpc()]
    private void destroyObjectClientRpc()
    {
        Destroy(gameObject);
    }





}
