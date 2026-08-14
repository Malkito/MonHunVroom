using UnityEngine;

public class Unstick : MonoBehaviour
{
    private playerRespawn Respawn;

    private bool UnstickPress;  

    private void Awake()
    {
        Respawn = GetComponent<playerRespawn>();
    }


    private void Update()
    {
        if (GameInput.instance.getUnstickInput() && !UnstickPress)
        {
            UnstickPress = true;
            Respawn.respawn();
        }
    }
}
