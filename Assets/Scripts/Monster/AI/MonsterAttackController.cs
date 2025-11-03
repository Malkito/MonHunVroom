using LordBreakerX.AttackSystem;
using LordBreakerX.Attributes;
using LordBreakerX.Health;
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

    [Header("Particles Properties")]
    [SerializeField]
    private ParticleSystem _preparingExplosionEffect;

    [SerializeField]
    private ParticleSystem _stompEffect;

    [SerializeField]
    private DamageTable _recentDamageTable = new DamageTable();

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

    public void PlayEffect(MonsterAttackEffect effectType)
    {
        switch(effectType)
        {
            case MonsterAttackEffect.PreparingDeathBomb:
                _preparingExplosionEffect.Play();
                break;
            case MonsterAttackEffect.Stomp:
                _stompEffect.Play();
                break;
        }
    }

    public void AdjustExplosionEffectRadius(float radius)
    {
        ParticleSystem.ShapeModule shape = _preparingExplosionEffect.shape;
        shape.radius = radius;
    }

    public void StopEffect(MonsterAttackEffect effectType)
    {
        switch (effectType)
        {
            case MonsterAttackEffect.PreparingDeathBomb:
                _preparingExplosionEffect.Stop();
                break;
            case MonsterAttackEffect.Stomp:
                _stompEffect.Stop();
                break;
        }
    }
}