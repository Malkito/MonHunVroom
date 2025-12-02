using UnityEngine;
using Unity.Netcode;

public abstract class PowerUpBase : NetworkBehaviour
{
    protected PowerUpManager manager;

    [SerializeField] UpgradeScriptableOBJ powerUpSO;

    // Cooldown variables
    [SerializeField] protected float cooldownDuration = 1f;
    protected float lastUsedTime = -999f;

    public virtual void Initialize(PowerUpManager mgr)
    {
        manager = mgr;
    }

    protected bool CanUse()
    {
        return Time.time >= lastUsedTime + cooldownDuration;
    }

    protected void TriggerCooldown()
    {
        lastUsedTime = Time.time;
    }

    /// <summary>
    /// Called every frame (ONLY for the slot this power-up occupies)
    /// </summary>
    public abstract void HandleInput(bool held, bool pressed);

    public virtual void OnRemoved() { }
}
