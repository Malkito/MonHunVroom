using UnityEngine;

namespace Niki.UI
{
    /// <summary>
    /// Widget for one ability slot: icon sprite, name label, input-press feedback
    /// and a radial cooldown display.
    ///
    /// Data flow:
    ///   model layer (playerUpgradeManager, UpgradeDatabase, GameInput)
    ///     -> widget.ViewModel (AbilitySlotViewModel)
    ///       -> bound BindableElements (auto-updated on change)
    ///
    /// Usage:
    ///   1. Wire the elements (Inspector, or Configure() / NikiUiBuilder menu).
    ///   2. Each frame (or on model events), write into widget.ViewModel:
    ///        Icon, Name, CooldownRemaining, IsPressed.
    ///   3. The bound UI elements react automatically.
    /// </summary>
    public class AbilitySlotWidget : MonoBehaviour
    {
        [Header("View model")]
        [SerializeField, HideInInspector] private AbilitySlotViewModel _viewModel;

        [Header("Elements")]
        [SerializeField, HideInInspector] private BindableImageSprite _icon;
        [SerializeField, HideInInspector] private BindableText _name;
        [SerializeField, HideInInspector] private BindableBoolColor _pressIndicator;
        [SerializeField, HideInInspector] private BindableRadialCooldown _radialCooldown;

        private void Awake()
        {
            // The view model is a plain C# object, so it does not survive domain
            // reloads; recreate it if needed. The element references are normal
            // MonoBehaviour references and DO survive, so re-bind everything here.
            if (_viewModel == null)
                _viewModel = new AbilitySlotViewModel();
            Initialize();
        }

        /// <summary>The view model driving this slot. The model layer writes into it.</summary>
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
            Initialize();
        }

        /// <summary>Bind all wired elements to the view model.</summary>
        public void Initialize()
        {
            _icon?.Bind(_viewModel.Icon);
            _name?.Bind(_viewModel.Name);
            _pressIndicator?.Bind(_viewModel.IsPressed);
            _radialCooldown?.Bind(_viewModel.CooldownRemaining);
        }

        /// <summary>Unbind everything (teardown / player left the game).</summary>
        public void Dispose()
        {
            _icon?.Unbind();
            _name?.Unbind();
            _pressIndicator?.Unbind();
            _radialCooldown?.Unbind();
        }

        private void OnDestroy() => Dispose();
    }
}
