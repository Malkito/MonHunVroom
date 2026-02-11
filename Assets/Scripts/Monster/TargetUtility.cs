using LordBreakerX.AttackSystem;
using System.Collections.Generic;
using UnityEngine;

public static class TargetUtility
{
    public static AttackTarget GetRandomTarget(GameObject attacker, float targetRadius, LayerMask ignoreMask)
    {
        Collider[] overlaps = Physics.OverlapSphere(attacker.transform.position, targetRadius, ~ignoreMask, QueryTriggerInteraction.Ignore);
        List<Transform> damageables = new List<Transform>();

        foreach (Collider collider in overlaps)
        {
            dealDamage healthScript = collider.GetComponent<dealDamage>();
            if (healthScript != null) damageables.Add(collider.transform);
        }

        if (damageables.Count > 0)
        {
            int randomIndex = Random.Range(0, damageables.Count);
            Transform randomDamageable = damageables[randomIndex];
            return new AttackTarget(randomDamageable, attacker.transform.position);
        }

        return new AttackTarget(attacker.transform.position);
    }
}
