﻿
using CsToml.Error;

namespace CsToml.Formatter.Resolver;

public sealed class TomlValueFormatterResolver
{
    private sealed class Cache<T>
    {
        public static ITomlValueFormatter<T>? Formatter;

        static Cache()
        {
            if (typeof(T) == typeof(object))
            {
                Formatter = PrimitiveObjectFormatterResolver.GetFormatter<T>();
                return;
            }
            else
            {
                var formatter = BuildinFormatterResolver.GetFormatter<T>();
                if (formatter != null)
                {
                    Formatter = formatter;
                    return;
                }

                var serializedObjectformatter = TomlSerializedObjectFormatterResolver.GetFormatter<T>();
                if (serializedObjectformatter != null)
                {
                    Formatter = serializedObjectformatter;
                    return;
                }
            }
            Formatter = null;
        }
    }

    public static ITomlValueFormatter<T> GetFormatter<T>()
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