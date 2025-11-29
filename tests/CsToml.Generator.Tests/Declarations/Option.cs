
namespace CsToml.Generator.Tests;

public static class Option
{
    public static CsTomlSerializerOptions Header { get; } = CsTomlSerializerOptions.Default with
    {
        SerializeOptions = new() { TableStyle = TomlTableStyle.Header }
    };

    public static CsTomlSerializerOptions ArrayHeader { get; } = CsTomlSerializerOptions.Default with
    {
        SerializeOptions = new() { ArrayStyle = TomlArrayStyle.Header }
    };

    public static CsTomlSerializerOptions HeaderAndArrayHeader { get; } = CsTomlSerializerOptions.Default with
    {
        SerializeOptions = new() { TableStyle = TomlTableStyle.Header, ArrayStyle = TomlArrayStyle.Header }
    };

    public static CsTomlSerializerOptions ErrorTomlNullHandling { get; } = CsTomlSerializerOptions.Default with
    {
        SerializeOptions = new() { DefaultNullHandling = TomlNullHandling.Error }
    };

    public static CsTomlSerializerOptions IgnoreTomlNullHandling { get; } = CsTomlSerializerOptions.Default with
    {
        SerializeOptions = new() { DefaultNullHandling = TomlNullHandling.Ignore }
    };
}