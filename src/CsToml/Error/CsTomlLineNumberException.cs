
using System.Runtime.CompilerServices;

namespace CsToml.Error;

public sealed class CsTomlLineNumberException : CsTomlException
{
    public long LineNumber { get; } = 0;

    internal CsTomlLineNumberException(string? message, long lineNumber) : base(message)
    {
        LineNumber = lineNumber;
    }

    internal CsTomlLineNumberException(CsTomlException exception, long lineNumber) 
        : base(CreateCsTomlExceptionMessage(exception, lineNumber), exception)
    {
        LineNumber = lineNumber;
    }

    internal CsTomlLineNumberException(Exception exception, long lineNumber)
        : base(CreateExceptionMessage(exception, lineNumber), exception)
    {
        LineNumber = lineNumber;
    }

    internal static string CreateCsTomlExceptionMessage(CsTomlException e, long lineNumber)
    {
        var handler = new DefaultInterpolatedStringHandler(0, 0);
        handler.AppendLiteral("A syntax error (");
        handler.AppendLiteral(nameof(CsTomlException));
        handler.AppendLiteral(") occurred while parsing line ");
        handler.AppendFormatted(lineNumber);
        handler.AppendLiteral(" of the TOML file. Check InnerException for details.");
        return handler.ToStringAndClear();
    }

    internal static string CreateExceptionMessage(Exception e, long lineNumber)
    {
        var handler = new DefaultInterpolatedStringHandler(0, 0);
        handler.AppendLiteral("An unexpected error (");
        handler.AppendLiteral(e.GetType().Name);
        handler.AppendLiteral(") occurred while parsing line ");
        handler.AppendFormatted(lineNumber);
        handler.AppendLiteral(" of the TOML file. Check InnerException for details.");
        return handler.ToStringAndClear();
    }
}
