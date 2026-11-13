using CsToml.Error;
using CsToml.Utility;
using System.Buffers;
using System.Text;

namespace CsToml.Tests;

public class InlineTableCommentTest
{
    private static readonly int[] SegmentSizes = [1, 7];

    private static readonly byte[][] InvalidCommentBytes = [[0x00], [0x01], [0x7F], [0x0D], [0xFF], [0xFF, 0xFE]];

    private static ReadOnlySequence<byte> CreateSequence(byte[] toml, int segmentSize)
    {
        var chunks = toml.Chunk(segmentSize).ToArray();
        var first = new ByteSequenceSegment(chunks[0]);
        var last = first;
        for (var i = 1; i < chunks.Length; i++)
        {
            last = last.AddNext(chunks[i]);
        }
        return new ReadOnlySequence<byte>(first, 0, last, last.Length);
    }

    private static void ShouldParse(string toml, string expected, CsTomlSerializerOptions options)
    {
        var bytes = Encoding.UTF8.GetBytes(toml);
        var expectedJson = CsTomlSerializer.Deserialize<TomlDocument>(Encoding.UTF8.GetBytes(expected)).ToJsonObject();

        var document = CsTomlSerializer.Deserialize<TomlDocument>(bytes.AsSpan(), options);
        JsonNodeExtensions.DeepEqualsForTomlFormat(expectedJson, document.ToJsonObject()).ShouldBeTrue();
        foreach (var size in SegmentSizes)
        {
            document = CsTomlSerializer.Deserialize<TomlDocument>(CreateSequence(bytes, size), options);
            JsonNodeExtensions.DeepEqualsForTomlFormat(expectedJson, document.ToJsonObject()).ShouldBeTrue();
        }
    }

    private static CsTomlSerializeException ShouldFail(byte[] toml, CsTomlSerializerOptions? options)
    {
        foreach (var size in SegmentSizes)
        {
            Should.Throw<CsTomlSerializeException>(() => CsTomlSerializer.Deserialize<TomlDocument>(CreateSequence(toml, size), options));
        }
        return Should.Throw<CsTomlSerializeException>(() => CsTomlSerializer.Deserialize<TomlDocument>(toml.AsSpan(), options));
    }

    private static CsTomlSerializeException ShouldFail(string toml, CsTomlSerializerOptions? options)
        => ShouldFail(Encoding.UTF8.GetBytes(toml), options);

    private static byte[] Fill(string template, byte[] bytes)
    {
        var utf8 = Encoding.UTF8.GetBytes(template);
        var index = Array.IndexOf(utf8, (byte)'@');
        return [.. utf8.AsSpan(0, index), .. bytes, .. utf8.AsSpan(index + 1)];
    }

    [Test]
    [Arguments("a = {#c\n}\n", "a = {}")]
    [Arguments("a = {#c\r\n}\r\n", "a = {}")]
    [Arguments("t = {\n# c\n}\n", "t = {}")]
    [Arguments("a = {#c\n\nb = 1}\n", "a = {b = 1}")]
    [Arguments("t = {# c\n\t\n a = 1}\n", "t = {a = 1}")]
    [Arguments("t = {# c\r\n\r\n a = 1}\n", "t = {a = 1}")]
    [Arguments("t = {# c\n# d\n\n# e\n a = 1}\n", "t = {a = 1}")]
    [Arguments("a = {b = 1 #c\n, c = 2}\n", "a = {b = 1, c = 2}")]
    [Arguments("a = {b = 1 #c\r\n, c = 2}\r\n", "a = {b = 1, c = 2}")]
    [Arguments("t = {\n  a = 1\n  # c\n  , b = 2\n}\n", "t = {a = 1, b = 2}")]
    [Arguments("t = {a = 1 # c\n # d\n , b = 2 }\n", "t = {a = 1, b = 2}")]
    [Arguments("t = {# 1\n\n# 2\n a = 1 # 3\n\n# 4\n , # 5\n\n# 6\n b = 2 # 7\n\n# 8\n}\n", "t = {a = 1, b = 2}")]
    [Arguments("t = {a = 1 # c\n,}\n", "t = {a = 1}")]
    [Arguments("t = {a = 1 # c\n, # d\n}\n", "t = {a = 1}")]
    [Arguments("t = {#c\n\n u = {#d\n}, v = {x = 1 #e\n, y = 2} #f\n}\n", "t = {u = {}, v = {x = 1, y = 2}}")]
    [Arguments("a = [{#c\n}, {b = 1 #c\n, c = 2}]\n", "a = [{}, {b = 1, c = 2}]")]
    [Arguments("t = {#\t日本語 }, {\n a = 1 #\té\n, b = 2}\n", "t = {a = 1, b = 2}")]
    public void Comment(string toml, string expected)
    {
        ShouldParse(toml, expected, Options.TomlSpecVersion110);
    }

    [Test]
    [Arguments("t = {#comment\n\t# comment\n\tc = 1,#comment\n\t#comment\n}#comment\n", "t = {c = 1}")]
    [Arguments("a = {b = 1 # c\n}\n", "a = {b = 1}")]
    [Arguments("a = {b = 1, # c\n c = 2}\n", "a = {b = 1, c = 2}")]
    [Arguments("a = {\n\n}\n", "a = {}")]
    [Arguments("a = {\n a = 1\n , b = 2}\n", "a = {a = 1, b = 2}")]
    [Arguments("a = {a = 1, # c\n\n b = 2}\n", "a = {a = 1, b = 2}")]
    [Arguments("a = {a = 1 # c\n\n}\n", "a = {a = 1}")]
    [Arguments("a = {a = 1,\n\n# c\n\n# d\n\n}\n", "a = {a = 1}")]
    public void Comment_AlreadyAcceptedForms(string toml, string expected)
    {
        ShouldParse(toml, expected, Options.TomlSpecVersion110);
    }

    [Test]
    [Arguments("a = {#c\n}\n")]
    [Arguments("a = {\n}\n")]
    [Arguments("a = {b = 1 #c\n}\n")]
    [Arguments("a = {b = 1 #c\n, c = 2}\n")]
    [Arguments("a = {b = 1,}\n")]
    public void Spec100_Throws(string toml)
    {
        ShouldFail(toml, null).ParseExceptions!.First().LineNumber.ShouldBe(1);
    }

    [Test]
    public void IndividualSpecFlags()
    {
        var newlinesOnly = CsTomlSerializerOptions.Default with { Spec = new TomlSpec { AllowNewlinesInInlineTables = true } };
        var trailingCommaOnly = CsTomlSerializerOptions.Default with { Spec = new TomlSpec { AllowTrailingCommaInInlineTables = true } };

        ShouldParse("a = {#c\n\n b = 1 #d\n, c = 2 #e\n}\n", "a = {b = 1, c = 2}", newlinesOnly);
        ShouldFail("a = {b = 1 #c\n,}\n", newlinesOnly);
        ShouldFail("a = {#c\n}\n", trailingCommaOnly);
    }

    [Test]
    [Arguments("a = {,}\n")]
    [Arguments("a = {a=1,,b=2}\n")]
    [Arguments("a = {a=1 b=2}\n")]
    [Arguments("a = {a = 1 #c\n")]
    [Arguments("a = {#c")]
    [Arguments("a = {#c\n, a = 1}\n")]
    [Arguments("a = {a = 1, #c\n, b = 2}\n")]
    [Arguments("a = {a = 1 #c\n b = 2}\n")]
    public void Malformed_Throws(string toml)
    {
        ShouldFail(toml, Options.TomlSpecVersion110);
    }

    [Test]
    [Arguments("t = {# x@y\n a = 1}\n", 1)]
    [Arguments("t = {# x@\r\n a = 1}\r\n", 1)]
    [Arguments("t = {# x@ y = 9,\n a = 1}\n", 1)]
    [Arguments("a = {\n# x@y\n b = 1 }\n", 2)]
    [Arguments("t = {a = 1, # x@y\n b = 2}\n", 1)]
    [Arguments("t = {a = 1 # x@y\n}\n", 1)]
    [Arguments("t = {a = 1 # x@y\n, b = 2}\n", 1)]
    public void Comment_InvalidContent_Throws(string template, int commentLine)
    {
        foreach (var bytes in InvalidCommentBytes)
        {
            var topLevel = ShouldFail(Fill("# x@y\n", bytes), Options.TomlSpecVersion110).ParseExceptions!.First();
            var inlineTable = ShouldFail(Fill(template, bytes), Options.TomlSpecVersion110).ParseExceptions!.First();

            inlineTable.LineNumber.ShouldBe(commentLine);
            inlineTable.InnerException!.Message.ShouldBe(topLevel.InnerException!.Message);
        }
    }

    [Test]
    [Arguments("t = {\n# c\n\n a = 1\n # d\n , b = 2 # e\n}\nx = $\n", 8)]
    [Arguments("t = {\r\n# c\r\n\r\n a = 1\r\n # d\r\n , b = 2 # e\r\n}\r\nx = $\r\n", 8)]
    [Arguments("t = {\n a = 1, # c\n\n b = 2 # d\n}\nx = $\n", 6)]
    public void LineNumber_OfErrorAfterInlineTable(string toml, int errorLine)
    {
        var exception = ShouldFail(toml, Options.TomlSpecVersion110);

        exception.ParseExceptions!.Count.ShouldBe(1);
        exception.ParseExceptions!.First().LineNumber.ShouldBe(errorLine);
    }
}
