
// init accessors are not available in .NET Standard 2.0,
// so we need to define this type to avoid compilation errors when using C# 9.0 or later features like init-only properties.

namespace System.Runtime.CompilerServices
{
    internal sealed class IsExternalInit
    { }
}