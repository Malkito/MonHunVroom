using UnityEngine;
using Unity.Netcode;

public class rearLights : NetworkBehaviour
{
    [SerializeField] private Material rearLightsMat;
    [SerializeField] private GameObject[] lights;

    void Update()
    {
        if (!IsOwner) return;

        Vector2 inputVector = GameInput.instance.getMovementInputNormalized();
        if (inputVector.y < 0)
        {
            rearLightsMat.EnableKeyword("_EMISSION");
            rearLightsMat.SetColor("_EmissionColor", Color.red);

            foreach (GameObject light in lights)
            {
                light.SetActive(true);
            }
        }
        else
        {
            rearLightsMat.EnableKeyword("_EMISSION");
            rearLightsMat.SetColor("_EmissionColor", Color.black);
            foreach (GameObject light in lights)
            {
                light.SetActive(false);
            }
        }


        
    }
}
