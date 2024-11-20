
using CsToml.Formatter.Resolver;

namespace CsToml;

public enum TomlTableStyle
{
    Default = 0,
    DottedKey = Default,
    Header = 1
}

public record CsTomlSerializerOptions(ITomlValueFormatterResolver Resolver)
{
    public static readonly CsTomlSerializerOptions Default = new(TomlValueFormatterResolver.Instance) { };

    public SerializeOptions SerializeOptions { get; set; } = new ();
}


public record SerializeOptions
{
    public TomlTableStyle TableStyle { get; set; } = TomlTableStyle.Default;
}