
using CsToml.Formatter.Resolver;

namespace CsToml;

public enum TomlTableStyle
{
    Default = 0,
    DottedKey = Default,
    Header = 1
}

public enum TomlArrayStyle
{
    Default = 0,        // array or inline table style (one line)
    Header = 1          // array of table style (multiple lines)
}

public enum TomlArrayOfTableSourceGenerationMode
{
    Default = 0,        // array or inline table (one line)
    ArrayOfTable = 1    // array of table (multiple lines)
}

public enum TomlNullHandling
{
    Error = 0,
    Ignore = 1
}

public record CsTomlSerializerOptions(ITomlValueFormatterResolver Resolver)
{
    public static readonly CsTomlSerializerOptions Default = new(TomlValueFormatterResolver.Instance) { };

    public SerializeOptions SerializeOptions { get; init; } = new();

    public TomlSpec Spec { get; init; } = new();
}

public record SerializeOptions
{
    public TomlTableStyle TableStyle { get; init; } = TomlTableStyle.Default;

    public TomlArrayStyle ArrayStyle { get; init; } = TomlArrayStyle.Default;

    public TomlNullHandling DefaultNullHandling { get; init; } = TomlNullHandling.Error;
}

public record TomlSpec
{
    #region "TOML v1.1.0 Preview Feature"

    public bool AllowUnicodeInBareKeys { get; init; }

    public bool AllowNewlinesInInlineTables { get; init; }

    public bool AllowTrailingCommaInInlineTables { get; init; }

    public bool SupportsEscapeSequenceE { get; init; }

    public bool AllowSecondsOmissionInTime { get; init; }

    public bool SupportsEscapeSequenceX { get; init; }

    #endregion
}
