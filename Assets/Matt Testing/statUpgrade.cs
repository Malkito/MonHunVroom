using UnityEngine;
using Unity.Netcode;

public class statUpgrade : NetworkBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            destryoObjClientRpc();
        }
    }

    [ClientRpc]
    private void destryoObjClientRpc()
    {
        Destroy(gameObject);
    }






}
