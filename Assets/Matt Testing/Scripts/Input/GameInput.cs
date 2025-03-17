using UnityEngine;
using UnityEngine.InputSystem;
public class GameInput : MonoBehaviour
{
    private PlayerInputActions playerInputActions;
    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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

    private void Update()
    {


    }
}
