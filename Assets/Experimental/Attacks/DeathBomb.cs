using LordBreakerX.Utilities.Math;
using UnityEngine;

public class DeathBomb : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem _particleSystem;

    [SerializeField]
    [Min(0)]
    private float _explosionRadius;

    [Header("Forces Properties")]
    [SerializeField]
    private float _minForce;

    [SerializeField]
    private float _maxForce;

    [Header("Damage Properties")]
    [SerializeField]
    private float _minDamage;

    [SerializeField]
    private float _maxDamage;

    private void Awake()
    {
        _particleSystem.Play();
        Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionRadius);

        foreach(Collider collider in colliders)
        {
            Rigidbody rigid = collider.GetComponent<Rigidbody>();
            dealDamage damageable = collider.GetComponent<dealDamage>();

            float distance = Vector3.Distance(collider.transform.position, transform.position);

            if (rigid != null)
            {
                Vector3 direction = collider.transform.position - transform.position;
                float forcePercentage = PercentageUtility.InvertedPercentageNormalized(distance, 0, _explosionRadius);
                float force = PercentageUtility.MapNormalizedPercentage(forcePercentage, _minForce, _maxForce);
                rigid.AddForce(direction * force, ForceMode.Force);
            }

            if (damageable != null)
            {
                float damagePercentage = PercentageUtility.InvertedPercentageNormalized(distance, 0, _explosionRadius);
                float damage = PercentageUtility.MapNormalizedPercentage(damagePercentage, _minDamage, _maxDamage);
                damageable.dealDamage(damage, Color.red, gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
}
