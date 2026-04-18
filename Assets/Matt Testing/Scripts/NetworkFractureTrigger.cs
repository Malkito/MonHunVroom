using UnityEngine;
using Unity.Netcode;

public class NetworkFractureTrigger : NetworkBehaviour
{
    private Fracture fracture;
    private bool fractured;

    void Awake()
    {
        fracture = GetComponent<Fracture>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (fractured) return;

        if (!fracture.ShouldFractureFromCollision(collision))
            return;

        if (!IsServer)
        {
            // CLIENT: send full trusted data
            var contact = collision.contacts[0];
            float force = collision.impulse.magnitude / Time.fixedDeltaTime;

            SendFractureRequestServerRpc(force, contact.point);
        }
        else
        {
            ProcessCollision(collision);
        }


    }

    void ProcessCollision(Collision collision)
    {
        if (fractured) return;
        fractured = true;

        var contact = collision.contacts[0];
        float force = collision.impulse.magnitude / Time.fixedDeltaTime;

        FractureClientRpc(force, contact.point);
    }

    [ServerRpc(RequireOwnership = false)]
    void SendFractureRequestServerRpc(float force, Vector3 hitPoint)
    {
        if (fractured) return;

        fractured = true;
        FractureClientRpc(force, hitPoint);
    }

    [ClientRpc]
    void FractureClientRpc(float impactForce, Vector3 hitPoint)
    {
        fracture.CauseFractureWithForce(impactForce, hitPoint);
    }
}