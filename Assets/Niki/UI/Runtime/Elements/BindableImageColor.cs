using UnityEngine;
using UnityEngine.UI;

namespace Niki.UI
{
    /// <summary>Binds a Property&lt;Color&gt; to an Image's tint color.</summary>
    [RequireComponent(typeof(Image))]
    public class BindableImageColor : BindableElement<Color>
    {
        [SerializeField, HideInInspector] private Image _image;

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

        protected override void Apply(Color value)
        {
            if (_image != null)
                _image.color = value;
        }
    }
}
