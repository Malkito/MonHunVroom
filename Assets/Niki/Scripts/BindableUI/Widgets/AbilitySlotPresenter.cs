using UnityEngine;
using UnityEngine.UI;
using UnityServiceLocator;

namespace Niki.UI
{
    /// <summary>Connects a player's ability-slot data to one HUD slot.</summary>
    public class AbilitySlotPresenter : MonoBehaviour
    {
        [SerializeField, Range(0, 2)] private int slotIndex;
        [SerializeField, HideInInspector] private AbilitySlotViewModel _viewModel;
        [SerializeField, HideInInspector] private BindableImageSprite _icon;
        [SerializeField, HideInInspector] private BindableText _name;
        [SerializeField, HideInInspector] private BindableBoolColor _pressIndicator;
        [SerializeField, HideInInspector] private BindableRadialCooldown _radialCooldown;

        [Header("Ability slot graphics")]
        [SerializeField] private Image _abilityIcon;
        [SerializeField] private Graphic _abilityGlint;
        [SerializeField] private Image _cooldownImage;
        [SerializeField] private bool hideNameLabel = true;
        [SerializeField] private Color emptyColor = Color.clear;
        [SerializeField] private Color cooldownColor = Color.black;

        private IPlayerAbilitySlots abilitySlots;

        public AbilitySlotViewModel ViewModel => _viewModel;

        private void Awake()
        {
            FindPrefabElements();
            if (_viewModel == null)
                _viewModel = new AbilitySlotViewModel();
            Initialize();
        }

        private void Start()
        {
            if (!ServiceLocator.For(this).TryGet(out abilitySlots))
                Debug.LogError($"[{name}] No IPlayerAbilitySlots service is registered for this player.", this);
        }

        private void Update()
        {
            if (abilitySlots == null) return;

            if (!abilitySlots.TryGetSlot(slotIndex, out var slot))
            {
                ClearSlot();
                return;
            }

            _viewModel.Icon.Value = slot.Icon;
            _viewModel.AbilityColor.Value = slot.Color;
            _viewModel.Name.Value = slot.Name;
            _viewModel.CooldownRemaining.Value = slot.CooldownRemaining;
            _viewModel.IsPressed.Value = slot.IsPressed;
        }

        public void Initialize()
        {
            _icon?.Bind(_viewModel.Icon);
            _name?.Bind(_viewModel.Name);
            _pressIndicator?.Bind(_viewModel.IsPressed);
            _radialCooldown?.Bind(_viewModel.CooldownRemaining);

            _viewModel.Icon.ValueChanged -= ApplyIcon;
            _viewModel.AbilityColor.ValueChanged -= ApplyAbilityColor;
            _viewModel.CooldownRemaining.ValueChanged -= ApplyCooldown;
            _viewModel.Icon.ValueChanged += ApplyIcon;
            _viewModel.AbilityColor.ValueChanged += ApplyAbilityColor;
            _viewModel.CooldownRemaining.ValueChanged += ApplyCooldown;

            ApplyIcon(_viewModel.Icon.Value);
            ApplyAbilityColor(_viewModel.AbilityColor.Value);
            ApplyCooldown(_viewModel.CooldownRemaining.Value);
        }

        private void FindPrefabElements()
        {
            _abilityIcon ??= FindChild<Image>("sprt_Icon", "Icon");
            _abilityGlint ??= FindChild<Graphic>("sprt_Ability_Glint");
            _cooldownImage ??= FindChild<Image>("sprt_Cooldown", "RadialCooldown");

            if (hideNameLabel && FindChild<Transform>("txt_Label", "Name") is { } label)
                label.gameObject.SetActive(false);
        }

        private T FindChild<T>(params string[] names) where T : Component
        {
            foreach (var childName in names)
            {
                var child = transform.Find(childName);
                var component = child != null ? child.GetComponent<T>() : null;
                if (component != null) return component;
            }

            return null;
        }

        private void ApplyIcon(Sprite icon)
        {
            if (_abilityIcon != null)
                _abilityIcon.sprite = icon;

            ApplySlotColor(_viewModel.CooldownRemaining.Value);
        }

        private void ApplyAbilityColor(Color _)
        {
            ApplySlotColor(_viewModel.CooldownRemaining.Value);
        }

        private void ApplyColor(Color color)
        {
            if (_abilityIcon != null) _abilityIcon.color = color;
        }

        private void ApplyCooldown(float remaining)
        {
            if (_cooldownImage != null)
                _cooldownImage.fillAmount = Mathf.Clamp01(remaining);

            ApplySlotColor(remaining);
        }

        private void ApplySlotColor(float cooldownRemaining)
        {
            var color = _viewModel.Icon.Value == null
                ? emptyColor
                : cooldownRemaining > 0f ? cooldownColor : _viewModel.AbilityColor.Value;
            ApplyColor(color);
            if (_abilityGlint != null)
                _abilityGlint.color = _viewModel.Icon.Value == null ? emptyColor : _viewModel.AbilityColor.Value;
        }

        private void ClearSlot()
        {
            _viewModel.Icon.Value = null;
            _viewModel.AbilityColor.Value = emptyColor;
            _viewModel.Name.Value = string.Empty;
            _viewModel.CooldownRemaining.Value = 0f;
            _viewModel.IsPressed.Value = false;
        }

        private void OnDestroy()
        {
            _icon?.Unbind();
            _name?.Unbind();
            _pressIndicator?.Unbind();
            _radialCooldown?.Unbind();
            _viewModel.Icon.ValueChanged -= ApplyIcon;
            _viewModel.AbilityColor.ValueChanged -= ApplyAbilityColor;
            _viewModel.CooldownRemaining.ValueChanged -= ApplyCooldown;
        }
    }
}
