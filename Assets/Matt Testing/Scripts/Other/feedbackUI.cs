using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class feedbackUI : MonoBehaviour
{
    [Header("Ability Icons")]
    [SerializeField] private Image abilityOneIcon;
    [SerializeField] private Image abilityTwoIcon;
    [SerializeField] private Image abilityThreeIcon;

    [Header("Ability Text Labels")]
    [SerializeField] private TMP_Text abilityOneName;
    [SerializeField] private TMP_Text abilityTwoName;
    [SerializeField] private TMP_Text abilityThreeName;

    [Header("References")]
    [SerializeField] private playerUpgradeManager playerUpgradeManager;

    private readonly Color activeColor = Color.green;
    private readonly Color inactiveColor = Color.red;

    private readonly Color cooldownColor = new Color(1, 1, 1, 0.5f);
    private readonly Color readyColor = new Color(1, 1, 1, 1);

    void Update()
    {
        if (playerUpgradeManager == null)
            return;

        UpdateAbilityInputs();
        UpdateAbilityNames();
        UpdateCooldownVisuals();
    }

    // -------------------------------------------------------------
    // INPUT FEEDBACK
    // -------------------------------------------------------------
    private void UpdateAbilityInputs()
    {
        abilityOneIcon.color = GameInput.instance.getAbilityOneInput() ? activeColor : inactiveColor;
        abilityTwoIcon.color = GameInput.instance.getAbilityTwoInput() ? activeColor : inactiveColor;
        abilityThreeIcon.color = GameInput.instance.getAbilityThreeInput() ? activeColor : inactiveColor;
    }

    // -------------------------------------------------------------
    // NAMES (NOW USING UpgradeDatabase + INT ID)
    // -------------------------------------------------------------
    private void UpdateAbilityNames()
    {
        var db = UpgradeDatabase.Instance;
        if (db == null) return;

        // Slot 1
        SetSlotUI(0, abilityOneName);
        // Slot 2
        SetSlotUI(1, abilityTwoName);
        // Slot 3
        SetSlotUI(2, abilityThreeName);
    }

    private void SetSlotUI(int slot, TMP_Text label)
    {
        if (playerUpgradeManager.equipped[slot].logicInstance == null)
        {
            label.text = "None";
            return;
        }

        int id = playerUpgradeManager.equipped[slot].upgradeID;

        var def = UpgradeDatabase.Instance.Get(id);

        if (def != null)
            label.text = def.name;
        else
            label.text = "Unknown";
    }

    // -------------------------------------------------------------
    // COOLDOWN VISUALS (UNCHANGED LOGIC)
    // -------------------------------------------------------------
    private void UpdateCooldownVisuals()
    {
        abilityOneName.color = playerUpgradeManager.abilityOneCooldown > 0
            ? cooldownColor
            : readyColor;

        abilityTwoName.color = playerUpgradeManager.abilityTwoCooldown > 0
            ? cooldownColor
            : readyColor;

        abilityThreeName.color = playerUpgradeManager.abilityThreeCooldown > 0
            ? cooldownColor
            : readyColor;
    }
}