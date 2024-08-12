#nullable enable

using BenchmarkDotNet.Attributes;
using CsToml;
using CsToml.Extensions;

namespace Benchmark;

[Config(typeof(BenchmarkConfig))]
public class ParseFromFileBenchmark
{
    private static readonly string TestTomlFilePath = "./../../../../../../../Toml/test.toml";

    [GlobalSetup]
    public void GlobalSetup()
    { }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public void CsTomlDeserializeFromFile()
    {
        var document = CsTomlFileSerializer.Deserialize<TomlDocument>(TestTomlFilePath);
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public async ValueTask CsTomlDeserializeFromFileAsync()
    {
        var document = await CsTomlFileSerializer.DeserializeAsync<TomlDocument>(TestTomlFilePath);
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void TommyParseFromFile()
    {
        using var reader = File.OpenText(TestTomlFilePath);
        var table = Tommy.TOML.Parse(reader);
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void TomletParseFromFile()
    {
        var document = Tomlet.TomlParser.ParseFile(TestTomlFilePath);
    }

}
