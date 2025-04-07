using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;

namespace Benchmark;

internal class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        AddExporter([MarkdownExporter.GitHub, MarkdownExporter.Console, HtmlExporter.Default]);
        AddDiagnoser(MemoryDiagnoser.Default);
        AddColumn(CategoriesColumn.Default);
        AddColumn(RankColumn.Arabic);
        AddColumn(StatisticColumn.Min);
        AddColumn(StatisticColumn.Max);

        AddColumnProvider(DefaultColumnProviders.Instance);
        AddAnalyser(DefaultConfig.Instance.GetAnalysers().ToArray());
        AddValidator(DefaultConfig.Instance.GetValidators().ToArray());
        AddLogger(ConsoleLogger.Default);

        //AddJob(Job.ShortRun);
        AddJob(Job.ShortRun.DontEnforcePowerPlan());
    }

}

