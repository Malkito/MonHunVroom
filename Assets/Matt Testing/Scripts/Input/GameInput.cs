using UnityEngine;
using UnityEngine.InputSystem;
public class GameInput : MonoBehaviour
{
    private PlayerInputActions playerInputActions;

    public static GameInput instance { get; private set; }
    public InputActionAsset Actions => playerInputActions?.asset;
    public InputAction OptionsAction { get; private set; }

    private const string BindingOverridesKey = "MonHunVroom.InputBindingOverrides";

    private void Awake()
    {
        instance = this;
        playerInputActions = new PlayerInputActions();
        string overrides = PlayerPrefs.GetString(BindingOverridesKey, string.Empty);
        if (!string.IsNullOrEmpty(overrides))
        {
            playerInputActions.asset.LoadBindingOverridesFromJson(overrides);
        }
        var globalMap = playerInputActions.asset.AddActionMap("Global");
        OptionsAction = globalMap.AddAction("Options", InputActionType.Button);
        OptionsAction.AddBinding("<Keyboard>/escape", groups: "Global");
        OptionsAction.AddBinding("<Gamepad>/start", groups: "Global");
        playerInputActions.Player.Enable();
        globalMap.Enable();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
        playerInputActions?.Dispose();
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
