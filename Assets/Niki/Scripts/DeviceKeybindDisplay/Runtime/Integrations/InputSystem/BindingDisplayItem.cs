using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CupOHappiness.DeviceKeybindDisplay.Integrations.InputSystem
{
    /// <summary>Displays one binding icon, fallback keycap, or separator. Does not resolve input.</summary>
    [DisallowMultipleComponent]
    public sealed class BindingDisplayItem : MonoBehaviour
    {
        [SerializeField, Tooltip("Image used for the binding icon. Its authored sprite is the fallback key background.")]
        private Image _image;
        [SerializeField, Tooltip("Text used when no icon is available, or for a separator such as + or /.")]
        private TMP_Text _label;

        private Sprite _background;
        private Image.Type _backgroundType;
        private bool _backgroundPreserveAspect;
        private bool _cachedBackground;

        private void Reset()
        {
            _image = GetComponent<Image>();
            _label = GetComponentInChildren<TMP_Text>(true);
        }

        private void Awake() => CacheBackground();

        /// <summary>Shows the icon, or the supplied text on the authored background when the icon is missing.</summary>
        public void Show(Sprite icon, string fallbackText)
        {
            Apply(icon, fallbackText, true);
        }

        /// <summary>Shows text without a key background, for example + between required keys.</summary>
        public void ShowSeparator(string text)
        {
            Apply(null, text, false);
        }

        private void Apply(Sprite icon, string text, bool showBackground)
        {
            CacheBackground();
            bool hasIcon = icon != null;
            bool hasText = !hasIcon && !string.IsNullOrEmpty(text);

            if (_image != null)
            {
                _image.sprite = hasIcon ? icon : _background;
                _image.type = hasIcon ? Image.Type.Simple : _backgroundType;
                _image.preserveAspect = hasIcon || _backgroundPreserveAspect;
                _image.enabled = hasIcon || (hasText && showBackground && _background != null);
            }

            if (_label != null)
            {
                // LayoutText measures disabled text too; clear it when an icon is shown.
                _label.text = hasText ? text : string.Empty;
                _label.enabled = hasText;
            }
        }

        private void CacheBackground()
        {
            if (_cachedBackground || _image == null)
                return;

            _background = _image.sprite;
            _backgroundType = _image.type;
            _backgroundPreserveAspect = _image.preserveAspect;
            _cachedBackground = true;
        }
    }
}
