using Microsoft.CodeAnalysis;

namespace CsToml.Generator;

[Generator(LanguageNames.CSharp)]
public partial class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        CsTomlPackageGenerator.Generate(context);
    }
}