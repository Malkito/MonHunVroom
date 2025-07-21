using System.Collections.Generic;
using UnityEngine;

public class DamageTable
{ 
    private Dictionary<GameObject, float> _damageRegistry = new Dictionary<GameObject, float>();

    public void ResetTable()
    {
        _damageRegistry.Clear();
    }

    public void UpdateTable(GameObject damageSource, float damage)
    {
        if (damageSource == null || damage <= 0) return;

        if (_damageRegistry.ContainsKey(damageSource))
            _damageRegistry[damageSource] += damage;
        else
            _damageRegistry[damageSource] = damage;
    }

    public GameObject GetMostDamageTarget() 
    {
        if (_damageRegistry.Count == 0) return null;

        KeyValuePair<GameObject, float> highestEntry = new KeyValuePair<GameObject, float>(null, 0);

        foreach (KeyValuePair<GameObject, float> damageEntry in _damageRegistry) 
        {
            if (damageEntry.Value > highestEntry.Value)
            {
                highestEntry = damageEntry;
            }
        }

        return highestEntry.Key;
    }
}
