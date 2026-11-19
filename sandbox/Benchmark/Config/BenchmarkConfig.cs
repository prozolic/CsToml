using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Toolchains.NetCoreApp;

namespace Benchmark;

internal class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        WithSummaryStyle(SummaryStyle.Default.WithMaxParameterColumnWidth(20));

        // Apply both global settings and local setting (Attribute)
        WithUnionRule(DefaultConfig.Instance.UnionRule);
        WithArtifactsPath(DefaultConfig.Instance.ArtifactsPath!);

        AddExporter([MarkdownExporter.GitHub, MarkdownExporter.Console, HtmlExporter.Default]);
        AddDiagnoser(MemoryDiagnoser.Default);

        AddColumnProvider(DefaultColumnProviders.Instance);
        AddAnalyser(DefaultConfig.Instance.GetAnalysers().ToArray());
        AddValidator(DefaultConfig.Instance.GetValidators().ToArray());
        AddLogger(ConsoleLogger.Default);

        // .NET 10.0 as default.
        AddJob(Job.MediumRun
            .WithStrategy(RunStrategy.Throughput)
            .DontEnforcePowerPlan()
            .WithToolchain(CsProjCoreToolchain.NetCoreApp10_0)
            .WithId("Benchmark.NET 10.0"));

        AddJob(Job.MediumRun
            .WithStrategy(RunStrategy.Throughput)
            .DontEnforcePowerPlan()
            .WithToolchain(CsProjCoreToolchain.NetCoreApp11_0)
            .WithId("Benchmark.NET 11.0"));

        // NuGet version
        AddJob(Job.MediumRun
            .WithStrategy(RunStrategy.Throughput)
            .DontEnforcePowerPlan()
            .WithToolchain(CsProjCoreToolchain.NetCoreApp10_0)
            .WithCustomBuildConfiguration("NuGetCsToml")
            .WithId("NuGetCsToml"));
    }

}