using UnityEngine;
using UnityEngine.UI;

namespace Niki.UI
{
    /// <summary>
    /// Radial cooldown display. Draws a circular "remaining cooldown" wedge over an
    /// ability icon (via the NikiUI/RadialCooldown shader on its Image material).
    ///
    /// Bound value semantics (0..1):
    ///   1.0 = just used  -> the full circle is dimmed
    ///   0.0 = ready      -> fully clear
    /// The wedge starts at 12 o'clock and covers clockwise; it shrinks toward 12 o'clock
    /// as the cooldown finishes.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class BindableRadialCooldown : BindableElement<float>
    {
        /// <summary>Shader property that controls the covered fraction of the circle.</summary>
        public const string FillProperty = "_Fill";

        [SerializeField, HideInInspector] private Image _image;
        [SerializeField] private Material _radialMaterial;

        private void Awake()
        {
            if (_image == null)
                _image = GetComponent<Image>();

            EnsureMaterial();
        }

        /// <summary>Point this element at a specific Image (used by editor tooling).</summary>
        public void SetImage(Image image)
        {
            _image = image;
        }

        /// <summary>Assign the radial shader material (falls back to creating one if not assigned).</summary>
        public void SetRadialMaterial(Material material)
        {
            _radialMaterial = material;
            if (_image != null)
                _image.material = _radialMaterial;
        }

        protected override void Apply(float remaining)
        {
            if (_radialMaterial == null)
                return;

            remaining = Mathf.Clamp01(remaining);
            _radialMaterial.SetFloat(FillProperty, remaining);
        }

        private void EnsureMaterial()
        {
            if (_radialMaterial == null)
            {
                var shader = Shader.Find("NikiUI/RadialCooldown");
                if (shader == null)
                {
                    Debug.LogError("[Niki.UI] Shader 'NikiUI/RadialCooldown' not found. " +
                                   "Create a material from it and assign it via SetRadialMaterial().");
                    return;
                }

                _radialMaterial = new Material(shader);
            }

            if (_image != null)
                _image.material = _radialMaterial;
        }
    }
}
