
namespace CsToml.Extension;

internal static class StringExtensions
{
    public static SplitEnumerator<byte> SplitSpan(this ReadOnlySpan<byte> target, ReadOnlySpan<byte> delimiter)
    {
        return new SplitEnumerator<byte>(target, delimiter);
    }

    public static SplitEnumerator<char> SplitSpan(this string target, ReadOnlySpan<char> delimiter)
    {
        return new SplitEnumerator<char>(target.AsSpan(), delimiter);
    }

    public static SplitEnumerator<char> SplitSpan(this ReadOnlySpan<char> target, ReadOnlySpan<char> delimiter)
    {
        return new SplitEnumerator<char>(target, delimiter);
    }

    internal ref struct SplitEntry<T>(ReadOnlySpan<T> value)
        where T : struct
    {
        public ReadOnlySpan<T> Value = value;
    }

    internal ref struct SplitEnumerator<T>(ReadOnlySpan<T> target, ReadOnlySpan<T> delimiter)
        where T : struct, IEquatable<T>?
    {
        private ReadOnlySpan<T> targetSpan = target;
        private ReadOnlySpan<T> delimiterSpan = delimiter;

        public SplitEntry<T> Current { get; private set; } = default;

        public readonly SplitEnumerator<T> GetEnumerator() => this;

        public bool MoveNext()
        {
            var target = targetSpan;
            if (target.Length == 0) return false;

            var index = target.IndexOf(delimiterSpan);
            if (index < 0) // end
            {
                Current = new SplitEntry<T>(target);
                targetSpan = [];
                return true;
            }

            Current = new SplitEntry<T>(target[..index]);
            targetSpan = target[++index..];

            return true;
        }

    }

}

