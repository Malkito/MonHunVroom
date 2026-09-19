using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;

namespace CupOHappiness.UI
{
    /// <summary>
    /// Owns exclusive tab selection and maps each TabButton to an optional page.
    /// Geometry remains the responsibility of uLayout or any built-in Unity LayoutGroup.
    /// </summary>
    [MovedFrom(true, "UserInterface")]
    [DisallowMultipleComponent, RequireComponent(typeof(ToggleGroup))]
    public class TabGroup : MonoBehaviour
    {
        public enum Transition
        {
            None,
            ColorTint,
            SpriteSwap
        }

        public enum NavigationMode
        {
            Clamp,
            Wrap
        }

        [Serializable]
        public sealed class TabIndexEvent : UnityEvent<int> { }

        [Header("Tabs")]
        [Tooltip("The ordered tab buttons and their optional page GameObjects. The selected button determines which page is active.")]
        [SerializeField] private List<TabEntry> m_tabs = new();
        [Tooltip("The tab selected when this group first becomes enabled. Indices follow the Tabs list, starting at 0.")]
        [SerializeField, Min(0)] private int m_initialIndex;
        [Tooltip("Allows every tab to be deselected. When disabled, the group always keeps one available tab selected.")]
        [SerializeField] private bool m_allowNoSelection;
        [Tooltip("Controls what NextTab and PreviousTab do at the first or last tab: Clamp stops, Wrap continues from the opposite end.")]
        [SerializeField] private NavigationMode m_navigation = NavigationMode.Clamp;
        [Tooltip("When navigating with NextTab or PreviousTab, skip tabs whose Toggle is inactive or not interactable.")]
        [SerializeField] private bool m_skipNonInteractable = true;
        [Tooltip("Keeps the current selected tab when this group is disabled and enabled again.")]
        [SerializeField] private bool m_preserveSelection = true;
        [Tooltip("Invoked with the newly selected tab index. It receives -1 when selection is cleared.")]
        [SerializeField] private TabIndexEvent m_onSelectedIndexChanged = new();
        [Tooltip("ToggleGroup used to enforce exclusive tab selection. It is created automatically when missing.")]
        [SerializeField] private ToggleGroup m_toggleGroup;

        [Header("Legacy Migration")]
        [SerializeField, HideInInspector, FormerlySerializedAs("transitionType")]
        private Transition m_legacyTransition = Transition.ColorTint;
        [SerializeField, HideInInspector, FormerlySerializedAs("tabButtons")]
        private List<TabButton> m_legacyButtons = new();
        [SerializeField, HideInInspector, FormerlySerializedAs("tabPages")]
        private List<GameObject> m_legacyPages = new();
        [SerializeField, HideInInspector, FormerlySerializedAs("tabIdleColor")]
        private Color m_legacyIdleColor = Color.white;
        [SerializeField, HideInInspector, FormerlySerializedAs("tabHoverColor")]
        private Color m_legacyHoverColor = Color.white;
        [SerializeField, HideInInspector, FormerlySerializedAs("tabSelectedColor")]
        private Color m_legacySelectedColor = Color.white;
        [SerializeField, HideInInspector, FormerlySerializedAs("tabIdleSprite")]
        private Sprite m_legacyIdleSprite;
        [SerializeField, HideInInspector, FormerlySerializedAs("tabHoverSprite")]
        private Sprite m_legacyHoverSprite;
        [SerializeField, HideInInspector, FormerlySerializedAs("tabSelectedSprite")]
        private Sprite m_legacySelectedSprite;
        [SerializeField, HideInInspector] private bool m_legacyMigrated;

        private TabState _state = new(-1);
        private bool _initialized;
        private bool _applyingSelection;

        public IReadOnlyList<TabEntry> Tabs => m_tabs;
        public int SelectedIndex => _state.SelectedIndex;
        public TabButton SelectedButton => IsValidIndex(_state.SelectedIndex) ? m_tabs[_state.SelectedIndex].Button : null;
        public GameObject SelectedPage => IsValidIndex(_state.SelectedIndex) ? m_tabs[_state.SelectedIndex].Page : null;
        public bool AllowNoSelection {
            get => m_allowNoSelection;
            set {
                if(m_allowNoSelection == value)
                    return;
                m_allowNoSelection = value;
                EnsureReferences();
                m_toggleGroup.allowSwitchOff = value;
                if(!value && _state.SelectedIndex < 0)
                    SelectFirstAvailable();
            }
        }
        public NavigationMode Navigation {
            get => m_navigation;
            set => m_navigation = value;
        }
        public bool SkipNonInteractable {
            get => m_skipNonInteractable;
            set => m_skipNonInteractable = value;
        }
        public bool PreserveSelection {
            get => m_preserveSelection;
            set => m_preserveSelection = value;
        }
        public int InitialIndex {
            get => m_initialIndex;
            set => m_initialIndex = Mathf.Max(0, value);
        }
        public TabIndexEvent OnSelectedIndexChanged => m_onSelectedIndexChanged;
        public ToggleGroup ToggleGroup => m_toggleGroup;

        protected virtual void Awake() {
            RepairConfiguration();
        }

        protected virtual void OnEnable() {
            RepairConfiguration();
            if(!m_preserveSelection || !IsValidIndex(_state.SelectedIndex))
                _state = new TabState(-1);
            _initialized = true;
            InitializeSelection();
        }

        protected virtual void Start() {
            // Start runs after every active child's OnEnable, independent of script execution order.
            InitializeSelection();
        }

        protected virtual void OnDisable() {
            _initialized = false;
            if(!m_preserveSelection)
                _state = new TabState(-1);
        }

        protected virtual void OnValidate() {
            m_initialIndex = Mathf.Max(0, m_initialIndex);
            EnsureReferences(addMissingGroup: false);
            if(m_toggleGroup)
                m_toggleGroup.allowSwitchOff = m_allowNoSelection;
        }

        public bool InitializeSelection() {
            RepairConfiguration();
            if(IsValidIndex(_state.SelectedIndex)) {
                ApplySelectionState();
                return true;
            }

            if(SelectNearestAvailable(m_initialIndex, allowPendingHierarchy: true))
                return true;

            if(m_allowNoSelection)
                ApplySelectionState();
            return false;
        }

        public bool Select(int index) {
            return SelectInternal(index, allowPendingHierarchy: false);
        }

        private bool SelectInternal(int index, bool allowPendingHierarchy) {
            RepairConfiguration();
            if(!CanSelect(index, allowPendingHierarchy))
                return false;

            return Dispatch(new TabCommand(TabCommandType.Select, index), allowPendingHierarchy);
        }

        public bool Select(TabButton button) {
            return button && Select(IndexOf(button));
        }

        public bool ClearSelection() {
            if(!m_allowNoSelection)
                return false;
            return Dispatch(new TabCommand(TabCommandType.Clear), allowPendingHierarchy: false);
        }

        public bool NextTab() {
            return Navigate(1);
        }

        public bool PreviousTab() {
            return Navigate(-1);
        }

        public int Register(TabButton button, GameObject page = null) {
            if(!button)
                return -1;

            EnsureReferences();
            MigrateLegacyDataIfNeeded();
            int index = IndexOf(button);
            if(index < 0) {
                m_tabs.Add(new TabEntry(button, page));
                index = m_tabs.Count - 1;
            }
            else if(page && !m_tabs[index].Page) {
                m_tabs[index].Page = page;
            }

            BindEntry(m_tabs[index]);
            if(_initialized)
                InitializeSelection();
            return index;
        }

        public bool Unregister(TabButton button) {
            int index = IndexOf(button);
            if(index < 0)
                return false;

            TabEntry removedEntry = m_tabs[index];
            bool removedSelected = index == _state.SelectedIndex;
            if(button.Toggle && button.Toggle.group == m_toggleGroup)
                button.Toggle.group = null;
            if(removedEntry.Page && removedEntry.Page.activeSelf)
                removedEntry.Page.SetActive(false);
            m_tabs.RemoveAt(index);

            if(index < _state.SelectedIndex)
                _state = new TabState(_state.SelectedIndex - 1);
            else if(removedSelected)
                _state = new TabState(-1);

            if(removedSelected) {
                button.NotifyDeselected();
                if(!SelectNearestAvailable(Mathf.Min(index, m_tabs.Count - 1))) {
                    ApplySelectionState();
                    m_onSelectedIndexChanged?.Invoke(-1);
                }
            }
            return true;
        }

        public bool SetPage(TabButton button, GameObject page) {
            int index = IndexOf(button);
            if(index < 0)
                return false;

            GameObject oldPage = m_tabs[index].Page;
            if(oldPage && oldPage != page && index == _state.SelectedIndex)
                oldPage.SetActive(false);
            m_tabs[index].Page = page;
            if(page)
                page.SetActive(index == _state.SelectedIndex);
            return true;
        }

        internal void HandleToggleChanged(TabButton button, bool selected) {
            if(_applyingSelection)
                return;

            if(selected) {
                Select(button);
                return;
            }

            if(SelectedButton != button)
                return;

            foreach(TabEntry entry in m_tabs) {
                if(entry?.Button && entry.Button != button && entry.Button.Toggle && entry.Button.Toggle.isOn) {
                    Select(entry.Button);
                    return;
                }
            }

            if(!ClearSelection())
                button.SetSelectedWithoutNotify(true);
        }

        public void CollectDirectChildTabs() {
            Dictionary<TabButton, GameObject> existingPages = new();
            foreach(TabEntry entry in m_tabs) {
                if(entry?.Button && !existingPages.ContainsKey(entry.Button))
                    existingPages.Add(entry.Button, entry.Page);
            }

            List<TabButton> buttons = new();
            foreach(TabButton button in GetComponentsInChildren<TabButton>(true)) {
                if(button.transform.parent == transform)
                    buttons.Add(button);
            }
            buttons.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

            m_tabs.Clear();
            for(int i = 0; i < buttons.Count; i++) {
                GameObject page = existingPages.TryGetValue(buttons[i], out GameObject existingPage)
                    ? existingPage
                    : i < m_legacyPages.Count ? m_legacyPages[i] : null;
                m_tabs.Add(new TabEntry(buttons[i], page));
            }
            BindEntries();
        }

        public void RepairConfiguration() {
            EnsureReferences();
            MigrateLegacyDataIfNeeded();
            BindEntries();
            m_initialIndex = m_tabs.Count == 0 ? 0 : Mathf.Clamp(m_initialIndex, 0, m_tabs.Count - 1);
        }

        [Obsolete("Use Register(TabButton, GameObject) instead.")]
        public void Subscribe(TabButton button) {
            Register(button);
        }

        [Obsolete("Use Select(TabButton) instead.")]
        public void OnTabSelected(TabButton button) {
            Select(button);
        }

        [Obsolete("Hover visuals are controlled by the standard Toggle transition.")]
        public void OnTabEnter(TabButton button) { }

        [Obsolete("Hover visuals are controlled by the standard Toggle transition.")]
        public void OnTabExit(TabButton button) { }

        private bool Navigate(int direction) {
            RepairConfiguration();
            TabCommandType command = direction > 0
                ? TabCommandType.Next
                : TabCommandType.Previous;
            return Dispatch(new TabCommand(command), allowPendingHierarchy: false);
        }

        private bool SelectNearestAvailable(int requestedIndex, bool allowPendingHierarchy = false) {
            if(m_tabs.Count == 0)
                return false;

            int start = Mathf.Clamp(requestedIndex, 0, m_tabs.Count - 1);
            for(int distance = 0; distance < m_tabs.Count; distance++) {
                int forward = start + distance;
                if(forward < m_tabs.Count && CanSelect(forward, allowPendingHierarchy))
                    return SelectInternal(forward, allowPendingHierarchy);
                int backward = start - distance;
                if(distance > 0 && backward >= 0 && CanSelect(backward, allowPendingHierarchy))
                    return SelectInternal(backward, allowPendingHierarchy);
            }
            return false;
        }

        private bool SelectFirstAvailable() {
            return SelectNearestAvailable(0);
        }

        private bool CanSelect(int index, bool allowPendingHierarchy = false) {
            if(!IsValidIndex(index))
                return false;
            TabButton button = m_tabs[index].Button;
            if(!button)
                return false;

            bool active = allowPendingHierarchy ? button.gameObject.activeSelf : button.gameObject.activeInHierarchy;
            if(!active)
                return false;
            if(!m_skipNonInteractable)
                return true;
            return button.Toggle && (allowPendingHierarchy ? button.Toggle.interactable : button.Toggle.IsInteractable());
        }

        private bool Dispatch(TabCommand command, bool allowPendingHierarchy) {
            TabSnapshot snapshot = BuildSnapshot(allowPendingHierarchy);
            TabTransition transition = TabStateReducer.Reduce(_state, snapshot, command);
            if(!transition.Accepted)
                return false;

            _state = transition.State;
            if(transition.Effects.Length == 0) {
                ApplySelectionState();
                return true;
            }

            foreach(TabEffect effect in transition.Effects)
                ApplyEffect(effect);
            return true;
        }

        private TabSnapshot BuildSnapshot(bool allowPendingHierarchy) {
            bool[] selectable = new bool[m_tabs.Count];
            for(int i = 0; i < selectable.Length; i++)
                selectable[i] = CanSelect(i, allowPendingHierarchy);
            return new TabSnapshot(
                selectable,
                m_allowNoSelection,
                m_navigation == NavigationMode.Wrap);
        }

        private void ApplyEffect(TabEffect effect) {
            switch(effect.Type) {
                case TabEffectType.ApplySelection:
                    ApplySelectionState();
                    break;
                case TabEffectType.NotifyDeselected:
                    if(IsValidIndex(effect.Index))
                        m_tabs[effect.Index].Button?.NotifyDeselected();
                    break;
                case TabEffectType.NotifySelected:
                    if(IsValidIndex(effect.Index))
                        m_tabs[effect.Index].Button?.NotifySelected();
                    break;
                case TabEffectType.NotifyIndexChanged:
                    m_onSelectedIndexChanged?.Invoke(effect.Index);
                    break;
            }
        }

        private void ApplySelectionState() {
            if(_applyingSelection)
                return;

            _applyingSelection = true;
            try {
                for(int i = 0; i < m_tabs.Count; i++) {
                    TabEntry entry = m_tabs[i];
                    if(entry == null)
                        continue;

                    bool selected = i == _state.SelectedIndex;
                    if(entry.Button)
                        entry.Button.SetSelectedWithoutNotify(selected);
                    if(entry.Page && entry.Page.activeSelf != selected)
                        entry.Page.SetActive(selected);
                }
            }
            finally {
                _applyingSelection = false;
            }
        }

        private void BindEntries() {
            m_toggleGroup.allowSwitchOff = m_allowNoSelection;
            foreach(TabEntry entry in m_tabs)
                BindEntry(entry);
        }

        private void BindEntry(TabEntry entry) {
            if(entry?.Button)
                entry.Button.Bind(this, m_toggleGroup);
        }

        private void MigrateLegacyDataIfNeeded() {
            if(m_legacyMigrated)
                return;

            if(m_tabs.Count == 0) {
                List<TabButton> buttons = new();
                foreach(TabButton button in m_legacyButtons) {
                    if(button && !buttons.Contains(button))
                        buttons.Add(button);
                }
                if(buttons.Count == 0) {
                    foreach(TabButton button in GetComponentsInChildren<TabButton>(true)) {
                        if(button.transform.parent == transform)
                            buttons.Add(button);
                    }
                    buttons.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
                }

                for(int i = 0; i < buttons.Count; i++) {
                    GameObject page = i < m_legacyPages.Count ? m_legacyPages[i] : null;
                    m_tabs.Add(new TabEntry(buttons[i], page));
                }
            }

            foreach(TabEntry entry in m_tabs) {
                entry?.Button?.ConfigureLegacyTransition(
                    m_legacyTransition,
                    m_legacyIdleColor,
                    m_legacyHoverColor,
                    m_legacySelectedColor,
                    m_legacyIdleSprite,
                    m_legacyHoverSprite,
                    m_legacySelectedSprite
                );
            }
            m_legacyMigrated = true;
        }

        private void EnsureReferences(bool addMissingGroup = true) {
            if(!m_toggleGroup)
                m_toggleGroup = GetComponent<ToggleGroup>();
            if(!m_toggleGroup && addMissingGroup)
                m_toggleGroup = gameObject.AddComponent<ToggleGroup>();
            m_tabs ??= new List<TabEntry>();
            m_legacyButtons ??= new List<TabButton>();
            m_legacyPages ??= new List<GameObject>();
            m_onSelectedIndexChanged ??= new TabIndexEvent();
        }

        private int IndexOf(TabButton button) {
            if(!button)
                return -1;
            for(int i = 0; i < m_tabs.Count; i++) {
                if(m_tabs[i]?.Button == button)
                    return i;
            }
            return -1;
        }

        private bool IsValidIndex(int index) {
            return index >= 0 && index < m_tabs.Count && m_tabs[index] != null;
        }
    }
}
