using UnityEngine;

public class oceanPlayerCatch : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<playerRespawn>().respawn();
        }
    }
}