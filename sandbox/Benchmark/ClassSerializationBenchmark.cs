using BenchmarkDotNet.Attributes;
using Benchmark.Model;
using CsToml;
using System;
using System.Collections.Generic;

using System.Text;
using Tomlet;
using System.Buffers;

namespace Benchmark;

[Config(typeof(BenchmarkConfig))]
public class ClassSerializationBenchmark
{
#pragma warning disable CS8618
    private CsTomlSerializerOptions options = CsTomlSerializerOptions.Default with
    {
        SerializeOptions = new SerializeOptions { TableStyle = TomlTableStyle.Header }
    };
    private TestTomlSerializedObject testTomlSerializedObject;
    private TestTomlSerializedObjectInSnakeCase testTomlSerializedObjectInSnakeCase;
    private Encoding utf8 = Encoding.UTF8;
#pragma warning restore CS8618

    [GlobalSetup]
    public void GlobalSetup()
    {
        testTomlSerializedObject = new TestTomlSerializedObject()
        {
            Str = "I'm a string. You can quote me. Name\tJosé\nLocation\tSF.",
            Long = 456789,
            Float = 3.1415,
            Boolean = true,
            OffsetDateTime = new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero),
            LocalDateTime = new DateTime(2024, 8, 12, 7, 32, 0),
            Array = ["red", "yellow", "green"],
            Table = new Table() { Value = "some string" },
            ArrayOfTable = new List<Table2>()
            {
                new Table2() { Value = "Hammer" },
                new Table2() { Value = "Hammer2" },
                new Table2() { Value = "Hammer3" }
            }
        };
        testTomlSerializedObjectInSnakeCase = new Benchmark.Model.TestTomlSerializedObjectInSnakeCase()
        {
            Str = testTomlSerializedObject.Str,
            Long = testTomlSerializedObject.Long,
            Float = testTomlSerializedObject.Float,
            Boolean = testTomlSerializedObject.Boolean,
            LocalDateTime = testTomlSerializedObject.LocalDateTime,
            OffsetDateTime = testTomlSerializedObject.OffsetDateTime,
            Array = testTomlSerializedObject.Array,
            Table = testTomlSerializedObject.Table,
            ArrayOfTable = testTomlSerializedObject.ArrayOfTable
        };
    }

    [BenchmarkCategory("Benchmark"), Benchmark(Baseline = true)]
    public void CsToml_Serialize()
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        CsToml.CsTomlSerializer.Serialize(ref bufferWriter, testTomlSerializedObject, options); //CsToml
        var test = utf8.GetString(bufferWriter.WrittenSpan);
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void Tomlet_Serialize()
    {
        var text = TomletMain.TomlStringFrom<TestTomlSerializedObject>(testTomlSerializedObject); // Tomlet
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public void Tomlyn_Serialize()
    {
        var text = Tomlyn.Toml.FromModel(testTomlSerializedObjectInSnakeCase); // Tomlyn
    }
}

