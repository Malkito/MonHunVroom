using UnityEngine;

public class airStrikeMissle : MonoBehaviour
{

    [SerializeField] private float explsionRadius;
    [SerializeField] private float explosonFore;
    [SerializeField] private ParticleSystem explosionParticle;





    private void OnCollisionEnter()
    {
        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, explsionRadius);
        foreach (Collider col in collidersInRange)
        {
            Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 launchDirection = (rb.transform.position - transform.position).normalized;
                float distance = Vector3.Distance(rb.transform.position, transform.position);
                print("Name: " + rb.name + " Distance: " + Vector3.Distance(rb.transform.position, transform.position) + " Foce Applied: " + (launchDirection * (explosonFore - distance)));
                rb.AddForce(launchDirection * (explosonFore - distance), ForceMode.Impulse);
            }
        }
        //explosionParticle.Play();
        Destroy(gameObject);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explsionRadius);
    }
}
