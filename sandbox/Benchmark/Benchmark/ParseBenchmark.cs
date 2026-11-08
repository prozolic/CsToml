#nullable enable

using BenchmarkDotNet.Attributes;
using CsToml;
using System.Globalization;
using System.Text;
using Tommy;
using Tomlyn;

#pragma warning disable CS8618

namespace Benchmark;

public class DefaultParseBenchmark
{
#pragma warning disable CS8618
    private static string tomlUtf16Text;
    private static byte[] tomlText;
#pragma warning restore CS8618

    [Params("config.toml", "test.toml")]
    public string TomlFile { get; set; } = "test.toml";

    [GlobalSetup]
    public void GlobalSetup()
    {
        tomlUtf16Text = File.ReadAllText(Path.Combine(Constants.TomlDirectoryPath, TomlFile));
        tomlText = Encoding.UTF8.GetBytes(tomlUtf16Text);
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public CsToml.TomlDocument CsToml_Parse_FromUTF16()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(Encoding.UTF8.GetBytes(tomlUtf16Text));
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public CsToml.TomlDocument CsToml_Parse_FromUTF8()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tommy.TomlTable Tommy_Parse()
    {
        using var reader = new StringReader(tomlUtf16Text);
        var table = TOML.Parse(reader);
        return table;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlet.Models.TomlDocument Tomlet_Parse()
    {
        var parser = new Tomlet.TomlParser();
        var document = parser.Parse(tomlUtf16Text);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlyn.Model.TomlTable Tomlyn_Parse()
    {
        var table = TomlSerializer.Deserialize<Tomlyn.Model.TomlTable>(tomlUtf16Text)!;
        return table;
    }
}

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
    public CsToml.TomlDocument CsToml()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tommy.TomlTable Tommy()
    {
        using var reader = new StringReader(tomlUtf16Text);
        var table = TOML.Parse(reader);
        return table;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlet.Models.TomlDocument Tomlet()
    {
        var parser = new Tomlet.TomlParser();
        var document = parser.Parse(tomlUtf16Text);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlyn.Model.TomlTable Tomlyn()
    {
        var table = TomlSerializer.Deserialize<Tomlyn.Model.TomlTable>(tomlUtf16Text)!;
        return table;
    }
}

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
    public CsToml.TomlDocument CsToml()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tommy.TomlTable Tommy()
    {
        using var reader = new StringReader(tomlUtf16Text);
        var table = TOML.Parse(reader);
        return table;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlet.Models.TomlDocument Tomlet()
    {
        var parser = new Tomlet.TomlParser();
        var document = parser.Parse(tomlUtf16Text);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlyn.Model.TomlTable Tomlyn()
    {
        var document = TomlSerializer.Deserialize<Tomlyn.Model.TomlTable>(tomlUtf16Text)!;
        return document;
    }
}

public class IntOnlyParseBenchmark
{
#pragma warning disable CS8618
    private static byte[] tomlText;
    private static string tomlUtf16Text;
#pragma warning restore CS8618

    [GlobalSetup]
    public void GlobalSetup()
    {
        var random = new Random(42);
        var builder = new StringBuilder(2097152);
        for (int i = 0; i < 100000; i++)
        {
            switch (i % 20)
            {
                case < 16:
                    var number = random.NextInt64(1L << random.Next(4, 34));
                    builder.AppendLine(CultureInfo.InvariantCulture, $"test{i} = {(random.Next(5) == 0 ? -number : number)}");
                    break;
                case < 18:
                    builder.AppendLine(CultureInfo.InvariantCulture, $"test{i} = {random.Next(1000, int.MaxValue).ToString("#,0", CultureInfo.InvariantCulture).Replace(',', '_')}");
                    break;
                case 18:
                    builder.AppendLine(CultureInfo.InvariantCulture, $"test{i} = 0x{random.Next():X8}");
                    break;
                default:
                    builder.AppendLine(i / 20 % 2 == 0
                        ? $"test{i} = 0o{Convert.ToString(random.Next(1 << 21), 8)}"
                        : $"test{i} = 0b{Convert.ToString(random.Next(256), 2)}");
                    break;
            }
        }
        tomlUtf16Text = builder.ToString();
        tomlText = Encoding.UTF8.GetBytes(tomlUtf16Text);
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public CsToml.TomlDocument CsToml()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tommy.TomlTable Tommy()
    {
        using var reader = new StringReader(tomlUtf16Text);
        var table = TOML.Parse(reader);
        return table;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlet.Models.TomlDocument Tomlet()
    {
        var parser = new Tomlet.TomlParser();
        var document = parser.Parse(tomlUtf16Text);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlyn.Model.TomlTable Tomlyn()
    {
        var table = TomlSerializer.Deserialize<Tomlyn.Model.TomlTable>(tomlUtf16Text)!;
        return table;
    }
}

public class FloatOnlyParseBenchmark
{
#pragma warning disable CS8618
    private static byte[] tomlText;
    private static string tomlUtf16Text;
#pragma warning restore CS8618

    [GlobalSetup]
    public void GlobalSetup()
    {
        var random = new Random(42);
        var builder = new StringBuilder(2097152);
        for (int i = 0; i < 100000; i++)
        {
            var sign = random.Next(4) == 0 ? "-" : "";
            switch (i % 50)
            {
                case < 40:
                    builder.AppendLine(CultureInfo.InvariantCulture, $"test{i} = {sign}{random.Next(10000)}.{random.Next(10000)}");
                    break;
                case < 45:
                    builder.AppendLine(CultureInfo.InvariantCulture, $"test{i} = {sign}{random.Next(1, 10)}.{random.Next(1000)}e{random.Next(-30, 31)}");
                    break;
                case < 49:
                    builder.AppendLine(CultureInfo.InvariantCulture, $"test{i} = {sign}{random.Next(1, 10)}E{random.Next(-30, 31)}");
                    break;
                default:
                    builder.AppendLine((i / 50 % 3) switch { 0 => $"test{i} = inf", 1 => $"test{i} = -inf", _ => $"test{i} = nan" });
                    break;
            }
        }
        tomlUtf16Text = builder.ToString();
        tomlText = Encoding.UTF8.GetBytes(tomlUtf16Text);
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public CsToml.TomlDocument CsToml()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tommy.TomlTable Tommy()
    {
        using var reader = new StringReader(tomlUtf16Text);
        var table = TOML.Parse(reader);
        return table;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlet.Models.TomlDocument Tomlet()
    {
        var parser = new Tomlet.TomlParser();
        var document = parser.Parse(tomlUtf16Text);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlyn.Model.TomlTable Tomlyn()
    {
        var table = TomlSerializer.Deserialize<Tomlyn.Model.TomlTable>(tomlUtf16Text)!;
        return table;
    }
}

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
    public CsToml.TomlDocument CsToml()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tommy.TomlTable Tommy()
    {
        using var reader = new StringReader(tomlUtf16Text);
        var table = TOML.Parse(reader);
        return table;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlet.Models.TomlDocument Tomlet()
    {
        var parser = new Tomlet.TomlParser();
        var document = parser.Parse(tomlUtf16Text);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlyn.Model.TomlTable Tomlyn()
    {
        var table = TomlSerializer.Deserialize<Tomlyn.Model.TomlTable>(tomlUtf16Text)!;
        return table;
    }
}

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
    public CsToml.TomlDocument CsToml()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tommy.TomlTable Tommy()
    {
        using var reader = new StringReader(tomlUtf16Text);
        var table = TOML.Parse(reader);
        return table;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlet.Models.TomlDocument Tomlet()
    {
        var parser = new Tomlet.TomlParser();
        var document = parser.Parse(tomlUtf16Text);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlyn.Model.TomlTable Tomlyn()
    {
        var table = TomlSerializer.Deserialize<Tomlyn.Model.TomlTable>(tomlUtf16Text)!;
        return table;
    }
}

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
    public CsToml.TomlDocument CsToml()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tommy.TomlTable Tommy()
    {
        using var reader = new StringReader(tomlUtf16Text);
        var table = TOML.Parse(reader);
        return table;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlet.Models.TomlDocument Tomlet()
    {
        var parser = new Tomlet.TomlParser();
        var document = parser.Parse(tomlUtf16Text);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlyn.Model.TomlTable Tomlyn()
    {
        var table = TomlSerializer.Deserialize<Tomlyn.Model.TomlTable>(tomlUtf16Text)!;
        return table;
    }
}

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
    public CsToml.TomlDocument CsToml()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tommy.TomlTable Tommy()
    {
        using var reader = new StringReader(tomlUtf16Text);
        var table = TOML.Parse(reader);
        return table;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlet.Models.TomlDocument Tomlet()
    {
        var parser = new Tomlet.TomlParser();
        var document = parser.Parse(tomlUtf16Text);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlyn.Model.TomlTable Tomlyn()
    {
        var table = TomlSerializer.Deserialize<Tomlyn.Model.TomlTable>(tomlUtf16Text)!;
        return table;
    }
}

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
            builder.AppendLine($"test{i} = [ 10, 200, 3000 ]");
            builder.AppendLine($"test{i + 1} = [ \"red\", \"yellow\", \"green\" ]");
            builder.AppendLine($"test{i + 2} = [ [ 1, 20 ], [300, 4000, 50000] ]");
            builder.AppendLine($"test{i + 3} = [ [ 1, 2 ], [\"a\", \"b\", \"c\"] ]");
            builder.AppendLine($"test{i + 4} = [ \"all\", 'strings', \"\"\"are the same\"\"\", '''type''' ]");
        }
        tomlUtf16Text = builder.ToString();
        tomlText = Encoding.UTF8.GetBytes(tomlUtf16Text);
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public CsToml.TomlDocument CsToml()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tommy.TomlTable Tommy()
    {
        using var reader = new StringReader(tomlUtf16Text);
        var table = TOML.Parse(reader);
        return table;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlet.Models.TomlDocument Tomlet()
    {
        var parser = new Tomlet.TomlParser();
        var document = parser.Parse(tomlUtf16Text);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlyn.Model.TomlTable Tomlyn()
    {
        var table = TomlSerializer.Deserialize<Tomlyn.Model.TomlTable>(tomlUtf16Text)!;
        return table;
    }
}

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
    public CsToml.TomlDocument CsToml()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tommy.TomlTable Tommy()
    {
        using var reader = new StringReader(tomlUtf16Text);
        var table = TOML.Parse(reader);
        return table;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlet.Models.TomlDocument Tomlet()
    {
        var parser = new Tomlet.TomlParser();
        var document = parser.Parse(tomlUtf16Text);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlyn.Model.TomlTable Tomlyn()
    {
        var table = TomlSerializer.Deserialize<Tomlyn.Model.TomlTable>(tomlUtf16Text)!;
        return table;
    }
}

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
    public CsToml.TomlDocument CsToml()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tommy.TomlTable Tommy()
    {
        using var reader = new StringReader(tomlUtf16Text);
        var table = TOML.Parse(reader);
        return table;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlet.Models.TomlDocument Tomlet()
    {
        var parser = new Tomlet.TomlParser();
        var document = parser.Parse(tomlUtf16Text);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlyn.Model.TomlTable Tomlyn()
    {
        var table = TomlSerializer.Deserialize<Tomlyn.Model.TomlTable>(tomlUtf16Text)!;
        return table;
    }
}

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
    public CsToml.TomlDocument CsToml()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tommy.TomlTable Tommy()
    {
        using var reader = new StringReader(tomlUtf16Text);
        var table = TOML.Parse(reader);
        return table;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlet.Models.TomlDocument Tomlet()
    {
        var parser = new Tomlet.TomlParser();
        var document = parser.Parse(tomlUtf16Text);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public Tomlyn.Model.TomlTable Tomlyn()
    {
        var table = TomlSerializer.Deserialize<Tomlyn.Model.TomlTable>(tomlUtf16Text)!;
        return table;
    }
}
