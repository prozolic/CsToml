#nullable enable

using BenchmarkDotNet.Attributes;
using CsToml;
using CsToml.Values;
using System.Text;
using Tomlet.Models;

namespace Benchmark;

[Config(typeof(BenchmarkConfig))]
public class ParseBenchmark
{
    private static readonly string TestTomlFilePath = "./../../../../../../../Toml/test.toml";
#pragma warning disable CS8618
    private static byte[] tomlText;
    private static string tomlUtf16Text;
#pragma warning restore CS8618

    [GlobalSetup]
    public void GlobalSetup()
    {
        tomlUtf16Text = File.ReadAllText(TestTomlFilePath);
        tomlText = Encoding.UTF8.GetBytes(tomlUtf16Text);
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public void CsTomlDeserialize()
    {
        var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void TommyParse()
    {
        using var reader = new StringReader(tomlUtf16Text);
        var table = Tommy.TOML.Parse(reader);
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void TomletParse()
    {
        var parser = new Tomlet.TomlParser();
        var document = parser.Parse(tomlUtf16Text);
    }
}

