
namespace CsToml.Error;

public sealed class CsTomlSerializeException : Exception
{
    public IReadOnlyCollection<CsTomlException> Exceptions { get; }

    internal CsTomlSerializeException(IReadOnlyCollection<CsTomlException> exceptions) : base("An error occurred while parsing the TOML file.")
    {
        Exceptions = exceptions;
    }

}

