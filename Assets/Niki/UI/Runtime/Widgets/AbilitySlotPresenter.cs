using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Niki.UI
{
    /// <summary>
    /// Presents one player ability slot through the binding system.
    /// It reads playerUpgradeManager and writes the slot view model. The view model
    /// then updates the bound icon, glint, input state, and cooldown image.
    ///
    /// CooldownRemaining is normalized as remaining cooldown / ability cooldown,
    /// so the Image.fillAmount value is always in the 0..1 range.
    /// </summary>
    public class AbilitySlotPresenter : MonoBehaviour
    {
        [Header("Player ability")]
        // Kept as MonoBehaviour because Niki.UI.Runtime is an assembly definition
        // and cannot reference the game's Assembly-CSharp types directly.
        [Tooltip("Player object containing the runtime equipped ability slots and cooldown values.")]
        [SerializeField] private MonoBehaviour playerUpgradeManager;
        [Tooltip("Ability slot read by this presenter: 0 = first, 1 = second, 2 = third.")]
        [SerializeField, Range(0, 2)] private int slotIndex;

        [Header("View model")]
        [SerializeField, HideInInspector] private AbilitySlotViewModel _viewModel;

        [Header("Elements")]
        [SerializeField, HideInInspector] private BindableImageSprite _icon;
        [SerializeField, HideInInspector] private BindableText _name;
        [SerializeField, HideInInspector] private BindableBoolColor _pressIndicator;
        [SerializeField, HideInInspector] private BindableRadialCooldown _radialCooldown;

        [Header("Ability prefab fallback elements")]
        [Tooltip("Image that receives the ability sprite and ability color.")]
        [SerializeField] private Image _abilityIcon;
        [Tooltip("Graphic that receives the same ability color as the icon.")]
        [SerializeField] private Graphic _abilityGlint;
        [Tooltip("Radial Image whose fillAmount displays normalized remaining cooldown: 0 = ready, 1 = full cooldown.")]
        [SerializeField] private Image _cooldownImage;
        [Tooltip("Disables the child named txt_Label or Name because this slot does not display ability names.")]
        [SerializeField] private bool hideNameLabel = true;
        [Tooltip("Icon and glint color used when this slot has no assigned ability. Color.clear hides both graphics.")]
        [SerializeField] private Color emptyColor = Color.clear;

        private void Awake()
        {
            if (playerUpgradeManager == null)
                playerUpgradeManager = FindGameComponent("playerUpgradeManager");

            FindPrefabElements();
            // The view model is a plain C# object, so it does not survive domain
            // reloads; recreate it if needed. The element references are normal
            // MonoBehaviour references and DO survive, so re-bind everything here.
            if (_viewModel == null)
                _viewModel = new AbilitySlotViewModel();
            Initialize();
        }

        private void Update()
        {
            if (playerUpgradeManager == null || !GetMember<bool>(playerUpgradeManager, "IsOwner"))
                return;

            PresentCurrentAbility();
        }

        /// <summary>The view model driving this slot.</summary>
        public AbilitySlotViewModel ViewModel => _viewModel;

        /// <summary>Assign elements (editor tooling / code-driven setup), then bind them.</summary>
        public void Configure(
            AbilitySlotViewModel viewModel,
            BindableImageSprite icon,
            BindableText name,
            BindableBoolColor pressIndicator,
            BindableRadialCooldown radialCooldown)
        {
            _viewModel = viewModel;
            _icon = icon;
            _name = name;
            _pressIndicator = pressIndicator;
            _radialCooldown = radialCooldown;
            FindPrefabElements();
            Initialize();
        }

        /// <summary>Bind all wired elements to the view model.</summary>
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

        /// <summary>Unbind everything (teardown / player left the game).</summary>
        public void Dispose()
        {
            _icon?.Unbind();
            _name?.Unbind();
            _pressIndicator?.Unbind();
            _radialCooldown?.Unbind();
            _viewModel.Icon.ValueChanged -= ApplyIcon;
            _viewModel.AbilityColor.ValueChanged -= ApplyAbilityColor;
            _viewModel.CooldownRemaining.ValueChanged -= ApplyCooldown;
        }

        private void FindPrefabElements()
        {
            _abilityIcon ??= FindChild<Image>("sprt_Icon", "Icon");
            _abilityGlint ??= FindChild<Graphic>("sprt_Ability_Glint");
            _cooldownImage ??= FindChild<Image>("sprt_Cooldown", "RadialCooldown");

            if (hideNameLabel)
            {
                var label = FindChild<Transform>("txt_Label", "Name");
                if (label != null)
                    label.gameObject.SetActive(false);
            }
        }

        private T FindChild<T>(params string[] names) where T : Component
        {
            foreach (var name in names)
            {
                var child = transform.Find(name);
                var component = child != null ? child.GetComponent<T>() : null;
                if (component != null)
                    return component;
            }

            return null;
        }

        private void ApplyIcon(Sprite icon)
        {
            if (_abilityIcon != null)
                _abilityIcon.sprite = icon;

            // A null sprite means that this slot has no assigned ability.
            var color = icon == null ? emptyColor : _viewModel.AbilityColor.Value;
            ApplyColor(color);
        }

        private void ApplyAbilityColor(Color color)
        {
            if (_viewModel.Icon.Value != null)
                ApplyColor(color);
        }

        private void ApplyColor(Color color)
        {
            if (_abilityIcon != null)
                _abilityIcon.color = color;
            if (_abilityGlint != null)
                _abilityGlint.color = color;
        }

        private void ApplyCooldown(float remaining)
        {
            if (_cooldownImage != null)
                _cooldownImage.fillAmount = Mathf.Clamp01(remaining);
        }

        private void PresentCurrentAbility()
        {
            var equipped = GetMember<Array>(playerUpgradeManager, "equipped");
            if (equipped == null || slotIndex < 0 || slotIndex >= equipped.Length)
            {
                ClearSlot();
                return;
            }

            var entry = equipped.GetValue(slotIndex);
            if (entry == null || GetMember<UnityEngine.Object>(entry, "logicInstance") == null)
            {
                ClearSlot();
                return;
            }

            int upgradeId = GetMember<int>(entry, "upgradeID");
            var definition = GetUpgradeDefinition(upgradeId);
            if (definition == null)
            {
                ClearSlot();
                return;
            }

            _viewModel.Icon.Value = GetMember<Sprite>(definition, "IconImage");
            _viewModel.AbilityColor.Value = GetMember<Color>(definition, "abilityColor");
            _viewModel.CooldownRemaining.Value = GetNormalizedCooldown(
                GetMember<float>(definition, "cooldown"));
            _viewModel.IsPressed.Value = IsAbilityPressed();
        }

        private float GetNormalizedCooldown(float cooldownDuration)
        {
            if (cooldownDuration <= 0f)
                return 0f;

            float remaining = slotIndex switch
            {
                0 => GetMember<float>(playerUpgradeManager, "abilityOneCooldown"),
                1 => GetMember<float>(playerUpgradeManager, "abilityTwoCooldown"),
                2 => GetMember<float>(playerUpgradeManager, "abilityThreeCooldown"),
                _ => 0f
            };

            return Mathf.Clamp01(remaining / cooldownDuration);
        }

        private bool IsAbilityPressed()
        {
            var input = GetStaticMember("GameInput", "instance");
            if (input == null)
                return false;

            string methodName = slotIndex switch
            {
                0 => "getAbilityOneInput",
                1 => "getAbilityTwoInput",
                2 => "getAbilityThreeInput",
                _ => null
            };

            return methodName != null && (bool)input.GetType()
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public)
                ?.Invoke(input, null);
        }

        private object GetUpgradeDefinition(int upgradeId)
        {
            var database = GetStaticMember("UpgradeDatabase", "Instance");
            return database?.GetType().GetMethod("Get", new[] { typeof(int) })
                ?.Invoke(database, new object[] { upgradeId });
        }

        private static MonoBehaviour FindGameComponent(string typeName)
        {
            foreach (var component in FindObjectsByType<MonoBehaviour>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (component.GetType().Name == typeName)
                    return component;
            }

            return null;
        }

        private static object GetStaticMember(string typeName, string memberName)
        {
            var type = FindType(typeName);
            var field = type?.GetField(memberName, BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
                return field.GetValue(null);

            return type?.GetProperty(memberName, BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null);
        }

        private static Type FindType(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }

            return null;
        }

        private static T GetMember<T>(object target, string memberName)
        {
            if (target == null)
                return default;

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var field = target.GetType().GetField(memberName, flags);
            if (field != null)
                return ConvertMember<T>(field.GetValue(target));

            var property = target.GetType().GetProperty(memberName, flags);
            return property == null ? default : ConvertMember<T>(property.GetValue(target));
        }

        private static T ConvertMember<T>(object value)
        {
            return value == null ? default : (T)value;
        }

        private void ClearSlot()
        {
            _viewModel.Icon.Value = null;
            _viewModel.AbilityColor.Value = emptyColor;
            _viewModel.CooldownRemaining.Value = 0f;
            _viewModel.IsPressed.Value = false;
        }

        private void OnDestroy() => Dispose();
    }
}
