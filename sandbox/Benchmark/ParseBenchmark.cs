#nullable enable

using BenchmarkDotNet.Attributes;
using CsToml;
using System.Text;

namespace Benchmark;


[Config(typeof(BenchmarkConfig))]
public class DefaultParseBenchmark
{
    private static readonly string TestTomlFilePath = "./../../../../../../../Toml/test.toml";
#pragma warning disable CS8618
    private static string tomlUtf16Text;
#pragma warning restore CS8618

    [GlobalSetup]
    public void GlobalSetup()
    {
        tomlUtf16Text = File.ReadAllText(TestTomlFilePath);
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public void CsToml_Parse()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(Encoding.UTF8.GetBytes(tomlUtf16Text));
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void Tommy_Parse()
    {
        using var reader = new StringReader(tomlUtf16Text);
        var table = Tommy.TOML.Parse(reader);
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void Tomlet_Parse()
    {
        var parser = new Tomlet.TomlParser();
        var document = parser.Parse(tomlUtf16Text);
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void Tomlyn_Parse()
    {
        var document = Tomlyn.Toml.ToModel(tomlUtf16Text);
    }
}

[Config(typeof(BenchmarkConfig))]
public class BoolOnlyParseBenchmark
{
#pragma warning disable CS8618
    private static byte[] tomlText;
    private static string tomlUtf16Text;
#pragma warning restore CS8618

    [GlobalSetup]
    public void GlobalSetup()
    {
        var builder = new StringBuilder(2097152);
        for (int i = 0; i < 100000; i += 2)
        {
            builder.AppendLine($"test{i} = true");
            builder.AppendLine($"test{i + 1} = false");
        }
        tomlUtf16Text = builder.ToString();
        tomlText = Encoding.UTF8.GetBytes(tomlUtf16Text);
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public void CsTomlDeserialize()
    {
        var package = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
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

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void TomlynParse()
    {
        var document = Tomlyn.Toml.ToModel(tomlUtf16Text);
    }
}

[Config(typeof(BenchmarkConfig))]
public class StringOnlyParseBenchmark
{
#pragma warning disable CS8618
    private static byte[] tomlText;
    private static string tomlUtf16Text;
#pragma warning restore CS8618

    [GlobalSetup]
    public void GlobalSetup()
    {
        var builder = new StringBuilder(2097152);
        for (int i = 0; i < 100000; i += 2)
        {
            builder.AppendLine($"test{i} = \"0123456789\"");
            builder.AppendLine($"test{i + 1} = \"I'm a string. \\\"You can quote me\\\". Name\\tJos\\u00E9\\nLocation\\tSF.\"");
        }
        tomlUtf16Text = builder.ToString();
        tomlText = Encoding.UTF8.GetBytes(tomlUtf16Text);
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public void CsTomlDeserialize()
    {
        var package = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
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

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void TomlynParse()
    {
        var document = Tomlyn.Toml.ToModel(tomlUtf16Text);
    }
}

[Config(typeof(BenchmarkConfig))]
public class IntOnlyParseBenchmark
{
#pragma warning disable CS8618
    private static byte[] tomlText;
    private static string tomlUtf16Text;
#pragma warning restore CS8618

    [GlobalSetup]
    public void GlobalSetup()
    {
        var builder = new StringBuilder(2097152);
        for (int i = 0; i < 100000; i += 5)
        {
            builder.AppendLine($"test{i} = 123456789");
            builder.AppendLine($"test{i + 1} = 1_000");
            builder.AppendLine($"test{i + 2} = 0xDEADBEEF");
            builder.AppendLine($"test{i + 3} = 0o01234567");
            builder.AppendLine($"test{i + 4} = 0b11010110");

        }
        tomlUtf16Text = builder.ToString();
        tomlText = Encoding.UTF8.GetBytes(tomlUtf16Text);
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public void CsTomlDeserialize()
    {
        var package = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
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

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void TomlynParse()
    {
        var document = Tomlyn.Toml.ToModel(tomlUtf16Text);
    }
}

[Config(typeof(BenchmarkConfig))]
public class FloatOnlyParseBenchmark
{
#pragma warning disable CS8618
    private static byte[] tomlText;
    private static string tomlUtf16Text;
#pragma warning restore CS8618

    [GlobalSetup]
    public void GlobalSetup()
    {
        var builder = new StringBuilder(2097152);
        for (int i = 0; i < 100000; i += 5)
        {
            builder.AppendLine($"test{i} = +1.0");
            builder.AppendLine($"test{i + 1} = 3.1415");
            builder.AppendLine($"test{i + 2} = -2E-2");
            builder.AppendLine($"test{i + 3} = nan");
            builder.AppendLine($"test{i + 4} = inf");
        }
        tomlUtf16Text = builder.ToString();
        tomlText = Encoding.UTF8.GetBytes(tomlUtf16Text);
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public void CsTomlDeserialize()
    {
        var package = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
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

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void TomlynParse()
    {
        var document = Tomlyn.Toml.ToModel(tomlUtf16Text);
    }
}

[Config(typeof(BenchmarkConfig))]
public class OffsetDateTimeOnlyParseBenchmark
{
#pragma warning disable CS8618
    private static byte[] tomlText;
    private static string tomlUtf16Text;
#pragma warning restore CS8618

    [GlobalSetup]
    public void GlobalSetup()
    {
        var builder = new StringBuilder(2097152);
        for (int i = 0; i < 100000; i += 5)
        {
            builder.AppendLine($"test{i} = 1979-05-27T07:32:00Z");
            builder.AppendLine($"test{i + 1} = 1979-05-27T00:32:00-07:00");
            builder.AppendLine($"test{i + 2} = 1979-05-27T00:32:00.999999-07:00");
            builder.AppendLine($"test{i + 3} = 1979-05-27 07:32:00Z");
            builder.AppendLine($"test{i + 4} = 1979-05-27 07:32:00-07:00");
        }
        tomlUtf16Text = builder.ToString();
        tomlText = Encoding.UTF8.GetBytes(tomlUtf16Text);
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public void CsTomlDeserialize()
    {
        var package = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
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

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void TomlynParse()
    {
        var document = Tomlyn.Toml.ToModel(tomlUtf16Text);
    }
}

[Config(typeof(BenchmarkConfig))]
public class LocalDateTimeOnlyParseBenchmark
{
#pragma warning disable CS8618
    private static byte[] tomlText;
    private static string tomlUtf16Text;
#pragma warning restore CS8618

    [GlobalSetup]
    public void GlobalSetup()
    {
        var builder = new StringBuilder(2097152);
        for (int i = 0; i < 100000; i += 2)
        {
            builder.AppendLine($"test{i} = 1979-05-27T07:32:00");
            builder.AppendLine($"test{i + 1} = 1979-05-27T00:32:00.999999");
        }
        tomlUtf16Text = builder.ToString();
        tomlText = Encoding.UTF8.GetBytes(tomlUtf16Text);
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public void CsTomlDeserialize()
    {
        var package = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
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

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void TomlynParse()
    {
        var document = Tomlyn.Toml.ToModel(tomlUtf16Text);
    }
}

[Config(typeof(BenchmarkConfig))]
public class LocalDateOnlyParseBenchmark
{
#pragma warning disable CS8618
    private static byte[] tomlText;
    private static string tomlUtf16Text;
#pragma warning restore CS8618

    [GlobalSetup]
    public void GlobalSetup()
    {
        var builder = new StringBuilder(2097152);
        for (int i = 0; i < 100000; i += 2)
        {
            builder.AppendLine($"test{i} = 1979-05-27");
            builder.AppendLine($"test{i + 1} = 2024-08-04");
        }
        tomlUtf16Text = builder.ToString();
        tomlText = Encoding.UTF8.GetBytes(tomlUtf16Text);
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public void CsTomlDeserialize()
    {
        var package = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
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

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void TomlynParse()
    {
        var document = Tomlyn.Toml.ToModel(tomlUtf16Text);
    }
}

[Config(typeof(BenchmarkConfig))]
public class LocalTimeOnlyParseBenchmark
{
#pragma warning disable CS8618
    private static byte[] tomlText;
    private static string tomlUtf16Text;
#pragma warning restore CS8618

    [GlobalSetup]
    public void GlobalSetup()
    {
        var builder = new StringBuilder(2097152);
        for (int i = 0; i < 100000; i += 2)
        {
            builder.AppendLine($"test{i} = 07:32:00");
            builder.AppendLine($"test{i + 1} = 00:32:00.999999");
        }
        tomlUtf16Text = builder.ToString();
        tomlText = Encoding.UTF8.GetBytes(tomlUtf16Text);
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public void CsTomlDeserialize()
    {
        var package = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
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

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void TomlynParse()
    {
        var document = Tomlyn.Toml.ToModel(tomlUtf16Text);
    }
}

[Config(typeof(BenchmarkConfig))]
public class ArrayOnlyParseBenchmark
{
#pragma warning disable CS8618
    private static byte[] tomlText;
    private static string tomlUtf16Text;
#pragma warning restore CS8618

    [GlobalSetup]
    public void GlobalSetup()
    {
        var builder = new StringBuilder(2097152);
        for (int i = 0; i < 100000; i += 5)
        {
            builder.AppendLine($"test{i} = [ 1, 2, 3 ]");
            builder.AppendLine($"test{i + 1} = [ \"red\", \"yellow\", \"green\" ]");
            builder.AppendLine($"test{i + 2} = [ [ 1, 2 ], [3, 4, 5] ]");
            builder.AppendLine($"test{i + 3} = [ [ 1, 2 ], [\"a\", \"b\", \"c\"] ]");
            builder.AppendLine($"test{i + 4} = [ \"all\", 'strings', \"\"\"are the same\"\"\", '''type''' ]");
        }
        tomlUtf16Text = builder.ToString();
        tomlText = Encoding.UTF8.GetBytes(tomlUtf16Text);
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public void CsTomlDeserialize()
    {
        var package = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
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

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void TomlynParse()
    {
        var document = Tomlyn.Toml.ToModel(tomlUtf16Text);
    }
}

[Config(typeof(BenchmarkConfig))]
public class TableOnlyParseBenchmark
{
#pragma warning disable CS8618
    private static byte[] tomlText;
    private static string tomlUtf16Text;
#pragma warning restore CS8618

    [GlobalSetup]
    public void GlobalSetup()
    {
        var builder = new StringBuilder(2097152);
        for (int i = 0; i < 20000; i++)
        {
            builder.AppendLine($"[table{i}]");
            builder.AppendLine($"test = \"some string\"");
            builder.AppendLine($"test2 = 123");
            builder.AppendLine($"test3 = true");
            builder.AppendLine($"test4 = 1979-05-27T07:32:00Z");
            builder.AppendLine($"test5 = [ \"red\", \"yellow\", \"green\" ]");
            builder.AppendLine();
        }
        tomlUtf16Text = builder.ToString();
        tomlText = Encoding.UTF8.GetBytes(tomlUtf16Text);
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public void CsTomlDeserialize()
    {
        var package = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
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

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void TomlynParse()
    {
        var document = Tomlyn.Toml.ToModel(tomlUtf16Text);
    }
}

[Config(typeof(BenchmarkConfig))]
public class InlineTableOnlyParseBenchmark
{
#pragma warning disable CS8618
    private static byte[] tomlText;
    private static string tomlUtf16Text;
#pragma warning restore CS8618

    [GlobalSetup]
    public void GlobalSetup()
    {
        var builder = new StringBuilder(2097152);
        for (int i = 0; i < 100000; i += 2)
        {
            builder.Append($"test{i} = ");
            builder.AppendLine("{first = \"Tom\", last = \"Preston-Werner\" }");
            builder.Append($"test{i + 1} = ");
            builder.AppendLine("{ type.name = \"pug\" }");
        }
        tomlUtf16Text = builder.ToString();
        tomlText = Encoding.UTF8.GetBytes(tomlUtf16Text);
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public void CsTomlDeserialize()
    {
        var package = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
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

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void TomlynParse()
    {
        var document = Tomlyn.Toml.ToModel(tomlUtf16Text);
    }
}

[Config(typeof(BenchmarkConfig))]
public class ArrayOfTableOnlyParseBenchmark
{
#pragma warning disable CS8618
    private static byte[] tomlText;
    private static string tomlUtf16Text;
#pragma warning restore CS8618

    [GlobalSetup]
    public void GlobalSetup()
    {
        var builder = new StringBuilder(2097152);
        for (int i = 0; i < 100000; i++)
        {
            builder.AppendLine($"[[table]]");
            builder.AppendLine($"test = \"some string\"");
            builder.AppendLine();
        }
        tomlUtf16Text = builder.ToString();
        tomlText = Encoding.UTF8.GetBytes(tomlUtf16Text);
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public void CsTomlDeserialize()
    {
        var package = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
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

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void TomlynParse()
    {
        var document = Tomlyn.Toml.ToModel(tomlUtf16Text);
    }
}
