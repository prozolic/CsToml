
namespace CsToml.Values;

internal interface ITomlStringParser<T>
    where T : TomlValue
{
    static abstract T Parse(ReadOnlySpan<byte> value, CsTomlStringType type = CsTomlStringType.Basic);
}
