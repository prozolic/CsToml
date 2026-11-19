using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace CsToml.Values;

internal partial class TomlDottedKey
{
    public override bool TryGetString([NotNullWhen(true)] out string? value)
    {
        value = Utf16String;
        return true;
    }

    public override bool TryGetInt64(out long value)
        => long.TryParse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

    public override bool TryGetDouble(out double value)
        => double.TryParse(Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

    public override bool TryGetBool(out bool value)
    {
        if (Value.SequenceEqual("true"u8))
        {
            value = true;
            return true;
        }
        value = false;
        return Value.SequenceEqual("false"u8);
    }

    public override bool TryGetDateTime(out DateTime value)
        => Utf8Parser.TryParse(Value, out value, out _, 'O');

    public override bool TryGetDateTimeOffset(out DateTimeOffset value)
        => Utf8Parser.TryParse(Value, out value, out _, 'O');

    public override bool TryGetDateOnly(out DateOnly value)
    {
        if (TryGetDateTime(out var dateTime))
        {
            value = DateOnly.FromDateTime(dateTime);
            return true;
        }
        value = default;
        return false;
    }

    public override bool TryGetTimeOnly(out TimeOnly value)
    {
        if (TryGetDateTime(out var dateTime))
        {
            value = TimeOnly.FromDateTime(dateTime);
            return true;
        }
        value = default;
        return false;
    }

    public override object GetObject() => bytes.ToArray();
}
