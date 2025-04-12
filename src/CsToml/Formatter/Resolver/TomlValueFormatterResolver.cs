
using CsToml.Error;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CsToml.Formatter.Resolver;

public interface ITomlValueFormatterResolver
{
    ITomlValueFormatter<T>? GetFormatter<T>();
}

public sealed class TomlValueFormatterResolver : ITomlValueFormatterResolver
{
    public static readonly TomlValueFormatterResolver Instance = new TomlValueFormatterResolver();

    private sealed class CacheCheck<T>
    {
        public static bool Registered;
    }

    private sealed class Cache<T>
    {
        public static ITomlValueFormatter<T>? Formatter;

        static Cache()
        {
            if (CacheCheck<T>.Registered) return;

            if (typeof(T) == typeof(object))
            {
                CacheCheck<T>.Registered = true;
                Formatter = PrimitiveObjectFormatterResolver.Instance.GetFormatter<T>();
                return;
            }
            else
            {
                var formatter = BuiltinFormatterResolver.Instance.GetFormatter<T>();
                if (formatter != null)
                {
                    CacheCheck<T>.Registered = true;
                    Formatter = formatter;
                    return;
                }

                var serializedObjectformatter = TomlSerializedObjectFormatterResolver.Instance.GetFormatter<T>();
                if (serializedObjectformatter != null)
                {
                    CacheCheck<T>.Registered = true;
                    Formatter = serializedObjectformatter;
                    return;
                }
            }
            Formatter = null;
        }
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRegistered<T>()
    {
        // If the static constructor in BuiltinFormatterResolver is not called, call it here.
        if (BuiltinFormatterResolver.Instance.IsRegistered<T>()) return true;
        if (TomlSerializedObjectFormatterResolver.Instance.IsRegistered<T>()) return true;

        return CacheCheck<T>.Registered;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Register<T>(ITomlValueFormatter<T> formatter)
    {
        if (IsRegistered<T>()) return;

        CacheCheck<T>.Registered = true;
        Cache<T>.Formatter = formatter;
    }

    public static void Register<T>(TomlSerializedObjectFormatter<T> formatter)
        where T : ITomlSerializedObject<T>
    {
        TomlSerializedObjectFormatterResolver.Instance.Register(formatter);
    }

    public static void Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>()
        where T : ITomlSerializedObjectRegister
    {
        T.Register();
    }
}
