using Benchmark;
using BenchmarkDotNet.Running;

#if DEBUG

var defaultBenchmark = new DefaultParseBenchmark();
defaultBenchmark.GlobalSetup();
defaultBenchmark.CsToml_Parse();

return;
#else

var config = new BenchmarkConfig();
var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config).ToArray();
if (summaries.Length == 0)
{
    Console.WriteLine("No benchmarks were executed.");
}

#endif