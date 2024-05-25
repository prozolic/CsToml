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
        var package = CsTomlFileSerializer.Deserialize<CsTomlPackage>(TestTomlFilePath);
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public async ValueTask CsTomlDeserializeFromFileAsync()
    {
        var package = await CsTomlFileSerializer.DeserializeAsync<CsTomlPackage>(TestTomlFilePath);
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public async ValueTask CsTomlDeserializeFromStreamAsync()
    {
        using (var stream = new FileStream(TestTomlFilePath, FileMode.Open))
        {
            var package = await CsTomlFileSerializer.DeserializeAsync<CsTomlPackage>(stream);
        }
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
