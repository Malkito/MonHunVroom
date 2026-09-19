using CupOHappiness.DeviceKeybindDisplay.Runtime;
using System.Text;
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

        [Header("Device Display")]
        [SerializeField, Tooltip("Optional device display database used to resolve wildcard bindings.")]
        private DeviceDisplayConfigurator deviceDisplayConfigurator;

        [Header("Binding Target")]
        [SerializeField, Tooltip("Optional action map name used to narrow the action lookup before falling back to action name only.")]
        private string actionMapName;
        [SerializeField, Tooltip("Action to resolve from the PlayerInput actions asset.")]
        private string actionName;

        [Header("Events")]
        [SerializeField, Tooltip("Invoked when a binding display string is resolved or refreshed.")]
        private UnityEvent<string> onBindingResolved;
        [SerializeField, Tooltip("Invoked when no binding display string can be resolved.")]
        private UnityEvent onBindingCleared;

        private string _bindingDisplay = string.Empty;

        public string BindingDisplay => _bindingDisplay;

        private void Awake()
        {
            SyncBindingTargetFromHierarchy();
        }

        private void OnEnable()
        {
            RefreshBindingDisplay();
        }

        public void RefreshBindingDisplay()
        {
            if (playerInput == null || playerInput.actions == null || string.IsNullOrEmpty(actionName))
            {
                ClearBindingDisplay();
                return;
            }

            InputAction action = null;
            if (!string.IsNullOrEmpty(actionMapName))
            {
                action = playerInput.actions.FindAction($"{actionMapName}/{actionName}", throwIfNotFound: false);
            }

            action ??= playerInput.actions.FindAction(actionName, throwIfNotFound: false);
            if (action == null || action.controls.Count == 0)
            {
                ClearBindingDisplay();
                return;
            }

            InputControl control = null;
            for (int i = 0; i < action.controls.Count; i++)
            {
                var candidate = action.controls[i];
                if (candidate?.device == null)
                {
                    continue;
                }

                if (IsPlayerDevice(playerInput, candidate.device))
                {
                    control = candidate;
                    break;
                }
            }

            control ??= action.controls[0];
            if (control == null)
            {
                ClearBindingDisplay();
                return;
            }

            int bindingIndex = action.GetBindingIndexForControl(control);
            if (bindingIndex < 0 || bindingIndex >= action.bindings.Count)
            {
                ClearBindingDisplay();
                return;
            }

            string bindingDisplay = GetBindingDisplayString(action, bindingIndex, control);
            if (string.IsNullOrEmpty(bindingDisplay))
            {
                ClearBindingDisplay();
                return;
            }

            if (string.IsNullOrEmpty(bindingDisplay))
            {
                ClearBindingDisplay();
                return;
            }

            _bindingDisplay = bindingDisplay;
            onBindingResolved?.Invoke(bindingDisplay);
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
            {
                return;
            }

            var target = display.GetBindingTarget();
            actionMapName = target.ActionMapName;
            actionName = target.ActionName;
        }

        private InputBindingDisplayBase FindNearestBindingDisplay()
        {
            var sameObject = GetComponent<InputBindingDisplayBase>();
            if (sameObject != null)
            {
                return sameObject;
            }

            var parentDisplay = GetComponentInParent<InputBindingDisplayBase>();
            if (parentDisplay != null)
            {
                return parentDisplay;
            }

            return GetComponentInChildren<InputBindingDisplayBase>(true);
        }


        private string GetBindingDisplayString(InputAction action, int bindingIndex, InputControl control)
        {
            if (action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count)
            {
                return string.Empty;
            }

            if (TryGetCompositeBindingDisplayString(action, bindingIndex, out string compositeDisplay))
            {
                return compositeDisplay;
            }

            var binding = action.bindings[bindingIndex];
            string effectivePath = binding.effectivePath;
            string originalPath = binding.path;
            string displayPath;

            if (IsSemanticBindingPath(effectivePath) || IsSemanticBindingPath(originalPath))
            {
                if (TryResolveWildcardDisplayString(effectivePath, originalPath, control, out string wildcardDisplay))
                {
                    return wildcardDisplay;
                }

                return GetControlDisplayString(control);
            }
            else if (!string.IsNullOrEmpty(effectivePath))
            {
                displayPath = effectivePath;
            }
            else if (!string.IsNullOrEmpty(originalPath))
            {
                displayPath = originalPath;
            }
            else
            {
                displayPath = control?.path;
            }

            if (string.IsNullOrEmpty(displayPath))
            {
                return string.Empty;
            }

            return InputControlPath.ToHumanReadableString(
                displayPath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);
        }

        private static bool TryGetCompositeBindingDisplayString(InputAction action, int bindingIndex, out string compositeDisplay)
        {
            compositeDisplay = null;
            if (action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count)
            {
                return false;
            }

            int compositeBindingIndex = bindingIndex;
            var binding = action.bindings[bindingIndex];
            if (binding.isPartOfComposite)
            {
                compositeBindingIndex = FindOwningCompositeBindingIndex(action, bindingIndex);
                if (compositeBindingIndex < 0)
                {
                    return false;
                }

                binding = action.bindings[compositeBindingIndex];
            }

            if (!binding.isComposite)
            {
                return false;
            }

            if (TryBuildCompositePartsDisplayString(action, compositeBindingIndex, out compositeDisplay))
            {
                return true;
            }

            compositeDisplay = action.GetBindingDisplayString(compositeBindingIndex);
            return !string.IsNullOrEmpty(compositeDisplay);
        }

        private static int FindOwningCompositeBindingIndex(InputAction action, int partBindingIndex)
        {
            if (action == null || partBindingIndex <= 0 || partBindingIndex >= action.bindings.Count)
            {
                return -1;
            }

            for (int i = partBindingIndex - 1; i >= 0; i--)
            {
                var binding = action.bindings[i];
                if (binding.isComposite)
                {
                    return i;
                }

                if (!binding.isPartOfComposite)
                {
                    break;
                }
            }

            return -1;
        }

        private static bool TryBuildCompositePartsDisplayString(InputAction action, int compositeBindingIndex, out string compositeDisplay)
        {
            compositeDisplay = null;
            if (action == null || compositeBindingIndex < 0 || compositeBindingIndex >= action.bindings.Count)
            {
                return false;
            }

            // This reader outputs a plain display string, so we keep the fallback simple and user-facing.
            var orderedParts = new System.Collections.Generic.List<string>(4);
            for (int i = compositeBindingIndex + 1; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (!binding.isPartOfComposite)
                {
                    break;
                }

                string partDisplay = action.GetBindingDisplayString(i);
                if (!string.IsNullOrEmpty(partDisplay) && !orderedParts.Contains(partDisplay))
                {
                    orderedParts.Add(partDisplay);
                }
            }

            if (orderedParts.Count == 0)
            {
                return false;
            }

            if (orderedParts.Count == 4 &&
                string.Equals(orderedParts[0], "W", System.StringComparison.OrdinalIgnoreCase) &&
                string.Equals(orderedParts[1], "A", System.StringComparison.OrdinalIgnoreCase) &&
                string.Equals(orderedParts[2], "S", System.StringComparison.OrdinalIgnoreCase) &&
                string.Equals(orderedParts[3], "D", System.StringComparison.OrdinalIgnoreCase))
            {
                compositeDisplay = "WASD";
                return true;
            }

            compositeDisplay = string.Join("/", orderedParts);
            return true;
        }

        private bool TryResolveWildcardDisplayString(string effectivePath, string originalPath, InputControl control, out string display)
        {
            display = null;
            if (deviceDisplayConfigurator == null || control?.device == null)
            {
                return false;
            }

            string semanticPath = IsSemanticBindingPath(effectivePath) ? effectivePath : originalPath;
            int start = semanticPath?.IndexOf('{') ?? -1;
            int end = semanticPath?.IndexOf('}', start + 1) ?? -1;
            if (start < 0 || end <= start)
            {
                return false;
            }

            string wildcard = semanticPath.Substring(start + 1, end - start - 1);
            return deviceDisplayConfigurator.TryResolveWildcardForDevice(control.device.layout, wildcard, out display);
        }

        private static bool IsSemanticBindingPath(string path)
        {
            return !string.IsNullOrEmpty(path) && path.Contains("{", System.StringComparison.Ordinal);
        }

        private static string GetControlDisplayString(InputControl control)
        {
            if (control == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(control.path))
            {
                string humanReadable = InputControlPath.ToHumanReadableString(
                    control.path,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);
                return NormalizeUnityDisplayString(humanReadable);
            }

            if (!string.IsNullOrEmpty(control.displayName))
            {
                return NormalizeUnityDisplayString(control.displayName);
            }

            if (!string.IsNullOrEmpty(control.shortDisplayName))
            {
                return NormalizeUnityDisplayString(control.shortDisplayName);
            }

            return string.Empty;
        }

        private static string NormalizeUnityDisplayString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (value.IndexOf(' ') >= 0)
            {
                return CapitalizeWords(value);
            }

            var builder = new StringBuilder(value.Length + 4);
            for (int i = 0; i < value.Length; i++)
            {
                char current = value[i];
                if (i > 0 && char.IsUpper(current) && char.IsLetter(value[i - 1]))
                {
                    builder.Append(' ');
                }

                if (i == 0 || (i > 0 && builder[builder.Length - 1] == ' '))
                {
                    builder.Append(char.ToUpperInvariant(current));
                }
                else
                {
                    builder.Append(current);
                }
            }

            return builder.ToString();
        }

        private static string CapitalizeWords(string value)
        {
            var builder = new StringBuilder(value.Length);
            bool capitalizeNext = true;
            for (int i = 0; i < value.Length; i++)
            {
                char current = value[i];
                if (char.IsWhiteSpace(current))
                {
                    capitalizeNext = true;
                    builder.Append(current);
                    continue;
                }

                builder.Append(capitalizeNext ? char.ToUpperInvariant(current) : current);
                capitalizeNext = false;
            }

            return builder.ToString();
        }

        private static bool IsPlayerDevice(PlayerInput playerInput, InputDevice device)
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
