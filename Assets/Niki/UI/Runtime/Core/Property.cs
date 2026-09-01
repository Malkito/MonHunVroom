using System;

namespace Niki.UI
{
    /// <summary>
    /// The workhorse of the view model: a value that notifies listeners when it changes.
    /// Writing a value equal to the current one does not raise events (no redundant UI work).
    /// </summary>
    public sealed class Property<T> : IProperty<T>
    {
        private T _value;

        public Property() : this(default)
        {
        }

        public Property(T value)
        {
            _value = value;
        }

        public T Value
        {
            get => _value;
            set => TrySetValue(value);
        }

        public event Action<T> ValueChanged;

        public bool TrySetValue(T value)
        {
            if (System.Object.Equals(_value, value))
            {
                return false;
            }

            _value = value;
            ValueChanged?.Invoke(value);
            return true;
        }
    }

    /// <summary>A bindable value that can be observed but never written.</summary>
#pragma warning disable CS0067
    public sealed class ReadOnlyProperty<T> : IReadOnlyProperty<T>
    {
        public ReadOnlyProperty(T value)
        {
            Value = value;
        }

        public T Value { get; }

        public event Action<T> ValueChanged;
    }
#pragma warning restore CS0067
}
