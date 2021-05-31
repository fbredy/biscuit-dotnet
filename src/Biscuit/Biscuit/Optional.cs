namespace Biscuit
{
    public struct Optional<T>
    {
        T Value;

        internal bool IsPresent()
        {
            return Value != null;
        }

        internal static Optional<T> Empty()
        {
            return new Optional<T>() { Value = default };
        }

        internal static Optional<T> Of(T variables)
        {
            return new Optional<T>() { Value = variables };
        }

        internal T Get()
        {
            return Value;
        }
    }
}
