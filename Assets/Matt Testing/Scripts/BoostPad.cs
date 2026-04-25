using UnityEngine;
using Unity.Netcode;

public class BoostPad : NetworkBehaviour
{

    [SerializeField] private GameObject BoostSphere;
    private bool isActive;
    [SerializeField] private float TimeBeforeReset;
    private float currentTime;

    void Start()
    {      
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= TimeBeforeReset)
            {
                ActivateClientRpc();
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && isActive)
        {
            speedBoost SpeedBoost = other.GetComponent<speedBoost>();
            if (SpeedBoost.AddBoost())
            {
                DeactivateClientRpc();
            }
        }
    }

    [ClientRpc]
    private void DeactivateClientRpc()
    {
        currentTime = 0;
        isActive = false;
        BoostSphere.SetActive(false);
    }


    [ClientRpc]
    private void ActivateClientRpc()
    {
        BoostSphere.SetActive(true);
        isActive = true;
    }





}
