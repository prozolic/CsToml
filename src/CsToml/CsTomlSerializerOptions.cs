﻿
namespace CsToml;

public record CsTomlSerializerOptions
{
    public static readonly CsTomlSerializerOptions Default = new() { IsThrowCsTomlException = true };
    public static readonly CsTomlSerializerOptions NoThrow = new() { IsThrowCsTomlException = false };

    public bool IsThrowCsTomlException { get; init; }
}
