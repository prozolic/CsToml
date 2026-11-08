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
    public static readonly TomlValueFormatterResolver Instance = new ();

    private static class Cache<T>
    {
        public static ITomlValueFormatter<T>? Formatter;
    }

    private static class Resolved<T>
    {
        public static readonly ITomlValueFormatter<T>? Formatter;

        static Resolved()
        {
            var formatter = BuiltinFormatterResolver.Instance.GetFormatter<T>();
            if (formatter == null && TomlSerializedObjectFormatterResolver.TryRegister<T>())
            {
                formatter = Cache<T>.Formatter;
            }
            Formatter = formatter;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ITomlValueFormatter<T>? GetFormatter<T>()
    {
        // Check Cache first to avoid the overhead of exception handling in Resolved<T> static constructor.
        var formatter = Cache<T>.Formatter;
        if (formatter != null)
        {
            return formatter;
        }
        return GetFormatterSlow<T>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ITomlValueFormatter<T> GetFormatterSlow<T>()
    {
        ITomlValueFormatter<T>? resolved;
        try
        {
            resolved = Resolved<T>.Formatter;
        }
        catch (TypeInitializationException e)
        {
            ExceptionHelper.ThrowException(e.ToString(), e);
            return default!;
        }

        var formatter = CompareExchangeFormatter(resolved);
        if (formatter == null)
        {
            ExceptionHelper.ThrowNotRegisteredInResolver<T>();
        }
        return formatter;
    }

    private static ITomlValueFormatter<T>? CompareExchangeFormatter<T>(ITomlValueFormatter<T>? resolved)
    {
        if (resolved != null)
        {
            Interlocked.CompareExchange(ref Cache<T>.Formatter, resolved, null);
        }
        return Cache<T>.Formatter;
    }

    public static bool IsRegistered<T>()
    {
        if (Cache<T>.Formatter != null)
        {
            return true;
        }

        ITomlValueFormatter<T>? resolved;
        try
        {
            resolved = Resolved<T>.Formatter;
        }
        catch (TypeInitializationException)
        {
            return Cache<T>.Formatter != null;
        }
        return CompareExchangeFormatter(resolved) != null;
    }

    public static void Register<T>(ITomlValueFormatter<T> formatter)
    {
        // Built-in formatters cannot be replaced.
        // otherwise the first registration wins.
        if (Cache<T>.Formatter != null)
        {
            return;
        }

        var builtin = BuiltinFormatterResolver.Instance.GetFormatter<T>();
        Interlocked.CompareExchange(ref Cache<T>.Formatter, builtin ?? formatter, null);
    }

    public static void Register<T, TFormatter>()
        where TFormatter : class, ITomlValueFormatter<T>, new()
    {
        // TFormatter is only instantiated while T has no formatter yet,
        // so generated Register() bodies can call this unconditionally.
        if (Cache<T>.Formatter != null)
        {
            return;
        }

        var builtin = BuiltinFormatterResolver.Instance.GetFormatter<T>();
        Interlocked.CompareExchange(ref Cache<T>.Formatter, builtin ?? new TFormatter(), null);
    }

    public static void Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>()
        where T : ITomlSerializedObjectRegister
    {
        if (Cache<T>.Formatter == null)
        {
            T.Register();
        }
    }

    private static class Registering<T>
    {
        [ThreadStatic]
        public static bool InProgress;
    }

    public static bool BeginRegistration<T>()
    {
        // Generated Register() bodies start here and publish T last, after the formatters of its members.
        // False once T is registered, or while this thread is already inside T.Register() (type cycles).
        if (Cache<T>.Formatter != null || Registering<T>.InProgress)
        {
            return false;
        }

        Registering<T>.InProgress = true;
        return true;
    }
}
