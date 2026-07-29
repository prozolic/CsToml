using System.Collections.Immutable;

namespace CsToml.Generator.Internal;

internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>
    where T : IEquatable<T>
{
    public static readonly EquatableArray<T> Empty = new(ImmutableArray<T>.Empty);

    public ImmutableArray<T> Values { get; }

    public int Length => Values.IsDefault ? 0 : Values.Length;

    public EquatableArray(ImmutableArray<T> values)
    {
        Values = values;
    }

    public bool Equals(EquatableArray<T> other)
    {
        var left = Values;
        var right = other.Values;

        if (left.IsDefaultOrEmpty)
        {
            return right.IsDefaultOrEmpty;
        }
        if (right.IsDefaultOrEmpty)
        {
            return false;
        }
        if (left.Length != right.Length)
        {
            return false;
        }

        for (var i = 0; i < left.Length; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(left[i], right[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
        => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        if (Values.IsDefaultOrEmpty)
            return 0;

        var hashCode = 17;
        foreach (var value in Values)
        {
            hashCode = unchecked(hashCode * 31 + (value?.GetHashCode() ?? 0));
        }

        return hashCode;
    }

    public ImmutableArray<T>.Enumerator GetEnumerator()
        => (Values.IsDefault ? ImmutableArray<T>.Empty : Values).GetEnumerator();
}
