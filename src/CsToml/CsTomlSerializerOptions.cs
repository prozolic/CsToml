
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

    public SerializeOptions SerializeOptions { get; init; } = new ();

    public TomlSpec Spec { get; init; } = new ();
}

public record SerializeOptions
{
    public TomlTableStyle TableStyle { get; init; } = TomlTableStyle.Default;
}

public record TomlSpec
{
    #region "TOML v1.1.0 Preview Feature"

    public bool AllowNewlinesInInlineTables { get; init; }

    public bool AllowTrailingCommaInInlineTables { get; init; }

    public bool SupportsEscapeSequenceE { get; init; }

    #endregion
}

