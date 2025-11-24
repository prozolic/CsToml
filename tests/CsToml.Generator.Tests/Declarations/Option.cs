
namespace CsToml.Generator.Tests;

public static class Option
{
    public static CsTomlSerializerOptions Header { get; set; } = CsTomlSerializerOptions.Default with
    {
        SerializeOptions = new() { TableStyle = TomlTableStyle.Header }
    };

    public static CsTomlSerializerOptions ArrayHeader { get; set; } = CsTomlSerializerOptions.Default with
    {
        SerializeOptions = new() { ArrayStyle = TomlArrayStyle.Header }
    };

    public static CsTomlSerializerOptions AllHeader { get; set; } = CsTomlSerializerOptions.Default with
    {
        SerializeOptions = new() { TableStyle = TomlTableStyle.Header, ArrayStyle = TomlArrayStyle.Header }
    };
}