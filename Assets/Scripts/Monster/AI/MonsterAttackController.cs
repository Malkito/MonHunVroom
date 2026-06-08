using LordBreakerX.AttackSystem;
using LordBreakerX.Attributes;
using LordBreakerX.Stats;
using UnityEngine;

public sealed class MonsterAttackController : AttackController
{
    [SerializeField]
    private StatHolder _statHolder;

    [SerializeField]
    [RequiredField]
    [Header("Laser Eyes Attack")]
    private Transform[] _eyes;

    [SerializeField]
    [RequiredField]
    [Header("Throw Attack")]
    private Transform _throwPoint;

    [SerializeField]
    private DamageTable _recentDamageTable = new DamageTable();

    private Transform _currentEye;

    public bool HasTrackedDamage { get { return _recentDamageTable.HasDamage; } }

    public Transform ThrowPoint { get { return _throwPoint; } }

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

            float damage = _statHolder.GetFloat("Laser-Damage");
            Laser laser = Laser.CreateLaser(prefab, gameObject, damage, eyePosition, attackPosition);
            SpawnProjectile(laser.gameObject);
        }
    }
}