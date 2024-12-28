using BenchmarkDotNet.Attributes;
using Benchmark.Model;
using CsToml;
using System.Text;
using Tomlet;

namespace Benchmark;

[Config(typeof(BenchmarkConfig))]
public class ClassDeserializationBenchmark
{
#pragma warning disable CS8618
    private static string tomlUtf16Text;
    private static string tomlUtf16TextInSnakeCase;
#pragma warning restore CS8618

    [GlobalSetup]
    public void GlobalSetup()
    {
        tomlUtf16Text = @"
#Benchmark

Str = ""I'm a string. You can quote me. Name\tJos\u00E9\nLocation\tSF.""
Long = 456789
Float = 3.1415
Boolean = true

LocalDateTime = 1979-05-27T07:32:00
OffsetDateTime = 1979-05-27T07:32:00Z

Array = [ ""red"", ""yellow"", ""green"" ]

[Table]
Value = ""some string""

[[ArrayOfTable]]
Value =  ""Hammer""

[[ArrayOfTable]]
Value =  ""Hammer2""

[[ArrayOfTable]]
Value =  ""Hammer3""

";

        tomlUtf16TextInSnakeCase = @"
#Benchmark

str = ""I'm a string. You can quote me. Name\tJos\u00E9\nLocation\tSF.""
long = 456789
float = 3.1415
boolean = true

local_date_time = 1979-05-27T07:32:00
offset_date_time = 1979-05-27T07:32:00Z

array = [ ""red"", ""yellow"", ""green"" ]

[table]
value = ""some string""

[[array_of_table]]
value =  ""Hammer""

[[array_of_table]]
value =  ""Hammer2""

[[array_of_table]]
value =  ""Hammer3""

";
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public void CsToml_Deserialize()
    {
        var obj = CsTomlSerializer.Deserialize<TestTomlSerializedObject>(Encoding.UTF8.GetBytes(tomlUtf16Text)); //CsToml
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void Tomlet_Deserialize()
    {
        var obj = TomletMain.To<TestTomlSerializedObject>(tomlUtf16Text); // Tomlet
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void Tomlyn_Deserialize()
    {
        var obj = Tomlyn.Toml.ToModel<TestTomlSerializedObjectInSnakeCase>(tomlUtf16TextInSnakeCase); // Tomlyn
    }
}

