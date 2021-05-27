
using System;

namespace Biscuit
{
    public struct Optional<T>
    {
        T Value;

        internal bool isPresent()
        {
            return Value != null;
        }

        internal static Optional<T> empty()
        {
            return new Optional<T>() { Value = default };
        }

        internal static Optional<T> of(T variables)
        {
            return new Optional<T>() { Value = variables };
        }

        internal T get()
        {
            return Value;
        }
    }
}
