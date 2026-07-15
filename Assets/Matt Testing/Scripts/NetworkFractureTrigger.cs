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

        if (!ShouldFractureFromCollision(collision))
            return;

        if (!IsServer)
        {
            SendFractureRequestServerRpc();
        }
        else
        {
            ProcessCollision();
        }
    }

    private bool ShouldFractureFromCollision(Collision collision)
    {
        if (fracture == null || collision.contactCount == 0 ||
            fracture.triggerOptions == null ||
            fracture.triggerOptions.triggerType != TriggerType.Collision)
            return false;

        var contact = collision.contacts[0];
        float collisionForce = collision.impulse.magnitude / Time.fixedDeltaTime;
        bool tagAllowed = fracture.triggerOptions.IsTagAllowed(
            contact.otherCollider.gameObject.tag);
        bool passesTagFilter = !fracture.triggerOptions.filterCollisionsByTag || tagAllowed;

        return collisionForce > fracture.triggerOptions.minimumCollisionForce &&
               passesTagFilter;
    }

    void ProcessCollision()
    {
        if (fractured) return;
        fractured = true;

        FractureClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void SendFractureRequestServerRpc()
    {
        if (fractured) return;

        fractured = true;
        FractureClientRpc();
    }

    [ClientRpc]
    void FractureClientRpc()
    {
        if (fracture != null)
            fracture.CauseFracture();
    }
}
