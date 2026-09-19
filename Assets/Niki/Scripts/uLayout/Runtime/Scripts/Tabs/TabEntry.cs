using System;
using UnityEngine;

namespace CupOHappiness.UI
{
    [Serializable]
    public sealed class TabEntry
    {
        [Tooltip("TabButton shown in the tab bar and used to select this entry.")]
        [SerializeField] private TabButton m_button;
        [Tooltip("Optional page GameObject activated while this tab is selected and deactivated otherwise.")]
        [SerializeField] private GameObject m_page;

        public TabButton Button {
            get => m_button;
            set => m_button = value;
        }

        public GameObject Page {
            get => m_page;
            set => m_page = value;
        }

        public TabEntry() { }

        public TabEntry(TabButton button, GameObject page = null) {
            m_button = button;
            m_page = page;
        }
    }
}
