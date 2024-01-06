
namespace CsToml.Error;

public class CsTomlException : Exception
{
    internal CsTomlException(string? message) : base(message)
    {}

    internal CsTomlException(string? message, CsTomlException innerException) : base(message, innerException)
    {}
}

    

