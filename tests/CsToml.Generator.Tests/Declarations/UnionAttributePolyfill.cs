#if !NET11_0_OR_GREATER
// Polyfill so that custom unions (detected by attribute name) can be tested on TFMs without .NET 11.
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
internal sealed class UnionAttribute : Attribute
{ }
#endif
