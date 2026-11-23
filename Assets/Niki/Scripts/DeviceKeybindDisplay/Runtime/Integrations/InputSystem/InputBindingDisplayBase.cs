using CupOHappiness.DeviceKeybindDisplay.Runtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using InputSystemAPI = UnityEngine.InputSystem.InputSystem;
using UnityEngine.UI;

namespace CupOHappiness.DeviceKeybindDisplay.Integrations.InputSystem
{
    public abstract class InputBindingDisplayBase : MonoBehaviour
    {
        protected abstract InputActionAsset GetInputActions();
        protected abstract InputDevice GetActiveDevice();
        protected virtual System.Func<InputDevice, bool> DeviceFilter => null;

        [System.Serializable]
        public struct BindingTarget
        {
            public string ActionMapName;
            public string ActionName;
            public int BindingIndex;
            public CompositeBindingDisplayMode CompositeMode;
            public DeviceDisplayConfigurator Configurator;
        }

        [SerializeField, Tooltip("Device display database used to resolve icons and device-specific presentation.")]
        private DeviceDisplayConfigurator deviceDisplayConfigurator;

        [Header("Binding Items")]
        [SerializeField, Tooltip("Image and text item prefab instantiated for each key or separator.")]
        private BindingDisplayItem bindingItemPrefab;
        [SerializeField, Tooltip("Layout container for generated items. Defaults to this transform.")]
        private RectTransform bindingItemsParent;
        [SerializeField, Tooltip("First In List selects the first alternative per part, not the first required key.")]
        private CompositeBindingDisplayMode compositeBindingMode = CompositeBindingDisplayMode.SeparateIcons;

        private readonly List<BindingDisplayItem> _items = new();
        private bool _hiddenLegacyImage;

        [Header("Legacy Display References")]
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

        private int runtimeBindingIndex = -1;

        private Sprite _defaultDisplaySprite;
        private Image.Type _defaultDisplayImageType;
        private bool _defaultDisplayImagePreserveAspect;
        private bool _cachedDefaultDisplaySprite;
        private InputDevice _lastActiveDevice;

        private void OnEnable()
        {
            CacheDefaultDisplaySprite();
            _lastActiveDevice = GetActiveDevice();
            InputSystemAPI.onEvent += OnInputEvent;
            InputSystemAPI.onAfterUpdate += RefreshIfActiveDeviceChanged;
            InputSystemAPI.onDeviceChange += OnDeviceChange;
            InputSystemAPI.onActionChange += OnActionChange;
            RefreshGraphic();
        }

        private void OnDisable()
        {
            InputSystemAPI.onEvent -= OnInputEvent;
            InputSystemAPI.onAfterUpdate -= RefreshIfActiveDeviceChanged;
            InputSystemAPI.onDeviceChange -= OnDeviceChange;
            InputSystemAPI.onActionChange -= OnActionChange;
            _lastActiveDevice = null;
            foreach (var item in _items)
                if (item != null)
                    Destroy(item.gameObject);
            _items.Clear();
        }

        private void OnValidate()
        {
            _refreshRequested = true;
        }

        private bool _inputEventReceived;
        private bool _refreshRequested;

        private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            _inputEventReceived = true;
        }

        private void RefreshIfActiveDeviceChanged()
        {
            if (!_inputEventReceived && !_refreshRequested)
                return;

            _inputEventReceived = false;
            var activeDevice = GetActiveDevice();
            if (activeDevice == _lastActiveDevice && !_refreshRequested)
                return;

            _lastActiveDevice = activeDevice;
            _refreshRequested = false;
            RefreshGraphic();
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change != InputDeviceChange.Added && change != InputDeviceChange.Removed &&
                change != InputDeviceChange.Disconnected && change != InputDeviceChange.Reconnected &&
                change != InputDeviceChange.Enabled && change != InputDeviceChange.Disabled &&
                change != InputDeviceChange.ConfigurationChanged)
                return;

            _refreshRequested = true;
        }

        private void OnActionChange(object changed, InputActionChange change)
        {
            if (change == InputActionChange.BoundControlsChanged &&
                BindingDisplayResolver.OwnsChange(changed, GetInputActions()))
                _refreshRequested = true;
        }

        public BindingTarget GetBindingTarget()
        {
            return new BindingTarget
            {
                ActionMapName = actionMapName,
                ActionName = actionName,
                BindingIndex = runtimeBindingIndex,
                CompositeMode = compositeBindingMode,
                Configurator = deviceDisplayConfigurator
            };
        }

        public void SetBindingTarget(InputAction action, int bindingIndex, bool showBindingNameWhenNoIcon = true)
        {
            if (action == null || action.actionMap == null)
            {
                actionMapName = string.Empty;
                actionName = string.Empty;
                runtimeBindingIndex = -1;
                RefreshGraphic();
                return;
            }

            SetInputActionsAsset(action.actionMap.asset);
            actionMapName = action.actionMap.name;
            actionName = action.name;
            runtimeBindingIndex = bindingIndex;
            displayBindingName = showBindingNameWhenNoIcon;
            RefreshGraphic();
        }

        protected virtual void SetInputActionsAsset(InputActionAsset inputActions)
        {
        }

        public void RefreshGraphic()
        {
            var action = BindingDisplayResolver.FindAction(GetInputActions(), actionMapName, actionName);
            if (!BindingDisplayResolver.TryResolve(action, runtimeBindingIndex, GetActiveDevice(),
                    DeviceFilter, out var presentation, compositeBindingMode, deviceDisplayConfigurator))
            {
                if (bindingItemPrefab != null)
                    RenderItems(default);
                else
                {
                    SetItemCount(0);
                    ApplyDisplay(null, null);
                }
                return;
            }

            if (bindingItemPrefab != null)
            {
                RenderItems(presentation);
                return;
            }

            SetItemCount(0);
            var sprite = deviceDisplayConfigurator != null
                ? deviceDisplayConfigurator.GetDeviceBindingIcon(presentation.DeviceLayout, presentation.IconKey)
                : null;
            ApplyDisplay(sprite, presentation.Text);
        }

        private void RenderItems(BindingDisplayResolver.Presentation presentation)
        {
            var parent = bindingItemsParent != null ? bindingItemsParent : (RectTransform)transform;
            if (displayImage != null)
            {
                displayImage.enabled = false;
                if (displayImage.transform != transform && !parent.IsChildOf(displayImage.transform))
                {
                    displayImage.gameObject.SetActive(false);
                    _hiddenLegacyImage = true;
                }
            }
            if (bindingNameText != null)
                bindingNameText.enabled = false;

            var elements = presentation.Elements ?? (string.IsNullOrEmpty(presentation.Text)
                ? System.Array.Empty<BindingDisplayResolver.Element>()
                : new[] { new BindingDisplayResolver.Element(presentation.Text, presentation.DeviceLayout, presentation.IconKey) });
            while (_items.Count < elements.Length)
            {
                var item = Instantiate(bindingItemPrefab, parent, false);
                item.name = $"BindingItem_{_items.Count}";
                item.gameObject.hideFlags = HideFlags.DontSave;
                _items.Add(item);
            }
            SetItemCount(elements.Length);
            for (int i = 0; i < elements.Length; i++)
            {
                var element = elements[i];
                _items[i].transform.SetParent(parent, false);
                _items[i].transform.SetAsLastSibling();
                if (element.IsSeparator)
                    _items[i].ShowSeparator(element.Text);
                else
                    _items[i].Show(deviceDisplayConfigurator != null
                        ? deviceDisplayConfigurator.GetDeviceBindingIcon(element.DeviceLayout, element.IconKey)
                        : null, element.Text);
            }
            if (parent.TryGetComponent<CupOHappiness.UI.Layout>(out var layout))
            {
                layout.RefreshChildCache();
                layout.SetDirty();
            }
            LayoutRebuilder.MarkLayoutForRebuild(parent);
        }

        private void SetItemCount(int count)
        {
            for (int i = 0; i < _items.Count; i++)
                if (_items[i] != null)
                    _items[i].gameObject.SetActive(i < count);
        }

        private void ApplyDisplay(Sprite sprite, string bindingDisplay)
        {
            if (_hiddenLegacyImage && displayImage != null)
            {
                displayImage.gameObject.SetActive(true);
                _hiddenLegacyImage = false;
            }
            bool hasIcon = sprite != null;
            bool showBindingName = displayBindingName && !string.IsNullOrEmpty(bindingDisplay) && !hasIcon;

            if (displayImage != null)
            {
                CacheDefaultDisplaySprite();
                displayImage.sprite = hasIcon ? sprite : _defaultDisplaySprite;
                displayImage.type = hasIcon ? Image.Type.Simple : _defaultDisplayImageType;
                displayImage.preserveAspect = hasIcon || _defaultDisplayImagePreserveAspect;
                displayImage.enabled = hasIcon || showBindingName;
            }

            if (bindingNameText != null)
            {
                bindingNameText.text = bindingDisplay ?? string.Empty;
                bindingNameText.enabled = showBindingName;
            }

            // Keep the authored icon RectTransform size. Automatic image scaling is disabled.
        }

        private void CacheDefaultDisplaySprite()
        {
            if (displayImage == null || _cachedDefaultDisplaySprite)
            {
                return;
            }

            _defaultDisplaySprite = displayImage.sprite;
            _defaultDisplayImageType = displayImage.type;
            _defaultDisplayImagePreserveAspect = displayImage.preserveAspect;
            _cachedDefaultDisplaySprite = true;
        }

    }
}
