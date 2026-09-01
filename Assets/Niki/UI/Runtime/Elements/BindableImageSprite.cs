using UnityEngine;
using UnityEngine.UI;

namespace Niki.UI
{
    /// <summary>Binds a Property&lt;Sprite&gt; to an Image's sprite slot.</summary>
    [RequireComponent(typeof(Image))]
    public class BindableImageSprite : BindableElement<Sprite>
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

        protected override void Apply(Sprite value)
        {
            if (_image != null)
                _image.sprite = value;
        }
    }
}
