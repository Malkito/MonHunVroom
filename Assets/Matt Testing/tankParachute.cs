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
        parachute.SetActive(true);
        rb.linearDamping = 0.2f;
        parachuteActive = true;
    }

    private void deactivateParachute()
    {
        parachute.SetActive(false);
        rb.linearDamping = 0;
        parachuteActive = false;
    }
}
