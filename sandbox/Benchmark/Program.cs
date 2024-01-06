// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;

Console.WriteLine("Hello, World!");

var switcher = new BenchmarkSwitcher(new[] { typeof(Benchmark.Benchmark) });
switcher.Run(["Release", "--filter", "*"]);

Console.WriteLine("End");