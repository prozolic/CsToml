
namespace CsToml.Values;

internal interface ITomlStringCreator<T>
    where T : TomlValue
{
    static abstract T CreateString(ReadOnlySpan<byte> value, CsTomlStringType type = CsTomlStringType.Basic);
}
