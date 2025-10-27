using UnityEngine;
using Unity.Netcode;

public class airStrikeMissle : NetworkBehaviour
{

    [SerializeField] private float explsionRadius;
    [SerializeField] private float explosonFore;
    [SerializeField] private ParticleSystem explosionParticle;
    [SerializeField] private float damage;




    private void OnCollisionEnter(Collision other)
    {
        if (!other.gameObject.CompareTag("Ground")) return;

        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, explsionRadius);
        foreach (Collider col in collidersInRange)
        {
            if(col.CompareTag("Monster"))
            {
                if(col.gameObject.TryGetComponent(out dealDamage healthScript))
                {
                    healthScript.dealDamage(damage, Color.red, gameObject);
                }
            }


            Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 launchDirection = (rb.transform.position - transform.position).normalized;
                float distance = Vector3.Distance(rb.transform.position, transform.position);
                //print("Name: " + rb.name + " Distance: " + Vector3.Distance(rb.transform.position, transform.position) + " Foce Applied: " + (launchDirection * (explosonFore - distance)));
                rb.AddForce(launchDirection * (explosonFore - distance), ForceMode.VelocityChange);
            }
        }
        destroyMissleServerRpc();
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explsionRadius);
    }

    [ServerRpc(RequireOwnership = false)]
    private void destroyMissleServerRpc()
    {
        Destroy(gameObject);
        //spawn explosin particles;
    }

}
