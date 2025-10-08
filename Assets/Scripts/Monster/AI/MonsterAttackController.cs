using LordBreakerX.AttackSystem;
using LordBreakerX.Attributes;
using LordBreakerX.Health;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MonsterAttackController : AttackController
{
    [SerializeField]
    [RequiredField]
    [Header("Laser Eyes")]
    private Transform[] _eyes;

    [SerializeField]
    [RequiredField]
    private Laser _laserPrefab;

    [Header("Targetting Player Properties")]
    [SerializeField]
    [Min(0f)]
    private float _timeBetweenPlayerAttacks = 30;

    [Header("Flying Properties")]
    [SerializeField]
    private Transform _model;

    private DamageTable _recentDamageTable = new DamageTable();

    public Transform Model { get { return _model; } }

    public bool HasTrackedDamage { get { return _recentDamageTable.HasDamage; } }

    public void RequestShootLaser(Laser prefab, Vector3 attackPosition)
    {
        if (IsServer)
        {
            ShootLaser(prefab, attackPosition);
            ShootLaserClientRpc(attackPosition);
        }
    }

    private void ShootLaser(Laser prefab, Vector3 attackPosition)
    {
        int randomEyeIndex = Random.Range(0, _eyes.Length);
        Vector3 eyePosition = _eyes[randomEyeIndex].position;

        Laser.CreateLaser(prefab, gameObject, eyePosition, attackPosition);
    }

    [ClientRpc(RequireOwnership = false)]
    private void ShootLaserClientRpc(Vector3 attackPosition)
    {
        if (!IsHost || !IsServer) ShootLaser(_laserPrefab, attackPosition);
    }

    public void OnMonsterHealthChanged(HealthInfo healthInfo)
    {
        if (IsServer)
            _recentDamageTable.UpdateTable(healthInfo.Source, healthInfo.DamageCaused);
    }

    public void ResetDamageTable()
    {
        if (IsServer) 
            _recentDamageTable.ResetTable();
    }
}