using BenchmarkDotNet.Attributes;
using CsToml;
using System.Buffers;
using System.Text;

namespace Benchmark.Benchmark;

public class Utf8TomlDocumentWriterBenchmark
{
#pragma warning disable CS8618
    private static readonly Encoding utf8 = Encoding.UTF8;
    private StringObject stringObject;
#pragma warning restore CS8618

    [Params(1, 10, 100, 1000, 10000)]
    public int Size = 10;

    [GlobalSetup]
    public void GlobalSetup()
    {
        stringObject = new StringObject() { Value = string.Join("", Enumerable.Repeat(1, Size).Select(i => i.ToString())) };
    }

    [Benchmark]
    public string WriteString()
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        CsTomlSerializer.Serialize(ref bufferWriter, stringObject);
        return utf8.GetString(bufferWriter.WrittenSpan);
    }
}

[TomlSerializedObject]
public partial class StringObject
{
    [TomlValueOnSerialized]
    public string? Value { get; set; }
}