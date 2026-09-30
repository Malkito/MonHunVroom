using TMPro;
using UnityEngine;

namespace Niki.UI
{
    /// <summary>Binds a Property&lt;string&gt; to a TextMeshPro element's text.</summary>
    [RequireComponent(typeof(TMP_Text))]
    public class BindableText : BindableElement<string>
    {
        [SerializeField, HideInInspector] private TMP_Text _text;

        private void Awake()
        {
            if (_text == null)
                _text = GetComponent<TMP_Text>();
        }

        /// <summary>Point this element at a specific TMP element (used by editor tooling).</summary>
        public void SetText(TMP_Text text)
        {
            _text = text;
        }

        protected override void Apply(string value)
        {
            if (_text != null)
                _text.text = value;
        }
    }
}
