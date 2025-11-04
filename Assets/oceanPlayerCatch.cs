using UnityEngine;

public class oceanPlayerCatch : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            respawnManager.Instance.StartSpawnPlayer(other.transform);
        }
    }






}
