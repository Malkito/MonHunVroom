using CupOHappiness.DeviceKeybindDisplay.Runtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

namespace CupOHappiness.DeviceKeybindDisplay.Integrations.InputSystem
{
    /// <summary>
    /// Small reusable helper that resolves a human-readable binding name from a Unity PlayerInput action.
    /// </summary>
    [MovedFrom(true, "CupOHappiness.DeviceKeybindDisplay.Integrations.UnityInputActions", "CupOHappiness.DeviceKeybindDisplay.UnityInputActions", "UnityInputActionBindingReader")]
    public class InputBindingReader : MonoBehaviour
    {
        [Header("Input Source")]
        [SerializeField, Tooltip("PlayerInput that owns the live Unity Input System actions for this binding reader.")]
        private PlayerInput playerInput;

        [Header("Binding Target")]
        [SerializeField, Tooltip("Optional action map name used to narrow the action lookup before falling back to action name only.")]
        private string actionMapName;
        [SerializeField, Tooltip("Action to resolve from the PlayerInput actions asset.")]
        private string actionName;

        [Header("Composite Display")]
        [SerializeField] private CompositeBindingDisplayMode compositeBindingMode = CompositeBindingDisplayMode.SeparateIcons;
        [SerializeField] private DeviceDisplayConfigurator deviceDisplayConfigurator;

        [Header("Events")]
        [SerializeField, Tooltip("Invoked when a binding display string is resolved or refreshed.")]
        private UnityEvent<string> onBindingResolved;
        [SerializeField, Tooltip("Invoked when no binding display string can be resolved.")]
        private UnityEvent onBindingCleared;

        private string _bindingDisplay = string.Empty;
        private int _bindingIndex = -1;
        private InputDevice _lastDevice;
        private bool _refreshRequested;

        public string BindingDisplay => _bindingDisplay;

        private void OnEnable()
        {
            UnityEngine.InputSystem.InputSystem.onActionChange += OnActionChange;
            UnityEngine.InputSystem.InputSystem.onDeviceChange += OnDeviceChange;
            UnityEngine.InputSystem.InputSystem.onAfterUpdate += OnAfterUpdate;
            RefreshBindingDisplay();
        }

        private void OnDisable()
        {
            UnityEngine.InputSystem.InputSystem.onActionChange -= OnActionChange;
            UnityEngine.InputSystem.InputSystem.onDeviceChange -= OnDeviceChange;
            UnityEngine.InputSystem.InputSystem.onAfterUpdate -= OnAfterUpdate;
        }

        private void OnActionChange(object changed, InputActionChange change)
        {
            if (change == InputActionChange.BoundControlsChanged &&
                BindingDisplayResolver.OwnsChange(changed, playerInput?.actions))
                _refreshRequested = true;
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            _refreshRequested = true;
        }

        private void OnAfterUpdate()
        {
            var device = InputDeviceSelectionUtility.FindActiveDevice(playerInput?.actions, IsPlayerDevice);
            if (_refreshRequested || device != _lastDevice)
                RefreshBindingDisplay();
        }

        public void RefreshBindingDisplay()
        {
            _refreshRequested = false;
            SyncBindingTargetFromHierarchy();
            _lastDevice = InputDeviceSelectionUtility.FindActiveDevice(playerInput?.actions, IsPlayerDevice);
            var action = BindingDisplayResolver.FindAction(playerInput?.actions, actionMapName, actionName);
            if (!BindingDisplayResolver.TryResolve(action, _bindingIndex, _lastDevice,
                    IsPlayerDevice, out var presentation, compositeBindingMode, deviceDisplayConfigurator))
            {
                ClearBindingDisplay();
                return;
            }

            _bindingDisplay = presentation.Text;
            onBindingResolved?.Invoke(_bindingDisplay);
        }

        public void ClearBindingDisplay()
        {
            _bindingDisplay = string.Empty;
            onBindingCleared?.Invoke();
        }

        public void SyncBindingTargetFromHierarchy()
        {
            var display = FindNearestBindingDisplay();
            if (display == null)
                return;

            var target = display.GetBindingTarget();
            actionMapName = target.ActionMapName;
            actionName = target.ActionName;
            _bindingIndex = target.BindingIndex;
            compositeBindingMode = target.CompositeMode;
            deviceDisplayConfigurator = target.Configurator;
        }

        private InputBindingDisplayBase FindNearestBindingDisplay()
        {
            var sameObject = GetComponent<InputBindingDisplayBase>();
            if (sameObject != null)
                return sameObject;
            var parentDisplay = GetComponentInParent<InputBindingDisplayBase>();
            return parentDisplay != null ? parentDisplay : GetComponentInChildren<InputBindingDisplayBase>(true);
        }

        private bool IsPlayerDevice(InputDevice device)
        {
            if (playerInput == null || device == null)
                return false;
            foreach (var paired in playerInput.devices)
                if (paired == device)
                    return true;
            return false;
        }
    }
}
