
namespace CsToml.Formatter.Resolver;

internal sealed class PrimitiveObjectFormatterResolver
{
    public static ITomlValueFormatter<T> GetFormatter<T>()
    {
        return (PrimitiveObjectFormatter.Instance as ITomlValueFormatter<T>)!;
    }
}

