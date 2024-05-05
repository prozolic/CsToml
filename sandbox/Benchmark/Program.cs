// See https://aka.ms/new-console-template for more information
using Benchmark;
using BenchmarkDotNet.Running;

Console.WriteLine("Hello, World!");

var switcher = new BenchmarkSwitcher(new[] { typeof(Benchmark.ParseBenchmark), typeof(ParseFromFileBenchmark),});
//var switcher = new BenchmarkSwitcher(new[] { typeof(Benchmark.ParseBenchmark) });
switcher.Run(["Release", "--filter", "*"]);

Console.WriteLine("End");