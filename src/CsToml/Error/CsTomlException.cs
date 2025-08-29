
using System.Runtime.CompilerServices;

namespace CsToml.Error;

public class CsTomlException : Exception
{
    public CsTomlException(string? message) : base(message)
    {}

    internal CsTomlException(string? message, Exception? innerException) : base(message, innerException)
    {}
}

public sealed class CsTomlParseException : CsTomlException
{
    public long LineNumber { get; } = 0;

    internal CsTomlParseException(string? message, long lineNumber) : base(message)
    {
        LineNumber = lineNumber;
    }

    internal CsTomlParseException(CsTomlException exception, long lineNumber)
        : base(CreateCsTomlExceptionMessage(exception, lineNumber), exception)
    {
        LineNumber = lineNumber;
    }

    internal CsTomlParseException(Exception exception, long lineNumber)
        : base(CreateExceptionMessage(exception, lineNumber), exception)
    {
        LineNumber = lineNumber;
    }

    internal static string CreateCsTomlExceptionMessage(CsTomlException e, long lineNumber)
    {
        var handler = new DefaultInterpolatedStringHandler(0, 0);
        handler.AppendLiteral("A syntax error (");
        handler.AppendLiteral(nameof(CsTomlException));
        handler.AppendLiteral(") was thrown while parsing line ");
        handler.AppendFormatted(lineNumber);
        handler.AppendLiteral($".");
        handler.AppendLiteral(Environment.NewLine);
        handler.AppendFormatted(e);
        return handler.ToStringAndClear();
    }

    internal static string CreateExceptionMessage(Exception e, long lineNumber)
    {
        var handler = new DefaultInterpolatedStringHandler(0, 0);
        handler.AppendLiteral("An unexpected exception (");
        handler.AppendLiteral(e.GetType().Name);
        handler.AppendLiteral(") was thrown while parsing line ");
        handler.AppendFormatted(lineNumber);
        handler.AppendLiteral($".");
        handler.AppendLiteral(Environment.NewLine);
        handler.AppendFormatted(e);
        return handler.ToStringAndClear();
    }
}

public sealed class CsTomlSerializeException : CsTomlException
{
    public IReadOnlyCollection<CsTomlParseException>? ParseExceptions { get; }

    internal CsTomlSerializeException(string message, IReadOnlyCollection<CsTomlParseException> parseExceptions) :
        base(message)
    {
        ParseExceptions = parseExceptions;
    }

    internal CsTomlSerializeException(string message, CsTomlException e) :
        base(message, e)
    {
        ParseExceptions = [];
    }

}


