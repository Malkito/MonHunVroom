using UnityEngine;

public class repulsionCannon : MonoBehaviour
{
    [SerializeField] private float repulsionRadius;
    [SerializeField] private float repulsionForce;



    private void Awake()
    {
        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, repulsionRadius);
        foreach(Collider col in collidersInRange)
        {
            Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 launchDirection = (rb.transform.position - transform.position).normalized;
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
