using BenchmarkDotNet.Attributes;
using CsToml;
using System.Text;
using Tomlet;

namespace Benchmark;

[CsToml.TomlSerializedObject]
public partial class TestTomlSerializedObject
{
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public uint intvalue { get; set; }

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public long longvalue { get; set; }

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public bool boolvalue { get; set; }

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public DateTime datetimevalue { get; set; }

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public DateTimeOffset datetimeoffsetvalue { get; set; }

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public double doublevalue { get; set; }

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public string stringvalue { get; set; }

    [TomlValueOnSerialized(TomlValueType.Array)]
    public string[] arrayvalue { get; set; }

}

[Config(typeof(BenchmarkConfig))]
public class ClassDeserializationBenchmark
{
#pragma warning disable CS8618
    private static byte[] tomlText;
    private static string tomlUtf16Text;
#pragma warning restore CS8618

    [GlobalSetup]
    public void GlobalSetup()
    {
        var tomlText = @"
intvalue = 123
longvalue = 456789
doublevalue = 3.1415
boolvalue = true

datetimeoffsetvalue = 1979-05-27T07:32:00Z
datetimevalue = 2024-08-12T07:32:00

stringvalue = ""I'm a string. \""You can quote me\"". Name\tJos\u00E9\nLocation\tSF.""
arrayvalue = [ ""red"", ""yellow"", ""green"" ]
#arrayvalue2 = [ 1, ""one"" , 1.0, 07:32:00, 1]

"u8;
        tomlText = tomlText.ToArray();
        tomlUtf16Text = Encoding.UTF8.GetString(tomlText);
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public void CsTomlDeserialize()
    {
        var obj = CsTomlSerializer.Deserialize<TestTomlSerializedObject>(tomlText); //CsToml
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void TomletDeserialize()
    {
        var obj = TomletMain.To<TestTomlSerializedObject>(tomlUtf16Text); // Tomlet
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void TomlynDeserialize()
    {
        var obj = Tomlyn.Toml.ToModel<TestTomlSerializedObject>(tomlUtf16Text); // Tomlyn
    }
}

