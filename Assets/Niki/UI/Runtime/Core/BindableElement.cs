using System;
using UnityEngine;

namespace Niki.UI
{
    /// <summary>
    /// Base class for single-element widgets: one element renders one view-model property.
    ///
    /// Subclasses implement <see cref="Apply"/> and describe how a value is rendered.
    /// The base class handles the subscription lifecycle, so elements self-clean up
    /// when their GameObject is destroyed.
    ///
    /// This is the modular unit of the framework: new UI capabilities are added by
    /// writing new BindableElement&lt;T&gt; subclasses and reusing existing ones.
    /// </summary>
    public abstract class BindableElement<T> : MonoBehaviour
    {
        private IReadOnlyProperty<T> _source;
        private Action<T> _handler;

        /// <summary>The property currently bound, or null when unbound.</summary>
        public IReadOnlyProperty<T> Source => _source;

        public bool IsBound => _source != null;

        /// <summary>Render <paramref name="value"/> on this element.</summary>
        protected abstract void Apply(T value);

        /// <summary>
        /// Bind to a view-model property. The current value is applied immediately,
        /// and the element updates automatically on every subsequent change.
        /// </summary>
        public void Bind(IReadOnlyProperty<T> source)
        {
            Unbind();
            if (source == null) return;

            _source = source;
            _handler = Apply;
            _source.ValueChanged += _handler;
            Apply(_source.Value);
        }

        /// <summary>Detach from the bound property.</summary>
        public void Unbind()
        {
            if (_source == null) return;

            _source.ValueChanged -= _handler;
            _source = null;
        }

        private void OnDestroy() => Unbind();
    }
}
