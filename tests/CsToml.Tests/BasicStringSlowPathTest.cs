using CsToml.Error;
using CsToml.Utility;
using System.Buffers;
using System.Text;

namespace CsToml.Tests;

public class BasicStringSlowPathTest
{
    private static ReadOnlySequence<byte> CreateSequence(params byte[][] chunks)
    {
        var first = new ByteSequenceSegment(chunks[0]);
        var last = first;
        for (var i = 1; i < chunks.Length; i++)
        {
            last = last.AddNext(chunks[i]);
        }
        return new ReadOnlySequence<byte>(first, 0, last, last.Length);
    }

    [Fact]
    public void PlainContent_SplitAcrossTwoSegments()
    {
        var sequence = CreateSequence(
            "value = \"abcde"u8.ToArray(),
            "fghij\""u8.ToArray());

        var document = CsTomlSerializer.Deserialize<TomlDocument>(sequence);

        document!.RootNode["value"u8].GetString().ShouldBe("abcdefghij");
    }

    [Fact]
    public void PlainContent_SplitAcrossManySegments()
    {
        var sequence = CreateSequence(
            "value = \"ab"u8.ToArray(),
            "cd"u8.ToArray(),
            "ef"u8.ToArray(),
            "gh\""u8.ToArray());

        var document = CsTomlSerializer.Deserialize<TomlDocument>(sequence);

        document!.RootNode["value"u8].GetString().ShouldBe("abcdefgh");
    }

    [Fact]
    public void ClosingQuote_IsFirstByteOfNextSegment()
    {
        var sequence = CreateSequence(
            "value = \"abcde"u8.ToArray(),
            "\""u8.ToArray());

        var document = CsTomlSerializer.Deserialize<TomlDocument>(sequence);

        document!.RootNode["value"u8].GetString().ShouldBe("abcde");
    }

    [Fact]
    public void EscapeSequence_PrecededByPlainContentSplitAcrossSegments()
    {
        // "sta" | "rt" + \n(escape) + "end" + closing quote
        var sequence = CreateSequence(
            "value = \"sta"u8.ToArray(),
            "rt\\nend\""u8.ToArray());

        var document = CsTomlSerializer.Deserialize<TomlDocument>(sequence);

        document!.RootNode["value"u8].GetString().ShouldBe("start\nend");
    }

    [Fact]
    public void EscapeSequence_BackslashAtSegmentBoundary()
    {
        // "ab" + '\' | 'n' + "cd" + closing quote
        var sequence = CreateSequence(
            "value = \"ab\\"u8.ToArray(),
            "ncd\""u8.ToArray());

        var document = CsTomlSerializer.Deserialize<TomlDocument>(sequence);

        document!.RootNode["value"u8].GetString().ShouldBe("ab\ncd");
    }

    [Fact]
    public void UnescapedControlCharacter_InSecondSegment()
    {
        var chunk2 = new byte[] { 0x01 }.Concat("cd\""u8.ToArray()).ToArray();
        var sequence = CreateSequence(
            "value = \"ab"u8.ToArray(),
            chunk2);

        Should.Throw<CsTomlSerializeException>(() =>
        {
            CsTomlSerializer.Deserialize<TomlDocument>(sequence);
        });
    }

    [Fact]
    public void UnterminatedString_AcrossMultipleSegments()
    {
        var sequence = CreateSequence(
            "value = \"abc"u8.ToArray(),
            "def"u8.ToArray());

        Should.Throw<CsTomlSerializeException>(() =>
        {
            CsTomlSerializer.Deserialize<TomlDocument>(sequence);
        });
    }

    [Fact]
    public void MultiByteUtf8Character_SplitAcrossSegmentBoundary()
    {
        // U+00E9 ('e' with acute accent) as UTF-8: 0xC3 0xA9, split between the two bytes.
        var chunk1 = "value = \"caf"u8.ToArray().Concat(new byte[] { 0xC3 }).ToArray();
        var chunk2 = new byte[] { 0xA9 }.Concat("\""u8.ToArray()).ToArray();
        var sequence = CreateSequence(chunk1, chunk2);

        var document = CsTomlSerializer.Deserialize<TomlDocument>(sequence);

        var expected = Encoding.UTF8.GetString([(byte)'c', (byte)'a', (byte)'f', 0xC3, 0xA9]);
        document!.RootNode["value"u8].GetString().ShouldBe(expected);
    }

    [Fact]
    public void InvalidUtf8AfterEscapeSequence_Throws()
    {
        // \n (escape, forces the slow path) followed by a lone UTF-8 continuation byte
        // (0x80), which is only checked once the slow path's accumulated buffer is
        // validated at the end -- distinct from the fast path's check on a raw slice.
        var bytes = "value = \""u8.ToArray()
            .Concat("\\n"u8.ToArray())
            .Concat(new byte[] { 0x80 })
            .Concat("\""u8.ToArray())
            .ToArray();

        Should.Throw<CsTomlSerializeException>(() =>
        {
            CsTomlSerializer.Deserialize<TomlDocument>(bytes);
        });
    }
}
