
using System.Diagnostics.CodeAnalysis;

namespace CsToml.Values;

internal partial class CsTomlBool : ISpanFormattable
{
    public override string GetString() => Value ? bool.TrueString : bool.FalseString;

    public override long GetInt64() => Value ? 1 : 0;

    public override double GetDouble() => Value ? 1d : 0d;

    public override bool GetBool() => Value;

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        var destinationSize = Value ? 4 : 5;
        if (destination.Length < destinationSize)
        {
            charsWritten = 0;
            return false;
        }

        (Value ? bool.TrueString : bool.FalseString).TryCopyTo(destination);
        charsWritten = destinationSize;
        return true;
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        Span<char> destination = Value ? stackalloc char[4] : stackalloc char[5];
        TryFormat(destination, out var charsWritten, format.AsSpan(), formatProvider);
        return destination.ToString();
    }
}

