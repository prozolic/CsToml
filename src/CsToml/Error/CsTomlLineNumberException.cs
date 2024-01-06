﻿
namespace CsToml.Error;

public sealed class CsTomlLineNumberException : CsTomlException
{
    public long LineNumber { get; } = 0;

    internal CsTomlLineNumberException(string? message, long lineNumber) : base(message)
    {
        LineNumber = lineNumber;
    }

    internal CsTomlLineNumberException(CsTomlException exception, long lineNumber) : base("An error occurred when parsing the TOML file.", exception)
    {
        LineNumber = lineNumber;
    }
}
