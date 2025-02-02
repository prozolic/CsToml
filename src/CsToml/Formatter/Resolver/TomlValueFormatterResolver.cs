
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
            else if (typeof(T).IsEnum)
            {
                var enumFormatterType = typeof(EnumFormatter<>).MakeGenericType(typeof(T));
                Formatter = Activator.CreateInstance(enumFormatterType) as ITomlValueFormatter<T>;
                return;
            }
            else
            {
                var formatter = BuiltinFormatterResolver.Instance.GetFormatter<T>();
                if (formatter != null)
                {
                    Formatter = formatter;
                    return;
                }

                var generalizedFormatter = GeneratedFormatterResolver.Instance.GetFormatter<T>();
                if (generalizedFormatter != null)
                {
                    Formatter = generalizedFormatter;
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

    public static readonly TomlValueFormatterResolver Instance = new TomlValueFormatterResolver();

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
