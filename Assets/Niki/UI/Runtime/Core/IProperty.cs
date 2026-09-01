using System;

namespace Niki.UI
{
    /// <summary>A value that can be read and observed but never written.</summary>
    public interface IReadOnlyProperty<T>
    {
        T Value { get; }

        /// <summary>Raised after the value has changed to <paramref name="value"/>.</summary>
        event Action<T> ValueChanged;
    }

    /// <summary>A bindable value: readable, writable, and observable.</summary>
    public interface IProperty<T> : IReadOnlyProperty<T>
    {
        new T Value { get; set; }

        /// <summary>Sets the value when it differs from the current one. Returns false when unchanged.</summary>
        bool TrySetValue(T value);
    }
}
