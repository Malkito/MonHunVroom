using LordBreakerX.Utilities;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Resistance", menuName = "Experimental/Attack Resistance")]
public class AttackResistance : ScriptableObject
{
    [SerializeField]
    private LayerMask _resistanceLayers;

    [SerializeField]
    [Range(0.0f, 100.0f)]
    private float _resistanceThreshold = 25.0f;

    public float ResistanceThreshold { get => _resistanceThreshold; }

    public bool ObjectHasResistance(GameObject target)
    {
        return _resistanceLayers.Contains(target.layer);
    }

    public static float GetResistance(List<AttackResistance> attackResistances, GameObject target)
    {
        foreach(AttackResistance resistance in attackResistances)
        {
            if (resistance.ObjectHasResistance(target))
            {
                return resistance.ResistanceThreshold;
            }
        }

        return 0.0f;
    }

}
