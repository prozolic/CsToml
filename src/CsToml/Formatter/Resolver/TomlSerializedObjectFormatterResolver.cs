using CsToml.Error;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CsToml.Formatter.Resolver;

internal sealed class TomlSerializedObjectFormatterResolver : ITomlValueFormatterResolver
{
    public static readonly TomlSerializedObjectFormatterResolver Instance = new ();

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

    public ITomlValueFormatter<T>? GetFormatter<T>()
    {
        return Cache<T>.Formatter;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsRegistered<T>()
        => CacheCheck<T>.Registered;

    public void Register<T>(TomlSerializedObjectFormatter<T> fomatter)
        where T : class, ITomlSerializedObject<T?>
    {
        if (CacheCheck<T>.Registered) return;

        CacheCheck<T>.Registered = true;
        Cache<T>.Formatter = fomatter!;
    }

    public void Register<T>(StructTomlSerializedObjectFormatter<T> fomatter)
        where T : struct, ITomlSerializedObject<T>
    {
        if (CacheCheck<T>.Registered) return;

        CacheCheck<T>.Registered = true;
        Cache<T>.Formatter = fomatter;
    }
}
