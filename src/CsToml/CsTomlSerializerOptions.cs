
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

    public SerializeOptions SerializeOptions { get; set; } = SerializeOptions.Default;
}


public record SerializeOptions
{
    public static readonly SerializeOptions Default = new() { TableStyle = TomlTableStyle.Default };

    public TomlTableStyle TableStyle { get; set; }
}