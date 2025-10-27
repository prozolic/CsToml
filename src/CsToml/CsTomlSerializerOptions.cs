
using CsToml.Formatter.Resolver;

namespace CsToml;

public enum TomlTableStyle
{
    Default = 0,
    DottedKey = Default,
    Header = 1
}

public enum TomlIgnoreCondition
{
    Never = 0,
    Always = 1,
    WhenWritingNull = 2
}

public enum TomlNullBehavior
{
    Default = 0,
    Skip = 1
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

    public TomlNullBehavior DefaultNullBehavior { get; init; } = TomlNullBehavior.Default;
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
