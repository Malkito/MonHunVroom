using LordBreakerX.AttackSystem;
using LordBreakerX.Attributes;
using LordBreakerX.Health;
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

    [SerializeField]
    [Header("Monster Properties")]
    [TagDropdown]
    private string _monsterTag = "Monster";

    private MonsterHealth _health;

    public DeathBomb(AttackController controller) : base(controller)
    {
        _health = controller.GetComponent<MonsterHealth>();
    }

    public override void OnStart()
    {
        Vector3 startPosition = GetStartPosition();

        Collider[] colliders = Physics.OverlapSphere(startPosition, _explosionRadius);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag(_monsterTag)) continue;

            Rigidbody rigid = collider.GetComponent<Rigidbody>();
            dealDamage damageable = collider.GetComponent<dealDamage>();

            float distance = Vector3.Distance(collider.transform.position, startPosition);
            float attackPercentage = Percentage.Create(distance, 0, _explosionRadius);
            attackPercentage = Percentage.Reverse(attackPercentage);

            if (rigid != null)
            {
                Vector3 direction = (collider.transform.position - startPosition).normalized;
                float force = Percentage.MapToNumber(attackPercentage, NO_EFFECT, _maxForce);
                rigid.AddForce(direction * force * rigid.mass, ForceMode.Force);
            }

            if (damageable != null)
            {
                float damage = Percentage.MapToNumber(attackPercentage, NO_EFFECT, _maxDamage);
                damageable.dealDamage(damage, Color.red, Controller.gameObject);
            }
        }

        _health.dealDamage(100000, Color.red, _health.gameObject);
    }

    public override Attack Clone(AttackController attackController)
    {
        DeathBomb copy = new DeathBomb(attackController);
        copy._explosionRadius = _explosionRadius;
        copy._maxForce = _maxForce;
        copy._maxDamage = _maxDamage;
        return copy;
    }
}
