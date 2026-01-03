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

    public TomlSpec Spec { get; init; } = TomlSpec.Version100;
}

public record SerializeOptions
{
    public TomlTableStyle TableStyle { get; init; } = TomlTableStyle.Default;

    public TomlArrayStyle ArrayStyle { get; init; } = TomlArrayStyle.Default;

    public TomlNullHandling DefaultNullHandling { get; init; } = TomlNullHandling.Error;
}

public record TomlSpec
{
    /// <summary>
    /// TOML v1.0.0 Specification
    /// </summary>
    public static readonly TomlSpec Version100 = new() {};

    /// <summary>
    /// TOML v1.1.0 Specification
    /// </summary>
    public static readonly TomlSpec Version110 = new() {
        AllowNewlinesInInlineTables = true,
        AllowTrailingCommaInInlineTables = true,
        SupportsEscapeSequenceE = true,
        AllowSecondsOmissionInTime = true,
        SupportsEscapeSequenceX = true,
    };

    #region "TOML v1.1.0"

    /// <summary>
    /// Gets a value indicating whether newlines are permitted within TOML inline tables.
    /// </summary>
    public bool AllowNewlinesInInlineTables { get; init; }

    /// <summary>
    /// Gets a value indicating whether a trailing comma is permitted in TOML inline tables.
    /// </summary>
    public bool AllowTrailingCommaInInlineTables { get; init; }

    /// <summary>
    /// Gets a value indicating whether the escape sequence 'E' is supported by the current implementation.
    /// </summary>
    public bool SupportsEscapeSequenceE { get; init; }

    /// <summary>
    /// Gets a value indicating whether time values are allowed to omit seconds when formatted or parsed.
    /// </summary>
    public bool AllowSecondsOmissionInTime { get; init; }

    /// <summary>
    /// Gets a value indicating whether escape sequence X is supported by the current implementation.
    /// </summary>
    public bool SupportsEscapeSequenceX { get; init; }

    #endregion

    #region "TOML unofficial feature"

    /// <summary>
    /// <para>Gets a value indicating whether Unicode characters are allowed in bare keys.</para>
    /// <para>This is an unofficial extension to the TOML specification and may not be supported by all TOML parsers.</para>
    /// </summary>
    public bool AllowUnicodeInBareKeys { get; init; }

    #endregion
}
