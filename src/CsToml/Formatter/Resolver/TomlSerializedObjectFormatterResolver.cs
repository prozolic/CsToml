using CsToml.Error;
using System.Reflection;

namespace CsToml.Formatter.Resolver;

public sealed class TomlSerializedObjectFormatterResolver : ITomlValueFormatterResolver
{
    private sealed class CacheCheck<T>
    {
        public static bool Registered;
    }

    private sealed class Cache<T>
    {
        public static ITomlValueFormatter<T>? Formatter;

        static Cache()
        {
            if (typeof(ITomlSerializedObjectRegister).IsAssignableFrom(typeof(T)))
            {
                var m = typeof(T).GetMethod("CsToml.ITomlSerializedObjectRegister.Register",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (m == null)
                {
                    ExceptionHelper.ThrowException($"{typeof(T).FullName} implements ITomlSerializedObjectRegister, but Register is not found.");
                }
                m.Invoke(null, null);
            }
        }
    }

    public static readonly TomlSerializedObjectFormatterResolver Instance = new TomlSerializedObjectFormatterResolver();

    public ITomlValueFormatter<T>? GetFormatter<T>()
    {
        return Cache<T>.Formatter;
    }

    public static void Register<T>(TomlSerializedObjectFormatter<T> fomatter)
        where T : ITomlSerializedObject<T>
    {
        if (CacheCheck<T>.Registered) return;

        CacheCheck<T>.Registered = true;
        Cache<T>.Formatter = fomatter;
    }
}
