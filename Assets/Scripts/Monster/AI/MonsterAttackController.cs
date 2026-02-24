using LordBreakerX.AttackSystem;
using LordBreakerX.Attributes;
using UnityEngine;

public sealed class MonsterAttackController : AttackController
{
    [SerializeField]
    [RequiredField]
    [Header("Laser Eyes")]
    private Transform[] _eyes;

    [SerializeField]
    private DamageTable _recentDamageTable = new DamageTable();

    private Transform _currentEye;

    public bool HasTrackedDamage { get { return _recentDamageTable.HasDamage; } }

    public void ChooseEye()
    {
        _currentEye = _eyes[Random.Range(0, _eyes.Length)];
    }

    public void ShootLaser(Laser prefab, Vector3 attackPosition)
    {
        if (IsServer)
        {
            if (_currentEye == null)
                ChooseEye();

            Vector3 eyePosition = _currentEye.position;

            Laser laser = Laser.CreateLaser(prefab, gameObject, eyePosition, attackPosition);
            SpawnProjectile(laser.gameObject);
        }
    }

    public void AttackRandomObject(float targetRadius, LayerMask ignoreMask)
    {
        Target = TargetUtility.GetRandomTarget(gameObject.transform.position, targetRadius, ignoreMask);

        if (Target.GetPosition() != transform.position)
        {
            StartRandomAttack();
        }
    }
}