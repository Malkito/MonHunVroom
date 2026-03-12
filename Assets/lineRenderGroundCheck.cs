using UnityEngine;
using Unity.Netcode;


public class lineRenderGroundCheck : NetworkBehaviour
{
    private TrailRenderer TrailRenderer;

    private void Start()
    {
        if (!IsOwner) return;

        TrailRenderer = gameObject.GetComponent<TrailRenderer>();

        TrailRenderer.emitting = false;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            TrailRenderer.emitting = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            TrailRenderer.emitting = false;
        }
    }

}
