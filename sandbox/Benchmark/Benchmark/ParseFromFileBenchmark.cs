#nullable enable

using BenchmarkDotNet.Attributes;
using CsToml;
using CsToml.Extensions;
using Tommy;
using Tomlet;

namespace Benchmark;

public class ParseFromFileBenchmark
{
    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public CsToml.TomlDocument CsToml()
    {
        var document = CsTomlFileSerializer.Deserialize<TomlDocument>(Constants.TomlFilePath);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public async ValueTask<CsToml.TomlDocument> CsTomlAsync()
    {
        var document = await CsTomlFileSerializer.DeserializeAsync<TomlDocument>(Constants.TomlFilePath);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tommy.TomlTable Tommy()
    {
        using var reader = File.OpenText(Constants.TomlFilePath);
        var table = TOML.Parse(reader);
        return table;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlet.Models.TomlDocument Tomlet()
    {
        var document = TomlParser.ParseFile(Constants.TomlFilePath);
        return document;
    }

}
