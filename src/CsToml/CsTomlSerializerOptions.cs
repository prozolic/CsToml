
namespace CsToml;

public record CsTomlSerializerOptions
{
    public static readonly CsTomlSerializerOptions Default = new() { IsThrowCsTomlException = true };
    public static readonly CsTomlSerializerOptions NoThrow = new() { IsThrowCsTomlException = false};

    public bool IsThrowCsTomlException { get; init; }

    public static CsTomlSerializerOptions CreateOptions(bool isThrowCsTomlException = true)
        => new() { IsThrowCsTomlException = isThrowCsTomlException};
}