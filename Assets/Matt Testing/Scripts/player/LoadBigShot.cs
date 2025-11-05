using UnityEngine;
using Unity.Netcode;    

public class LoadBigShot : NetworkBehaviour
{


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerShooting ps = other.GetComponent<playerShooting>();
            if (ps.bigShotLoaded) return;
            ps.bigShotLoaded = true;
            destroyObjectClientRpc();
        }
    }

    [ClientRpc()]
    private void destroyObjectClientRpc()
    {
        Destroy(gameObject);
    }





}
