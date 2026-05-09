using UnityEngine;
using UnityEngine.InputSystem;
public class GameInput : MonoBehaviour
{
    private PlayerInputActions playerInputActions;

    public static GameInput instance { get; private set; }

    private void Awake()
    {
        instance = this;
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }


    public Vector2 getMovementInputNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        inputVector = inputVector.normalized;
        return inputVector;

    }
    public bool getAttackInput()
    {
        bool attackingInputPressed = playerInputActions.Player.Attack.IsPressed();
        return attackingInputPressed;
    }

    public bool getAltAttackInput()
    {
        bool altAttackingInputPressed = playerInputActions.Player.AltAttack.IsPressed();
        return altAttackingInputPressed;
    }
    
    public bool getAbilityOneInput()
    {
        bool abilityOneInputPressed = playerInputActions.Player.Ability1.IsPressed();
        return abilityOneInputPressed;
    }

    public bool getAbilityTwoInput()
    {
        bool abilityTwoInputPressed = playerInputActions.Player.Ability2.IsPressed();
        return abilityTwoInputPressed;
    }

    public bool getAbilityThreeInput()
    {
        bool abilityThreeInputPressed = playerInputActions.Player.Ability3.IsPressed();
        return abilityThreeInputPressed;
    }
    

    public bool getSprintInput()
    {
        bool abilityThreeInputPressed = playerInputActions.Player.Sprint.IsPressed();
        return abilityThreeInputPressed;
    }

    public bool getJumpInput()
    {
        bool jumpInputPressed = playerInputActions.Player.Jump.IsPressed();
        return jumpInputPressed;

    }

    public bool getUnstickInput()
    {
        bool UnstickInputPressed = playerInputActions.Player.Unstick.IsPressed();
        return UnstickInputPressed;

    }


    public bool getSwapMovementInput()
    {
        bool SwapMovementInputPressed = playerInputActions.Player.SwapMovement.IsPressed();
        return SwapMovementInputPressed;

    }

    public bool getSelectUpgradeOneInput()
    {
        bool SelectUpgradeOneInputPressed = playerInputActions.Player.SelectUpgradeOne.IsPressed();
        return SelectUpgradeOneInputPressed;

    }

    public bool getSelectUpgradeTwoInput()
    {
        bool SelectUpgradeTwoInputPressed = playerInputActions.Player.SelectUpgradeTwo.IsPressed();
        return SelectUpgradeTwoInputPressed;

    }

    public bool getSelectUpgradeThreeInput()
    {
        bool SelectUpgradeThreeInputPressed = playerInputActions.Player.SelectUpgradeThree.IsPressed();
        return SelectUpgradeThreeInputPressed;

    }

    public void enableOrDisablePlayerAction(bool enableOrDisable)
    {
        if (enableOrDisable)
        {
            playerInputActions.Player.Enable();
        }
        else
        {
            playerInputActions.Player.Disable();
        }
    }
}
