
using CsToml.Formatter.Resolver;

namespace CsToml;

public record CsTomlSerializerOptions(ITomlValueFormatterResolver Resolver)
{
    public static readonly CsTomlSerializerOptions Default = new(TomlValueFormatterResolver.Instance);

}