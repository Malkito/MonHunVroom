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
    // Shows if the ability button is being pressed
    // -------------------------------------------------------------
    private void UpdateAbilityInputs()
    {
        abilityOneIcon.color = GameInput.instance.getAbilityOneInput() ? activeColor : inactiveColor;
        abilityTwoIcon.color = GameInput.instance.getAbilityTwoInput() ? activeColor : inactiveColor;
        abilityThreeIcon.color = GameInput.instance.getAbilityThreeInput() ? activeColor : inactiveColor;
    }

    // -------------------------------------------------------------
    // Shows ability names from equipped upgrades
    // -------------------------------------------------------------
    private void UpdateAbilityNames()
    {
        // Slot 1
        if (playerUpgradeManager.equipped[0] != null &&
            playerUpgradeManager.equipped[0].definition != null)
            abilityOneName.text = playerUpgradeManager.equipped[0].definition.name;
        else
            abilityOneName.text = "None";

        // Slot 2
        if (playerUpgradeManager.equipped[1] != null &&
            playerUpgradeManager.equipped[1].definition != null)
            abilityTwoName.text = playerUpgradeManager.equipped[1].definition.name;
        else
            abilityTwoName.text = "None";

        // Slot 3
        if (playerUpgradeManager.equipped[2] != null &&
            playerUpgradeManager.equipped[2].definition != null)
            abilityThreeName.text = playerUpgradeManager.equipped[2].definition.name;
        else
            abilityThreeName.text = "None";
    }

    // -------------------------------------------------------------
    // Fades out ability name colors when they are on cooldown
    // -------------------------------------------------------------
    private void UpdateCooldownVisuals()
    {
        // Ability slot 1 cooldown
        abilityOneName.color = playerUpgradeManager.abilityOneCooldown > 0
            ? cooldownColor
            : readyColor;

        // Ability slot 2 cooldown
        abilityTwoName.color = playerUpgradeManager.abilityTwoCooldown > 0
            ? cooldownColor
            : readyColor;

        // Ability slot 3 cooldown
        abilityThreeName.color = playerUpgradeManager.abilityThreeCooldown > 0
            ? cooldownColor
            : readyColor;
    }
}
