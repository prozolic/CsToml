#pragma warning disable CS8618

using CsToml;
using System.Runtime.CompilerServices;

#if !NET11_0_OR_GREATER

namespace System.Runtime.CompilerServices
{
    // Polyfill: net10.0 has no UnionAttribute; the generator detects it by name.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    internal sealed class UnionAttribute : Attribute
    { }
}

#endif

namespace ConsoleNativeAOT
{
    // Custom union pinned to its enum case: on NativeAOT the enum formatter has no dynamic fallback,
    // so it must be pre-registered by the generated Register() helper (spec F8).
    [TomlUnion<Color>]
    [Union]
    public struct ColorOrName
    {
        private readonly object? _value;

        public ColorOrName(Color value) => _value = value;
        public ColorOrName(string value) => _value = value;

        public object? Value => _value;
    }

    [TomlSerializedObject]
    public partial class UnionAotSample
    {
        [TomlValueOnSerialized]
        public ColorOrName First { get; set; }

        [TomlValueOnSerialized]
        public ColorOrName Second { get; set; }
    }
}
