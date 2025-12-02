using UnityEngine;
using Unity.Netcode;


public class tankParachute : NetworkBehaviour
{
    private bool parachuteActive;
    [SerializeField] private GameObject parachute;
    [SerializeField] private Rigidbody rb;


    private void Update()
    {
        if (!IsLocalPlayer) return;

        if(gameObject.transform.position.y >= 120 && !parachuteActive)
        {
            activateParachute();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            deactivateParachute();
        }
    }

    private void activateParachute()
    {
        activateParachuiteClientRpc(true);
        rb.linearDamping = 0.2f;
    }

    private void deactivateParachute()
    {
        activateParachuiteClientRpc(false);
        rb.linearDamping = 0;
    }

    [ClientRpc]
    private void activateParachuiteClientRpc(bool active)
    {
        parachute.SetActive(active);
        parachuteActive = active;

    }


}
