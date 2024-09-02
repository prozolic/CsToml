
using CsToml.Error;

namespace CsToml.Formatter.Resolver;

public interface ITomlValueFormatterResolver
{
    ITomlValueFormatter<T>? GetFormatter<T>();
}

internal sealed class TomlValueFormatterResolver : ITomlValueFormatterResolver
{
    private sealed class Cache<T>
    {
        public static ITomlValueFormatter<T>? Formatter;

        static Cache()
        {
            if (typeof(T) == typeof(object))
            {
                Formatter = PrimitiveObjectFormatterResolver.Instance.GetFormatter<T>();
                return;
            }
            else
            {
                var formatter = BuildinFormatterResolver.Instance.GetFormatter<T>();
                if (formatter != null)
                {
                    Formatter = formatter;
                    return;
                }

                var serializedObjectformatter = TomlSerializedObjectFormatterResolver.Instance.GetFormatter<T>();
                if (serializedObjectformatter != null)
                {
                    Formatter = serializedObjectformatter;
                    return;
                }
            }
            Formatter = null;
        }
    }

    private sealed class TomlDocumentFormatterCache
    {
        private static readonly TomlSerializedObjectFormatter<TomlDocument> tomlDocumentFormatter = new TomlSerializedObjectFormatter<TomlDocument>();

        public static ITomlValueFormatter<T>? GetFormatter<T>()
        {
            return tomlDocumentFormatter as ITomlValueFormatter<T>;
        }
    }

    public static readonly TomlValueFormatterResolver Instance = new TomlValueFormatterResolver();

    internal ITomlValueFormatter<T> GetFormatterForInternal<T>()
    {
        // get the TomlSerializedObjectFormatter<TomlDocument>
        var formatter = TomlDocumentFormatterCache.GetFormatter<T>();
        if (formatter != null)
        {
            return formatter;
        }

        return GetFormatter<T>()!;
    }

    public ITomlValueFormatter<T>? GetFormatter<T>()
    {
        try
        {
            var formatter = Cache<T>.Formatter;
            if (formatter != null)
            {
                return formatter;
            }
        }
        catch (TypeInitializationException e)
        {
            ExceptionHelper.ThrowException(e.ToString(), e);
            return default;
        }

        ExceptionHelper.ThrowNotRegisteredInResolver<T>();
        return default;
    }
}