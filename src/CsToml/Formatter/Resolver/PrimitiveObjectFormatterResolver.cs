
namespace CsToml.Formatter.Resolver;

internal sealed class PrimitiveObjectFormatterResolver : ITomlValueFormatterResolver
{
    public static readonly PrimitiveObjectFormatterResolver Instance = new PrimitiveObjectFormatterResolver();

    public ITomlValueFormatter<T> GetFormatter<T>()
    {
        return (PrimitiveObjectFormatter.Instance as ITomlValueFormatter<T>)!;
    }
}

