
namespace CsToml.Values;

internal partial class CsTomlInt64
{
    public override string GetString()
    {
        Span<char> destination = stackalloc char[32];
        TryFormat(destination, out var charsWritten, null, null);
        return destination[..charsWritten].ToString();
    }

    public override long GetInt64() => Value;

    public override double GetDouble() => Value;

    public override bool GetBool() => Value != 0;
}

