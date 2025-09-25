using LordBreakerX.AttackSystem;
using LordBreakerX.Utilities;
using UnityEngine;

[System.Serializable]
public class DeathBomb : Attack
{
    private float NO_EFFECT = 0;

    [SerializeField]
    [Min(0)]
    private float _explosionRadius;

    [Header("Forces Properties")]
    [SerializeField]
    private float _maxForce;

    [Header("Damage Properties")]
    [SerializeField]
    private float _maxDamage;

    public override void OnStart()
    {
        Collider[] colliders = Physics.OverlapSphere(StartPosition, _explosionRadius);

        foreach (Collider collider in colliders)
        {
            Rigidbody rigid = collider.GetComponent<Rigidbody>();
            dealDamage damageable = collider.GetComponent<dealDamage>();

            float distance = Vector3.Distance(collider.transform.position, StartPosition);
            float attackPercentage = Percentage.Create(distance, 0, _explosionRadius);
            attackPercentage = Percentage.Reverse(attackPercentage);

            if (rigid != null)
            {
                Vector3 direction = collider.transform.position - StartPosition;

                float force = Percentage.MapToNumber(attackPercentage, NO_EFFECT, _maxForce);
                rigid.AddForce(direction * force, ForceMode.Force);
            }

            if (damageable != null)
            {
                float damage = Percentage.MapToNumber(attackPercentage, NO_EFFECT, _maxDamage);
                damageable.dealDamage(damage, Color.red, Controller.gameObject);
            }
        }
    }

    public override Attack Copy(AttackController attackController)
    {
        DeathBomb copy = new DeathBomb();
        copy._explosionRadius = _explosionRadius;
        copy._maxForce = _maxForce;
        copy._maxDamage = _maxDamage;
        return copy;
    }
}
