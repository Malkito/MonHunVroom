using CupOHappiness.DeviceKeybindDisplay.Runtime;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CupOHappiness.DeviceKeybindDisplay.Integrations.InputSystem
{
    public abstract class InputBindingDisplayBase : MonoBehaviour
    {
        protected abstract InputActionAsset GetInputActions();
        protected abstract InputDevice GetActiveDevice();

        [System.Serializable]
        public struct BindingTarget
        {
            public string ActionMapName;
            public string ActionName;
        }

        [SerializeField, Tooltip("Device display database used to resolve icons and device-specific presentation.")]
        private DeviceDisplayConfigurator deviceDisplayConfigurator;

        [Header("Display References")]
        [SerializeField, Tooltip("Single image used as either the resolved binding icon or the binding-name background when no icon is available.")]
        private Image displayImage;
        [SerializeField, Tooltip("Optional TMP text used to display the human-readable binding name such as Esc or Space.")]
        private TMP_Text bindingNameText;

        [Header("Display Options")]
        [SerializeField, Tooltip("If enabled, shows the human-readable binding name in the TMP text field.")]
        private bool displayBindingName;
        [SerializeField, Tooltip("Extra breathing room added after fitting the text inside the sprite's 9-slice center area.")]
        private Vector2 bindingNameExtraMargin = new Vector2(8f, 4f);

        [Header("Binding Target")]
        [SerializeField, Tooltip("Optional action map name used to narrow the action lookup before falling back to action name only.")]
        private string actionMapName;
        [SerializeField, Tooltip("Action to resolve from the PlayerInput actions asset.")]
        private string actionName;

        private Sprite _defaultDisplaySprite;
        private bool _cachedDefaultDisplaySprite;

        private void OnEnable()
        {
            CacheDefaultDisplaySprite();
            RefreshGraphic();
        }

        public BindingTarget GetBindingTarget()
        {
            return new BindingTarget
            {
                ActionMapName = actionMapName,
                ActionName = actionName
            };
        }

        public void RefreshGraphic()
        {
            var inputActions = GetInputActions();
            var activeDevice = GetActiveDevice();
            if (inputActions == null || deviceDisplayConfigurator == null)
            {
                return;
            }

            if (!TryResolveAction(inputActions, actionMapName, actionName, out var action))
            {
                ApplyDisplay(null, null);
                return;
            }

            string bindingDisplay = TryResolveBindingDisplayString(activeDevice, action, out var resolvedBindingDisplay)
                ? resolvedBindingDisplay
                : null;

            Sprite sprite = string.IsNullOrEmpty(bindingDisplay)
                ? null
                : deviceDisplayConfigurator.GetDeviceBindingIcon(activeDevice, bindingDisplay);

            ApplyDisplay(sprite, bindingDisplay);
        }

        private void ApplyDisplay(Sprite sprite, string bindingDisplay)
        {
            bool hasIcon = sprite != null;
            bool showBindingName = displayBindingName && !string.IsNullOrEmpty(bindingDisplay) && !hasIcon;

            if (displayImage != null)
            {
                CacheDefaultDisplaySprite();
                displayImage.sprite = hasIcon ? sprite : _defaultDisplaySprite;
                displayImage.enabled = hasIcon || showBindingName;
            }

            if (bindingNameText != null)
            {
                bindingNameText.text = bindingDisplay ?? string.Empty;
                bindingNameText.enabled = showBindingName;
            }

            // Keep the authored icon RectTransform size. Automatic image scaling is disabled.
        }

        private void UpdateDisplayImageSize(bool showBindingName, bool hasIcon)
        {
            if (displayImage == null)
            {
                return;
            }

            if (hasIcon || !showBindingName || bindingNameText == null)
            {
                return;
            }

            if (displayImage.transform is not RectTransform imageRect)
            {
                return;
            }

            bindingNameText.ForceMeshUpdate();
            Vector2 textSize = GetMeasuredTextSize(bindingNameText);
            Vector4 borderPadding = GetNineSliceBorderPadding(displayImage);
            float targetWidth = textSize.x + borderPadding.x + borderPadding.z + bindingNameExtraMargin.x;
            float targetHeight = textSize.y + borderPadding.y + borderPadding.w + bindingNameExtraMargin.y;
            float finalWidth = Mathf.Max(imageRect.rect.width, targetWidth);
            float finalHeight = Mathf.Max(imageRect.rect.height, targetHeight);

            imageRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                finalWidth);
            imageRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                finalHeight);
        }

        private void CacheDefaultDisplaySprite()
        {
            if (displayImage == null || _cachedDefaultDisplaySprite)
            {
                return;
            }

            _defaultDisplaySprite = displayImage.sprite;
            _cachedDefaultDisplaySprite = true;
        }

        private static Vector4 GetNineSliceBorderPadding(Image image)
        {
            if (image == null || image.sprite == null)
            {
                return Vector4.zero;
            }

            Sprite sprite = image.sprite;
            Vector4 border = sprite.border;
            float canvasScaleFactor = image.canvas != null && image.canvas.scaleFactor > 0f
                ? image.canvas.scaleFactor
                : 1f;
            Vector4 scaledBorder = new Vector4(
                border.x / canvasScaleFactor,
                border.y / canvasScaleFactor,
                border.z / canvasScaleFactor,
                border.w / canvasScaleFactor);

            return scaledBorder;
        }

        private static Vector2 GetMeasuredTextSize(TMP_Text text)
        {
            if (text == null)
            {
                return Vector2.zero;
            }

            Vector2 rendered = text.GetRenderedValues(true);
            Vector2 bounds = text.textBounds.size;
            Vector2 preferred = text.GetPreferredValues(text.text);
            return new Vector2(
                Mathf.Max(rendered.x, bounds.x, preferred.x),
                Mathf.Max(rendered.y, bounds.y, preferred.y));
        }

        private static bool TryResolveAction(InputActionAsset inputActions, string actionMapName, string actionName, out InputAction action)
        {
            action = null;
            if (inputActions == null || string.IsNullOrEmpty(actionName))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(actionMapName))
            {
                action = inputActions.FindAction($"{actionMapName}/{actionName}", throwIfNotFound: false);
            }

            action ??= inputActions.FindAction(actionName, throwIfNotFound: false);
            return action != null;
        }

        private bool TryResolveBindingDisplayString(InputDevice preferredDevice, InputAction action, out string bindingDisplay)
        {
            bindingDisplay = null;
            if (action == null || action.controls.Count == 0)
            {
                return false;
            }

            InputControl control = null;
            for (int i = 0; i < action.controls.Count; i++)
            {
                var candidate = action.controls[i];
                if (candidate?.device == null)
                {
                    continue;
                }

                if (candidate.device == preferredDevice)
                {
                    control = candidate;
                    break;
                }
            }

            control ??= action.controls[0];
            if (control == null)
            {
                return false;
            }

            int bindingIndex = action.GetBindingIndexForControl(control);
            if (bindingIndex < 0 || bindingIndex >= action.bindings.Count)
            {
                return false;
            }

            if (TryGetCompositeBindingDisplayName(action, bindingIndex, out string compositeDisplay))
            {
                bindingDisplay = compositeDisplay;
                return true;
            }

            if (IsSemanticBindingPath(action.bindings[bindingIndex].effectivePath) || IsSemanticBindingPath(action.bindings[bindingIndex].path))
            {
                if (!TryResolveWildcardDisplayString(action.bindings[bindingIndex].effectivePath, action.bindings[bindingIndex].path, control, out bindingDisplay))
                {
                    bindingDisplay = GetControlDisplayString(control);
                }

                return !string.IsNullOrEmpty(bindingDisplay);
            }

            string displayPath = GetDisplayPath(action, bindingIndex, control);
            if (string.IsNullOrEmpty(displayPath))
            {
                return false;
            }

            bindingDisplay = InputControlPath.ToHumanReadableString(
                displayPath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);

            return !string.IsNullOrEmpty(bindingDisplay);
        }

        private static bool TryGetCompositeBindingDisplayName(InputAction action, int bindingIndex, out string compositeDisplay)
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

            var partDisplays = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

            for (int i = compositeBindingIndex + 1; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (!binding.isPartOfComposite)
                {
                    break;
                }

                string partName = binding.name;
                string partDisplay = action.GetBindingDisplayString(i);
                if (string.IsNullOrEmpty(partDisplay))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(partName) && !partDisplays.ContainsKey(partName))
                {
                    partDisplays[partName] = partDisplay;
                }
            }

            if (partDisplays.Count == 0)
            {
                return false;
            }

            if (TryBuildWASDDisplay(partDisplays, out compositeDisplay))
            {
                return true;
            }

            var orderedParts = new List<string>(4);
            AddPartIfPresent("up", partDisplays, orderedParts);
            AddPartIfPresent("left", partDisplays, orderedParts);
            AddPartIfPresent("down", partDisplays, orderedParts);
            AddPartIfPresent("right", partDisplays, orderedParts);

            foreach (var pair in partDisplays)
            {
                if (!orderedParts.Contains(pair.Value))
                {
                    orderedParts.Add(pair.Value);
                }
            }

            compositeDisplay = string.Join("/", orderedParts);
            return !string.IsNullOrEmpty(compositeDisplay);
        }

        private static bool TryBuildWASDDisplay(Dictionary<string, string> partDisplays, out string compositeDisplay)
        {
            compositeDisplay = null;
            if (!partDisplays.TryGetValue("up", out string up) ||
                !partDisplays.TryGetValue("left", out string left) ||
                !partDisplays.TryGetValue("down", out string down) ||
                !partDisplays.TryGetValue("right", out string right))
            {
                return false;
            }

            if (string.Equals(up, "W", System.StringComparison.OrdinalIgnoreCase) &&
                string.Equals(left, "A", System.StringComparison.OrdinalIgnoreCase) &&
                string.Equals(down, "S", System.StringComparison.OrdinalIgnoreCase) &&
                string.Equals(right, "D", System.StringComparison.OrdinalIgnoreCase))
            {
                compositeDisplay = "WASD";
                return true;
            }

            return false;
        }

        private static void AddPartIfPresent(string partName, Dictionary<string, string> partDisplays, List<string> orderedParts)
        {
            if (partDisplays.TryGetValue(partName, out string value))
            {
                orderedParts.Add(value);
            }
        }

        private static string GetDisplayPath(InputAction action, int bindingIndex, InputControl control)
        {
            if (action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count)
            {
                return control?.path;
            }

            var binding = action.bindings[bindingIndex];
            string effectivePath = binding.effectivePath;
            string originalPath = binding.path;

            if (IsSemanticBindingPath(effectivePath) || IsSemanticBindingPath(originalPath))
            {
                return control?.path;
            }

            if (!string.IsNullOrEmpty(effectivePath))
            {
                return effectivePath;
            }

            if (!string.IsNullOrEmpty(originalPath))
            {
                return originalPath;
            }

            return control?.path;
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

    }
}
