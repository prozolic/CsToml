#nullable enable

using BenchmarkDotNet.Attributes;
using CsToml;
using System.Text;

namespace Benchmark;

[Config(typeof(BenchmarkConfig))]
public class Benchmark
{
#pragma warning disable CS8618
    private static byte[] tomlText;
    private static string tomlUtf16Text;
#pragma warning restore CS8618

    [GlobalSetup]
    public void GlobalSetup()
    {
        tomlUtf16Text = File.ReadAllText("./../../../../../../../Toml/test.toml");
        tomlText = Encoding.UTF8.GetBytes(tomlUtf16Text);
    }

    [BenchmarkCategory("Sample"), Benchmark]
    public void TestCsToml()
    {
        var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);
    }

    [BenchmarkCategory("Sample"), Benchmark]
    public void TestTommy()
    {
        using var reader = new StringReader(tomlUtf16Text);
        var table = Tommy.TOML.Parse(reader);
    }


}

