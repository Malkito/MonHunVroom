using LordBreakerX.AttackSystem;
using System.Collections.Generic;
using UnityEngine;

public static class TargetUtility
{
    public static AttackTarget GetRandomTarget<THealth>(AttackController attackController) 
    {
        Vector3 startPosition = attackController.transform.position;
        float attackRadius = attackController.RandomAttackRadius;
        LayerMask ignoredLayers = attackController.IgnoredLayers;

        return GetRandomTarget<THealth>(startPosition, attackRadius, ignoredLayers);
    }

    public static AttackTarget GetRandomTarget<THealth>(Vector3 startPosition, float attackRadius, LayerMask ignoredLayers)
    {
        Collider[] overlaps = Physics.OverlapSphere(startPosition, attackRadius, ~ignoredLayers, QueryTriggerInteraction.Ignore);
        List<Transform> damageables = new List<Transform>();

        foreach (Collider collider in overlaps)
        {
            THealth healthScript = collider.GetComponent<THealth>();

            if (healthScript != null) 
                damageables.Add(collider.transform);
        }

        if (damageables.Count > 0)
        {
            int randomIndex = Random.Range(0, damageables.Count);
            Transform randomDamageable = damageables[randomIndex];
            return new AttackTarget(randomDamageable, startPosition);
        }

        return new AttackTarget(startPosition);
    }
}
