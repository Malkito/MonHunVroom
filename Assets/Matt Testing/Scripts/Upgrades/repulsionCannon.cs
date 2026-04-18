using UnityEngine;
using Unity.Netcode;

public class repulsionCannon : NetworkBehaviour
{
    [SerializeField] private float repulsionRadius;
    [SerializeField] private float repulsionForce;


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Fire();
        }
    }


    public void Fire()
    {
        if (IsOwner)
        {
            FireServerRpc(transform.position);
        }
    }

    [ServerRpc]
    private void FireServerRpc(Vector3 origin)
    {
        Collider[] collidersInRange = Physics.OverlapSphere(origin, repulsionRadius);

        foreach (Collider col in collidersInRange)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 launchDirection = (rb.transform.position - origin).normalized;

                rb.AddForce(launchDirection * repulsionForce, ForceMode.Impulse);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, repulsionRadius);
    }
}
