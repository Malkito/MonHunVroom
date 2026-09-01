using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class feedbackUI : MonoBehaviour
{

    /// <summary>
    /// 
    /// This script handles the representation of the feedback of the abilites.
    /// - Changes The name of the equipped abilites
    /// - chenages the text color based off the cooldown of said abilites: White when ready, slightly transparent when on cooldown
    /// - Flashes the ability icons from red to green when the input is being pressed
    /// </summary>



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

    private void UpdateAbilityInputs()     ///Changes The name of the equipped abilites
    {
        abilityOneIcon.color = GameInput.instance.getAbilityOneInput() ? activeColor : inactiveColor;
        abilityTwoIcon.color = GameInput.instance.getAbilityTwoInput() ? activeColor : inactiveColor;
        abilityThreeIcon.color = GameInput.instance.getAbilityThreeInput() ? activeColor : inactiveColor;
    }

    private void UpdateAbilityNames()///Changes The name of the equipped abilites
    {
        var db = UpgradeDatabase.Instance;
        if (db == null) return;

        SetSlotUI(0, abilityOneName);
        SetSlotUI(1, abilityTwoName);
        SetSlotUI(2, abilityThreeName);
    }

    private void SetSlotUI(int slot, TMP_Text label)
    {
        if (playerUpgradeManager.equippedPowerUps[slot].logicInstance == null)
        {
            label.text = "None";
            return;
        }

        int id = playerUpgradeManager.equippedPowerUps[slot].upgradeID;

        var def = UpgradeDatabase.Instance.Get(id);

        if (def != null)
            label.text = def.name;
        else
            label.text = "Unknown";
    }

    private void UpdateCooldownVisuals() /// changes the text color based off the cooldown: White when ready, slightly transparent when on cooldown

    {
        abilityOneName.color = playerUpgradeManager.abilityOneCooldown > 0 ? cooldownColor: readyColor;

        abilityTwoName.color = playerUpgradeManager.abilityTwoCooldown > 0 ? cooldownColor: readyColor;

        abilityThreeName.color = playerUpgradeManager.abilityThreeCooldown > 0 ? cooldownColor : readyColor;
    }
}