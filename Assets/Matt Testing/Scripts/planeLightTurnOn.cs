using UnityEngine;
using Unity.Netcode;

public class planeLightTurnOn : NetworkBehaviour
{
    private Material mat;
    [SerializeField] private float turnOnDelay;
    private float currentTime;

    private void Awake()
    {
        mat = GetComponent<MeshRenderer>().materials[0];
    }
    void Start()
    {
        currentTime = 0;
    }

    
    void Update()
    {
        if(GameStateManager.Instance.CurrentState == GameStateManager.State.GamePlaying)
        {
            turnOnLightClientRpc();
        }     
    }


    [ClientRpc]
    private void turnOnLightClientRpc()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= turnOnDelay)
        {
            mat.color = Color.green;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.green);
        }
    }

}
