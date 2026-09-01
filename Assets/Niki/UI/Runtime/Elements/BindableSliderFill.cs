using UnityEngine;
using UnityEngine.UI;

namespace Niki.UI
{
    /// <summary>Binds a Property&lt;float&gt; to a Slider's fill value (e.g. health, mana, boost bars).</summary>
    [RequireComponent(typeof(Slider))]
    public class BindableSliderFill : BindableElement<float>
    {
        [SerializeField, HideInInspector] private Slider _slider;

        private void Awake()
        {
            if (_slider == null)
                _slider = GetComponent<Slider>();
        }

        /// <summary>Point this element at a specific Slider (used by editor tooling).</summary>
        public void SetSlider(Slider slider)
        {
            _slider = slider;
        }

        protected override void Apply(float value)
        {
            if (_slider != null)
                _slider.value = value;
        }
    }
}
