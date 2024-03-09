using BenchmarkDotNet.Attributes;
using CsToml;
using System.IO;
using System.Reflection;
using System.Text;

namespace Benchmark;

[Config(typeof(BenchmarkConfig))]
public class Benchmark
{
    private static byte[] tomlText;
    private static string tomlUtf16Text;

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

