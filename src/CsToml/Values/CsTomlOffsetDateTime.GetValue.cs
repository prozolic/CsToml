
namespace CsToml.Values;

internal partial class CsTomlOffsetDateTime
{
    public override string GetString()
    {
        Span<char> destination = stackalloc char[32];
        TryFormat(destination, out var charsWritten, null, null);
        return destination[..charsWritten].ToString();
    }

    public override DateTime GetDateTime() => Value.DateTime;

    public override DateTimeOffset GetDateTimeOffset() => Value;

    public override DateOnly GetDateOnly() => DateOnly.FromDateTime(Value.DateTime);

    public override TimeOnly GetTimeOnly() => TimeOnly.FromDateTime(Value.DateTime);

}

