using UnityEngine;

public class Unstick : MonoBehaviour
{
    private playerRespawn Respawn;

    private void Awake()
    {
        Respawn = GetComponent<playerRespawn>();
    }


    private void Update()
    {
        if (GameInput.instance.getUnstickInput())
        {
            Respawn.respawn();
        }
    }
}
