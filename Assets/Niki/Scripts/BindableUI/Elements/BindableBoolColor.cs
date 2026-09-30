using UnityEngine;
using UnityEngine.UI;

namespace Niki.UI
{
    /// <summary>
    /// Binds a Property&lt;bool&gt; to an Image's tint: one color when true,
    /// another when false (e.g. input-press feedback on an ability icon).
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class BindableBoolColor : BindableElement<bool>
    {
        [SerializeField, HideInInspector] private Image _image;
        [SerializeField] private Color trueColor = Color.white;
        [SerializeField] private Color falseColor = new Color(1f, 1f, 1f, 0.5f);

        private void Awake()
        {
            if (_image == null)
                _image = GetComponent<Image>();
        }

        /// <summary>Point this element at a specific Image (used by editor tooling).</summary>
        public void SetImage(Image image)
        {
            _image = image;
        }

        /// <summary>Set the two tints (used by editor tooling).</summary>
        public void SetColors(Color whenTrue, Color whenFalse)
        {
            trueColor = whenTrue;
            falseColor = whenFalse;
        }

        protected override void Apply(bool value)
        {
            if (_image != null)
                _image.color = value ? trueColor : falseColor;
        }
    }
}
