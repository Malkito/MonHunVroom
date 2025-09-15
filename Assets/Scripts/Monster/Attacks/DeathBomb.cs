using LordBreakerX.AttackSystem;
using LordBreakerX.Utilities;
using UnityEngine;

public class DeathBomb : Attack
{
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

    public override void OnStart()
    {
        //_particleSystem.Play();
        Collider[] colliders = Physics.OverlapSphere(Controller.transform.position, _explosionRadius);

        foreach (Collider collider in colliders)
        {
            Rigidbody rigid = collider.GetComponent<Rigidbody>();
            dealDamage damageable = collider.GetComponent<dealDamage>();

            float distance = Vector3.Distance(collider.transform.position, Controller.transform.position);
            float attackPercentage = Percentage.Create(distance, 0, _explosionRadius);
            attackPercentage = Percentage.Reverse(attackPercentage);

            if (rigid != null)
            {
                Vector3 direction = collider.transform.position - Controller.transform.position;

                float force = Percentage.MapToNumber(attackPercentage, _minForce, _maxForce);
                rigid.AddForce(direction * force, ForceMode.Force);
            }

            if (damageable != null)
            {
                float damage = Percentage.MapToNumber(attackPercentage, _minDamage, _maxDamage);
                damageable.dealDamage(damage, Color.red, Controller.gameObject);
            }
        }
    }

    public override Attack Copy(AttackController attackController)
    {
        DeathBomb copy = new DeathBomb();
        copy._explosionRadius = _explosionRadius;
        copy._minForce = _minForce;
        copy._maxForce = _maxForce;
        copy._minDamage = _minDamage;
        copy._maxDamage = _maxDamage;
        return copy;
    }
}
