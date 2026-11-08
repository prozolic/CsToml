using System.Runtime.CompilerServices;

namespace CsToml.Generator.Tests;

/// <summary>
/// Skips a test in the NativeAOT build of this project, detected the same way BuiltinFormatterResolver decides
/// whether its MakeGenericType fallback can run. The reason must say why the test cannot pass there:
/// either CsToml resolves the type only through that fallback by design, or a known gap.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
internal sealed class SkipOnNativeAotAttribute(string reason) : SkipAttribute(reason)
{
    public override Task<bool> ShouldSkip(TestRegisteredContext context)
        => Task.FromResult(!RuntimeFeature.IsDynamicCodeCompiled);
}
