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

    [SerializeField]
    [Range(0, 100)]
    private float _reachedTargetPercentage = 50;

    [SerializeField]
    private float _timeBeforeExplosion = 3;

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
    private MonsterMovementController _movement;
    private MonsterAttackController _attack;

    private float _reachedDistance;

    private bool _attackComplete;

    private bool _exploding;

    private Timer _explodeTimer;

    public DeathBomb(AttackController controller) : base(controller)
    {
        _health = controller.GetComponent<MonsterHealth>();
        _movement = controller.GetComponent<MonsterMovementController>();
        _attack = controller.GetComponent<MonsterAttackController>();
        _explodeTimer = new Timer(_timeBeforeExplosion);
        _explodeTimer.OnTimerFinished += Explode;
    }

    public override void OnStart()
    {
        _attackComplete = false;
        _reachedDistance = Percentage.MapToNumber(_reachedTargetPercentage, 0, _explosionRadius);
        _explodeTimer.Reset();
    }

    public override void OnStop()
    {
        _attack.StopEffect(MonsterAttackEffect.PreparingDeathBomb);
    }

    public override void OnAttackUpdate()
    {
        if (_exploding) return;

        if (_movement.ReachedDestination(_reachedDistance))
        {
            _movement.StopMovement();
            _movement.UpdateWalkAnimation(false);
            _explodeTimer.Update();
            _attack.PlayEffect(MonsterAttackEffect.PreparingDeathBomb);
        }else
        {
            Vector3 targetPosition = GetTargetPosition();
            _movement.ChangeDestination(targetPosition);
        }
    }

    private void Explode()
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

        _health.dealDamage(999999, Color.red, _health.gameObject);

        _attackComplete = true;
    }

    public override bool HasAttackFinished()
    {
        return _attackComplete;
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
