using UnityEngine;
using System.Collections.Generic;

public class AbilityDatabase : MonoBehaviour
{
    public static AbilityDatabase Instance;

    [Header("Index = Upgrade ID")]
    [SerializeField] private AbilityScriptableOBJ[] upgrades;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // =========================
    // GET BY INT ID
    // =========================
    public AbilityScriptableOBJ Get(int id)
    {
        if (id < 0 || id >= upgrades.Length)
        {
            Debug.LogError($"Invalid Upgrade ID: {id}");
            return null;
        }

        return upgrades[id];
    }

    // =========================
    // OPTIONAL HELPERS
    // =========================

    public int Count => upgrades.Length;

    public AbilityScriptableOBJ GetRandom()
    {
        return upgrades[Random.Range(0, upgrades.Length)];
    }

    public int GetRandomID()
    {
        return Random.Range(0, upgrades.Length);
    }
}