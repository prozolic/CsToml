using CsToml.Error;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CsToml.Formatter.Resolver;

internal static class TomlSerializedObjectFormatterResolver
{
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2090",
        Justification = "If T implements CsToml.ITomlSerializedObjectRegister.Register, it is executable.")]
    public static bool TryRegister<T>()
    {
        if (!typeof(ITomlSerializedObjectRegister).IsAssignableFrom(typeof(T)))
        {
            return false;
        }

        var m = typeof(T).GetMethod("CsToml.ITomlSerializedObjectRegister.Register",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (m == null)
        {
            ExceptionHelper.ThrowException($"{typeof(T).FullName} implements ITomlSerializedObjectRegister, but Register is not found.");
        }
        m.Invoke(null, null);
        return true;
    }
}
