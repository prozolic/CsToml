// See https://aka.ms/new-console-template for more information
using Benchmark;
using BenchmarkDotNet.Running;

Console.WriteLine("Hello, World!");

var switcher = new BenchmarkSwitcher(new[] {
    typeof(ClassDeserializationBenchmark),
    typeof(StringOnlyParseBenchmark),
    typeof(IntOnlyParseBenchmark),
    typeof(FloatOnlyParseBenchmark),
    typeof(BoolOnlyParseBenchmark),
    typeof(OffsetDateTimeOnlyParseBenchmark),
    typeof(LocalDateTimeOnlyParseBenchmark),
    typeof(LocalDateOnlyParseBenchmark),
    typeof(LocalTimeOnlyParseBenchmark),
    typeof(ArrayOnlyParseBenchmark),
    typeof(TableOnlyParseBenchmark),
    typeof(InlineTableOnlyParseBenchmark),
    typeof(ArrayOfTableOnlyParseBenchmark),
    typeof(DefaultParseBenchmark),
    typeof(ParseFromFileBenchmark)
});
switcher.Run(["Release", "--filter", "*"]);

Console.WriteLine("End");