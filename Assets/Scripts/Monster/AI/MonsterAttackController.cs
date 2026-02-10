using LordBreakerX.AttackSystem;
using LordBreakerX.Attributes;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackController : AttackController
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

    public bool AttackRandomObject(float targetRadius, LayerMask ignoreMask)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, targetRadius, ~ignoreMask, QueryTriggerInteraction.Ignore);
        List<Transform> damageables = new List<Transform>();

        foreach (var collider in colliders)
        {
            dealDamage healthScript = collider.GetComponent<dealDamage>();
            if (healthScript != null) damageables.Add(collider.transform);
        }

        if (damageables.Count > 0)
        {
            int randomIndex = Random.Range(0, damageables.Count);
            Transform randomDamageable = damageables[randomIndex];
            Target = new AttackTarget(randomDamageable, transform.position);
            return true;
        }

        return false;
    }

    //public AttackTarget GetRandomTarget(float targetRadius, LayerMask ignoreMask)
    //{
    //    Collider[] colliders = Physics.OverlapSphere(transform.position, targetRadius, ~ignoreMask, QueryTriggerInteraction.Ignore);
    //    List<Transform> damageables = new List<Transform>();

    //    foreach (var collider in colliders)
    //    {
    //        dealDamage healthScript = collider.GetComponent<dealDamage>();
    //        if (healthScript != null) damageables.Add(collider.transform);
    //    }

    //    if (damageables.Count > 0)
    //    {
    //        int randomIndex = Random.Range(0, damageables.Count);
    //        Transform randomDamageable = damageables[randomIndex];
    //        Target.Set(randomDamageable, transform.position);
    //        return new AttackTarget();
    //    }

    //    return false;
    //}
}