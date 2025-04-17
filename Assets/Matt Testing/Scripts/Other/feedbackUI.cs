using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class feedbackUI : MonoBehaviour
{
    [SerializeField] private Image abilityOne;
    [SerializeField] private Image abilityTwo;
    [SerializeField] private Image abilityThree;

    [SerializeField] private TMP_Text abilityOneName;
    [SerializeField] private TMP_Text abilityTwoName;
    [SerializeField] private TMP_Text abilityThreeName;


    [SerializeField] private playerUpgradeManager playerUpgradeManager;


    void Update()
    {
        if (GameInput.instance.getAbilityOneInput())
        {
            abilityOne.color = Color.green;
        }
        else
        {
            abilityOne.color = Color.red;
        }

        if (GameInput.instance.getAbilityTwoInput())
        {
            abilityTwo.color = Color.green;
        }
        else
        {
            abilityTwo.color = Color.red;
        }

        if (GameInput.instance.getAbilityThreeInput())
        {
            abilityThree.color = Color.green;
        }
        else
        {
            abilityThree.color = Color.red;
        }

        if (playerUpgradeManager.currentUpgrades[0] != null) abilityOneName.text = playerUpgradeManager.currentUpgrades[0].name;
        if (playerUpgradeManager.currentUpgrades[1] != null) abilityTwoName.text = playerUpgradeManager.currentUpgrades[1].name;
        if (playerUpgradeManager.currentUpgrades[2] != null) abilityThreeName.text = playerUpgradeManager.currentUpgrades[2].name;

        if (playerUpgradeManager.abilityOneCooldown > 0) {
            abilityOneName.color = new Color(1, 1, 1, 0.5f);
        }
        else
        {
            abilityOneName.color = new Color(1, 1, 1, 1);
        }

        if (playerUpgradeManager.abilityTwoCooldown > 0)
        {
            abilityTwoName.color = new Color(1, 1, 1, 0.5f);
        }
        else
        {
            abilityTwoName.color = new Color(1, 1, 1, 1);
        }

        if (playerUpgradeManager.abilityThreeCooldown > 0)
        {
            abilityThreeName.color = new Color(1, 1, 1, 0.5f);
        }
        else
        {
            abilityThreeName.color = new Color(1, 1, 1, 1);
        }
    }
}
