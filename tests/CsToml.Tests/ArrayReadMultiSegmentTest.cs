using CsToml.Error;
using CsToml.Utility;
using System.Buffers;

namespace CsToml.Tests;

public class ArrayReadMultiSegmentTest
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

    private static TomlDocument DeserializeAndCompare(params byte[][] chunks)
    {
        var whole = chunks.SelectMany(c => c).ToArray();
        var singleSegment = CsTomlSerializer.Deserialize<TomlDocument>(whole);
        var multiSegment = CsTomlSerializer.Deserialize<TomlDocument>(CreateSequence(chunks));
        multiSegment.LineNumber.ShouldBe(singleSegment.LineNumber);
        return multiSegment;
    }

    [Fact]
    public void Elements_SplitAcrossTwoSegments()
    {
        var document = DeserializeAndCompare(
            "value = [ 1, 2"u8.ToArray(),
            ", 3 ]"u8.ToArray());

        document.RootNode["value"u8][0].GetInt64().ShouldBe(1);
        document.RootNode["value"u8][1].GetInt64().ShouldBe(2);
        document.RootNode["value"u8][2].GetInt64().ShouldBe(3);
    }

    [Fact]
    public void DelimiterRun_ExhaustsSegment()
    {
        // The first segment ends inside a run of spaces, driving the
        // Advance(unreadSpan.Length) segment-exhausted path.
        var document = DeserializeAndCompare(
            "value = [ 1,   "u8.ToArray(),
            "   2 ]"u8.ToArray());

        document.RootNode["value"u8][0].GetInt64().ShouldBe(1);
        document.RootNode["value"u8][1].GetInt64().ShouldBe(2);
    }

    [Fact]
    public void ClosingBracket_IsFirstByteOfNextSegment()
    {
        var document = DeserializeAndCompare(
            "value = [ 1, 2 "u8.ToArray(),
            "]"u8.ToArray());

        document.RootNode["value"u8][1].GetInt64().ShouldBe(2);
    }

    [Fact]
    public void ValueStart_IsFirstByteOfNextSegment()
    {
        var document = DeserializeAndCompare(
            "value = [ 1, "u8.ToArray(),
            "22 ]"u8.ToArray());

        document.RootNode["value"u8][1].GetInt64().ShouldBe(22);
    }

    [Fact]
    public void LineFeed_IsLastByteOfSegment()
    {
        // LF as the final byte of a segment drives the Advance + IncreaseLineNumber sync path.
        var document = DeserializeAndCompare(
            "value = [ 1,\n"u8.ToArray(),
            "2 ]\nafter = 3"u8.ToArray());

        document.RootNode["value"u8][1].GetInt64().ShouldBe(2);
        document.RootNode["after"u8].GetInt64().ShouldBe(3);
        document.LineNumber.ShouldBe(3);
    }

    [Fact]
    public void MultilineArray_CountsLines_SingleSegmentEquivalence()
    {
        var document = DeserializeAndCompare(
            "value = [\n  1,\n  2,\n"u8.ToArray(),
            "  3,\n]\nafter = 4"u8.ToArray());

        document.RootNode["value"u8][2].GetInt64().ShouldBe(3);
        document.RootNode["after"u8].GetInt64().ShouldBe(4);
        document.LineNumber.ShouldBe(6);
    }

    [Fact]
    public void CrLf_SplitAcrossSegments()
    {
        // CR as the final byte of a segment drives the reader-synced CR fallback.
        var document = DeserializeAndCompare(
            "value = [ 1,\r"u8.ToArray(),
            "\n2 ]"u8.ToArray());

        document.RootNode["value"u8][1].GetInt64().ShouldBe(2);
    }

    [Fact]
    public void NestedArray_AtSegmentBoundary()
    {
        var document = DeserializeAndCompare(
            "value = [ [ 1, 2 ], "u8.ToArray(),
            "[ 3 ] ]"u8.ToArray());

        document.RootNode["value"u8][0][1].GetInt64().ShouldBe(2);
        document.RootNode["value"u8][1][0].GetInt64().ShouldBe(3);
    }

    [Fact]
    public void CommentInsideArray_SplitAcrossSegments()
    {
        var document = DeserializeAndCompare(
            "value = [ 1, # com"u8.ToArray(),
            "ment\n2 ]"u8.ToArray());

        document.RootNode["value"u8][0].GetInt64().ShouldBe(1);
        document.RootNode["value"u8][1].GetInt64().ShouldBe(2);
    }

    [Fact]
    public void TrailingComma_BeforeSegmentBoundary()
    {
        var document = DeserializeAndCompare(
            "value = [ 1, 2,"u8.ToArray(),
            " ]"u8.ToArray());

        document.RootNode["value"u8][1].GetInt64().ShouldBe(2);
    }

    [Fact]
    public void DoubleComma_AcrossSegments_Throws()
    {
        var sequence = CreateSequence(
            "value = [ 1,"u8.ToArray(),
            ", 2 ]"u8.ToArray());

        Should.Throw<CsTomlSerializeException>(() => CsTomlSerializer.Deserialize<TomlDocument>(sequence));
    }

    [Fact]
    public void UnclosedArray_EndsAtSegmentBoundary_Throws()
    {
        var sequence = CreateSequence(
            "value = [ 1, "u8.ToArray(),
            "2 "u8.ToArray());

        Should.Throw<CsTomlSerializeException>(() => CsTomlSerializer.Deserialize<TomlDocument>(sequence));
    }
}
