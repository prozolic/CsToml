using CsToml.Error;
using CsToml.Utility;
using System.Buffers;
using System.Text;

namespace CsToml.Tests;

// Arrays and inline tables are read recursively. Before MaxDepth, a few thousand '[' overflowed
// the stack, which cannot be caught and ends the process (so these tests would end the test run).
public class NestingDepthTest
{
    // Deeper than any stack survived before the limit.
    private const int Deep = 10_000;

    private static string Repeat(string value, int count)
        => string.Concat(Enumerable.Repeat(value, count));

    private static ReadOnlySequence<byte> Split(byte[] bytes)
    {
        const int ChunkSize = 7;
        var first = new ByteSequenceSegment(bytes.AsMemory(0, Math.Min(ChunkSize, bytes.Length)));
        var last = first;
        for (var offset = ChunkSize; offset < bytes.Length; offset += ChunkSize)
        {
            last = last.AddNext(bytes.AsMemory(offset, Math.Min(ChunkSize, bytes.Length - offset)));
        }
        return new ReadOnlySequence<byte>(first, 0, last, last.Length);
    }

    private static void ShouldParse(string toml, CsTomlSerializerOptions? options = null)
    {
        var bytes = Encoding.UTF8.GetBytes(toml);
        CsTomlSerializer.Deserialize<TomlDocument>(bytes.AsSpan(), options);
        CsTomlSerializer.Deserialize<TomlDocument>(Split(bytes), options);
    }

    private static void ShouldFail(string toml, string message, CsTomlSerializerOptions? options = null)
    {
        var bytes = Encoding.UTF8.GetBytes(toml);
        ShouldBeSingleError(Should.Throw<CsTomlSerializeException>(() => CsTomlSerializer.Deserialize<TomlDocument>(bytes.AsSpan(), options)));
        ShouldBeSingleError(Should.Throw<CsTomlSerializeException>(() => CsTomlSerializer.Deserialize<TomlDocument>(Split(bytes), options)));

        void ShouldBeSingleError(CsTomlSerializeException exception)
        {
            var parseException = exception.ParseExceptions!.ShouldHaveSingleItem();
            parseException.LineNumber.ShouldBe(1);
            parseException.InnerException!.Message.ShouldContain(message);
        }
    }

    // The point is the stack size, not concurrency, so this is a Thread rather than a Task.
    private static void RunOnSmallStack(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception e) { failure = e; }
        }, 256 * 1024);
        thread.Start();
        thread.Join();
        failure.ShouldBeNull();
    }

    [Test]
    [Arguments("[", "]")]
    [Arguments("{a=", "}")]
    [Arguments("[{a=", "}]")]
    public void DeepNesting_IsReportedAsParseError(string open, string close)
    {
        ShouldFail($"x = {Repeat(open, Deep)}", "maximum depth of 64");
        ShouldFail($"x = {Repeat(open, Deep)}1{Repeat(close, Deep)}\n", "maximum depth of 64");
    }

    [Test]
    public void DeepNesting_OnSmallStack_IsReportedAsParseError()
    {
        RunOnSmallStack(() => ShouldFail($"x = {Repeat("[", Deep)}", "maximum depth of 64"));
    }

    [Test]
    public void DeepNesting_WithMaxDepthRaised_StopsBeforeStackOverflow()
    {
        var options = CsTomlSerializerOptions.Default with { MaxDepth = int.MaxValue };

        RunOnSmallStack(() => ShouldFail($"x = {Repeat("[{a=", Deep)}", "remaining stack space", options));
    }

    [Test]
    [Arguments("[", "]", 64)]
    [Arguments("{a=", "}", 64)]
    [Arguments("[{a=", "}]", 32)]
    public void DefaultMaxDepth_Allows64Levels(string open, string close, int count)
    {
        var value = $"{Repeat(open, count)}1{Repeat(close, count)}";

        ShouldParse($"x = {value}\n");
        ShouldFail($"x = [{value}]\n", "maximum depth of 64");
        ShouldFail($"x = {{a={value}}}\n", "maximum depth of 64");
    }

    [Test]
    public void MaxDepth_CanBeChanged()
    {
        var options = CsTomlSerializerOptions.Default with { MaxDepth = 2 };

        CsTomlSerializerOptions.Default.MaxDepth.ShouldBe(64);
        ShouldParse("x = [[1], {a=2}]\n", options);
        ShouldFail("x = [[[1]]]\n", "maximum depth of 2", options);
        ShouldFail("x = {a={a={a=1}}}\n", "maximum depth of 2", options);
        ShouldParse($"x = {Repeat("[", 65)}1{Repeat("]", 65)}\n", CsTomlSerializerOptions.Default with { MaxDepth = 65 });
        Should.Throw<ArgumentOutOfRangeException>(() => CsTomlSerializerOptions.Default with { MaxDepth = 0 });
    }

    [Test]
    public void DeepNesting_IsCollectedWithLineNumber_AndParsingContinues()
    {
        var toml = string.Join('\n',
            "a = 1",
            $"b = {Repeat("[", Deep)}",                     // 2: too deep
            $"c = {Repeat("[", 64)}1{Repeat("]", 64)}",     // 3: still allowed after the error above
            "d = @",                                        // 4: unrelated error
            "e = 2",
            "");

        var exception = Should.Throw<CsTomlSerializeException>(() => CsTomlSerializer.Deserialize<TomlDocument>(Encoding.UTF8.GetBytes(toml)));

        exception.ParseExceptions!.Select(e => e.LineNumber).ShouldBe([2L, 4L]);
        exception.ParseExceptions!.First().InnerException!.Message.ShouldContain("maximum depth of 64");
    }

    [Test]
    public void RealisticNesting_StillParses()
    {
        var toml = """
            matrix = [[[1, 2], [3, 4]], [[5, 6], [7, 8]]]
            points = [{ x = 1, y = { z = [1, 2, { w = "deep" }] } }, { x = 2, y = { z = [] } }]
            """u8.ToArray();

        var document = CsTomlSerializer.Deserialize<TomlDocument>(Split(toml));

        document.RootNode["matrix"u8][1][0][1].GetInt64().ShouldBe(6);
        document.RootNode["points"u8][0]["y"u8]["z"u8][2]["w"u8].GetString().ShouldBe("deep");
    }

    [Test]
    public void DeserializeValueType_DeepNesting_Throws()
    {
        var toml = Encoding.UTF8.GetBytes(Repeat("[", Deep));

        Assert.ThrowsExactly<CsTomlException>(() => CsTomlSerializer.DeserializeValueType<object>(toml)).Message.ShouldContain("maximum depth of 64");
        Assert.ThrowsExactly<CsTomlException>(() => CsTomlSerializer.DeserializeValueType<object>(Split(toml))).Message.ShouldContain("maximum depth of 64");
    }
}
