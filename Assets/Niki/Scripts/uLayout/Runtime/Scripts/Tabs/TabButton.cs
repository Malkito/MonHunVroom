using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;

namespace CupOHappiness.UI
{
    /// <summary>
    /// Adapts Unity's Toggle to the TabGroup selection model.
    /// Visuals, navigation, interaction, and transitions remain standard uGUI behavior.
    /// </summary>
    [MovedFrom(true, "UserInterface")]
    [DisallowMultipleComponent, RequireComponent(typeof(Toggle))]
    public class TabButton : MonoBehaviour
    {
        [Tooltip("TabGroup that owns this tab. Usually assigned automatically when the button is registered with a group.")]
        [SerializeField, FormerlySerializedAs("tabGroup")]
        private TabGroup m_group;
        [Tooltip("The standard Unity Toggle that supplies pointer, keyboard, gamepad, interactable, and transition behavior.")]
        [SerializeField]
        private Toggle m_toggle;
        [Tooltip("Optional callbacks invoked after this tab becomes selected.")]
        [SerializeField, FormerlySerializedAs("onTabSelected")]
        private UnityEvent m_onSelected = new();
        [Tooltip("Optional callbacks invoked after this tab is no longer selected.")]
        [SerializeField, FormerlySerializedAs("onTabDeselected")]
        private UnityEvent m_onDeselected = new();

        private bool _synchronizing;

        public TabGroup Group => m_group;
        public Toggle Toggle => m_toggle;
        public Graphic TargetGraphic => m_toggle ? m_toggle.targetGraphic : null;
        public UnityEvent OnSelected => m_onSelected;
        public UnityEvent OnDeselected => m_onDeselected;
        public bool IsSelected => m_toggle && m_toggle.isOn;
        public bool IsInteractable => isActiveAndEnabled && m_toggle && m_toggle.IsInteractable();

        protected virtual void Awake() {
            EnsureReferences();
        }

        protected virtual void OnEnable() {
            EnsureReferences();
            m_toggle.onValueChanged.RemoveListener(HandleToggleChanged);
            m_toggle.onValueChanged.AddListener(HandleToggleChanged);
            if(m_group)
                m_group.Register(this);
        }

        protected virtual void OnDisable() {
            if(m_toggle)
                m_toggle.onValueChanged.RemoveListener(HandleToggleChanged);
        }

        protected virtual void OnValidate() {
            EnsureReferences(addMissingToggle: false);
        }

        public bool RequestSelection() {
            return m_group && m_group.Select(this);
        }

        public void Deselect() {
            m_onDeselected?.Invoke();
        }

        public void SetTargetGraphic(Graphic targetGraphic) {
            EnsureReferences();
            m_toggle.targetGraphic = targetGraphic;
        }

        internal void Bind(TabGroup group, ToggleGroup toggleGroup) {
            EnsureReferences();
            m_group = group;
            m_toggle.onValueChanged.RemoveListener(HandleToggleChanged);
            m_toggle.onValueChanged.AddListener(HandleToggleChanged);
            m_toggle.group = toggleGroup;
        }

        internal void SetSelectedWithoutNotify(bool selected) {
            EnsureReferences();
            _synchronizing = true;
            m_toggle.SetIsOnWithoutNotify(selected);
            _synchronizing = false;
        }

        internal void NotifySelected() {
            m_onSelected?.Invoke();
        }

        internal void NotifyDeselected() {
            Deselect();
        }

        internal void ConfigureLegacyTransition(
            TabGroup.Transition transition,
            Color idleColor,
            Color hoverColor,
            Color selectedColor,
            Sprite idleSprite,
            Sprite hoverSprite,
            Sprite selectedSprite
        ) {
            EnsureReferences();
            switch(transition) {
                case TabGroup.Transition.ColorTint:
                    m_toggle.transition = Selectable.Transition.ColorTint;
                    ColorBlock colors = m_toggle.colors;
                    colors.normalColor = idleColor;
                    colors.highlightedColor = hoverColor;
                    colors.selectedColor = selectedColor;
                    colors.pressedColor = selectedColor;
                    m_toggle.colors = colors;
                    break;
                case TabGroup.Transition.SpriteSwap:
                    m_toggle.transition = Selectable.Transition.SpriteSwap;
                    SpriteState sprites = m_toggle.spriteState;
                    sprites.highlightedSprite = hoverSprite;
                    sprites.selectedSprite = selectedSprite;
                    sprites.pressedSprite = selectedSprite;
                    m_toggle.spriteState = sprites;
                    if(m_toggle.targetGraphic is Image image)
                        image.sprite = idleSprite;
                    break;
                default:
                    m_toggle.transition = Selectable.Transition.None;
                    break;
            }
        }

        private void HandleToggleChanged(bool selected) {
            if(_synchronizing || !m_group)
                return;

            m_group.HandleToggleChanged(this, selected);
        }

        private void EnsureReferences(bool addMissingToggle = true) {
            if(!m_toggle)
                m_toggle = GetComponent<Toggle>();
            if(!m_toggle && addMissingToggle)
                m_toggle = gameObject.AddComponent<Toggle>();
            if(m_toggle && !m_toggle.targetGraphic)
                m_toggle.targetGraphic = GetComponent<Graphic>();
            m_onSelected ??= new UnityEvent();
            m_onDeselected ??= new UnityEvent();
        }
    }
}
