using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

namespace CupOHappiness.DeviceKeybindDisplay.Integrations.InputSystem
{
    [MovedFrom(true, "CupOHappiness.DeviceKeybindDisplay.Integrations.UnityInputActions", "CupOHappiness.DeviceKeybindDisplay.UnityInputActions", "UnityInputActionBindingDisplay")]
    public class MultiplayerInputBindingDisplay : InputBindingDisplayBase
    {
        [Header("Input Source")]
        [SerializeField, Tooltip("PlayerInput that owns this multiplayer player's actions and paired devices.")]
        private PlayerInput playerInput;

        protected override InputActionAsset GetInputActions() => playerInput?.actions;

        protected override InputDevice GetActiveDevice() =>
            InputDeviceSelectionUtility.FindActiveDevice(playerInput?.actions, IsPlayerDevice);

        private bool IsPlayerDevice(InputDevice device)
        {
            if (playerInput == null || device == null)
            {
                return false;
            }

            for (int i = 0; i < playerInput.devices.Count; i++)
            {
                if (playerInput.devices[i] == device)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
