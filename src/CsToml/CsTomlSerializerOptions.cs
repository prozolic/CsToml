
namespace CsToml;

public record CsTomlSerializerOptions
{
    public static readonly CsTomlSerializerOptions Default = new() { IsThrowCsTomlException = true, NewLine = NewLineOption.Default };
    public static readonly CsTomlSerializerOptions NoThrow = new() { IsThrowCsTomlException = false, NewLine = NewLineOption.Default };

    public bool IsThrowCsTomlException { get; init; }

    public NewLineOption NewLine { get; init; }

    public static CsTomlSerializerOptions CreateOptions(bool isThrowCsTomlException = true, NewLineOption newLine = NewLineOption.Default)
        => new() { IsThrowCsTomlException = isThrowCsTomlException, NewLine = newLine };
}

public enum NewLineOption : byte
{
    Default,
    Lf,
    CrLf
}