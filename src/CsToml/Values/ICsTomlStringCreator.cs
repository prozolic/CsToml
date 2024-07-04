
namespace CsToml.Values;

internal interface ICsTomlStringCreator<T>
    where T : CsTomlValue
{
    static abstract T CreateString(ReadOnlySpan<byte> value, CsTomlStringType type = CsTomlStringType.Basic);
}
