using CupOHappiness;
using Niki.UI;
using UnityEngine;
using UnityServiceLocator;

public class PlayerStatePresenter : MonoBehaviour
{
    public ProceduralProgressBar HealthBar;
    public ProceduralProgressBar ManaBar;

    private IReadOnlyProperty<float> manaFill;
    private IReadOnlyProperty<float> healthFill;
    private float previousMana;
    private float previousHealth;

    private void Start()
    {
        var locator = ServiceLocator.For(this);
        if (locator.TryGet<ManaSystem>(out var manaSystem))
            BindMana(manaSystem.ManaFill);
        else
            Debug.LogError($"[{name}] ManaSystem is not registered in this player's ServiceLocator.", this);

        if (locator.TryGet<playerHealth>(out var healthSystem))
            BindHealth(healthSystem.HealthFill);
        else
            Debug.LogError($"[{name}] playerHealth is not registered in this player's ServiceLocator.", this);
    }

    public void BindMana(IReadOnlyProperty<float> source)
    {
        if (manaFill != null)
            manaFill.ValueChanged -= OnManaChanged;

        manaFill = source;
        if (manaFill == null || ManaBar == null)
        {
            Debug.LogError($"[{name}] Cannot bind mana: source or ManaBar is missing.", this);
            return;
        }

        previousMana = Mathf.Clamp01(manaFill.Value);
        ManaBar.UpdateBarFillAmount(previousMana);
        if (isActiveAndEnabled)
            manaFill.ValueChanged += OnManaChanged;
        Debug.Log($"[{name}] Bound mana at {previousMana:0.###}.", this);
    }

    public void BindHealth(IReadOnlyProperty<float> source)
    {
        if (healthFill != null)
            healthFill.ValueChanged -= OnHealthChanged;

        healthFill = source;
        if (healthFill == null || HealthBar == null)
        {
            Debug.LogError($"[{name}] Cannot bind health: source or HealthBar is missing.", this);
            return;
        }

        previousHealth = Mathf.Clamp01(healthFill.Value);
        HealthBar.UpdateBarFillAmount(previousHealth);
        if (isActiveAndEnabled)
            healthFill.ValueChanged += OnHealthChanged;
        Debug.Log($"[{name}] Bound health at {previousHealth:0.###}.", this);
    }

    private void OnManaChanged(float value) => UpdateBar("Mana", ManaBar, ref previousMana, value);
    private void OnHealthChanged(float value) => UpdateBar("Health", HealthBar, ref previousHealth, value);

    private void UpdateBar(string label, ProceduralProgressBar bar, ref float previous, float value)
    {
        float fill = Mathf.Clamp01(value);
        float change = fill - previous;
        string action = change > 0f ? $"BarFill({change:0.###})" :
            change < 0f ? $"BarLoss({-change:0.###})" : "no call (unchanged)";

        Debug.Log($"[{name}] {label} changed: value={value:0.###}, previous={previous:0.###}, delta={change:0.###}; {action}.", this);

        if (bar != null)
        {
            if (change > 0f)
                bar.BarFill(change);
            else if (change < 0f)
                bar.BarLoss(-change);
        }
        else
        {
            Debug.LogError($"[{name}] {label} bar is not assigned.", this);
        }

        previous = fill;
    }

    private void OnDisable()
    {
        if (manaFill != null)
            manaFill.ValueChanged -= OnManaChanged;
        if (healthFill != null)
            healthFill.ValueChanged -= OnHealthChanged;
    }

    private void OnEnable()
    {
        if (manaFill != null)
            manaFill.ValueChanged += OnManaChanged;
        if (healthFill != null)
            healthFill.ValueChanged += OnHealthChanged;
    }

    private void OnDestroy()
    {
        OnDisable();
    }
}
