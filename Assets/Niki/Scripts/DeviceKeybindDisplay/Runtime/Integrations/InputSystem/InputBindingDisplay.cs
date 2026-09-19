using UnityEngine;
using UnityEngine.InputSystem;

namespace CupOHappiness.DeviceKeybindDisplay.Integrations.InputSystem
{
    public class InputBindingDisplay : InputBindingDisplayBase
    {
        [Header("Input Source")]
        [SerializeField, Tooltip("Input actions asset used by the single-player UI.")]
        private InputActionAsset inputActions;

        protected override InputActionAsset GetInputActions() => inputActions;

        protected override InputDevice GetActiveDevice() =>
            InputDeviceSelectionUtility.FindActiveDevice(inputActions);
    }
}
