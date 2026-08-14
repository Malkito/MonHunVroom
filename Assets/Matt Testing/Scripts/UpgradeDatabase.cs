using UnityEngine;
using System.Collections.Generic;

public class UpgradeDatabase : MonoBehaviour
{
    public static UpgradeDatabase Instance;

    [Header("Index = Upgrade ID")]
    [SerializeField] private UpgradeScriptableOBJ[] upgrades;

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
    public UpgradeScriptableOBJ Get(int id)
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

    public UpgradeScriptableOBJ GetRandom()
    {
        return upgrades[Random.Range(0, upgrades.Length)];
    }

    public int GetRandomID()
    {
        return Random.Range(0, upgrades.Length);
    }
}