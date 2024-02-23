
namespace CsToml.Error;

public class CsTomlException : Exception
{
    internal CsTomlException(string? message) : base(message)
    {}

    internal CsTomlException(string? message, Exception? innerException) : base(message, innerException)
    {}
}

    

