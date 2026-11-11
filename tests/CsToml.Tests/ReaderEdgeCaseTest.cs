using CsToml.Error;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Text;

namespace CsToml.Tests;

public class ReaderEdgeCaseTest
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

    private static void Parse(byte[] toml, Action<TomlDocument> assert, CsTomlSerializerOptions? options = null)
    {
        assert(CsTomlSerializer.Deserialize<TomlDocument>(toml.AsSpan(), options));
        assert(CsTomlSerializer.Deserialize<TomlDocument>(new ReadOnlySequence<byte>(toml), options));
    }

    private static void Parse(string toml, Action<TomlDocument> assert, CsTomlSerializerOptions? options = null)
        => Parse(Encoding.UTF8.GetBytes(toml), assert, options);

    private static void ShouldFail(byte[] toml, CsTomlSerializerOptions? options = null)
    {
        Should.Throw<CsTomlSerializeException>(() => CsTomlSerializer.Deserialize<TomlDocument>(toml.AsSpan(), options));
        Should.Throw<CsTomlSerializeException>(() => CsTomlSerializer.Deserialize<TomlDocument>(new ReadOnlySequence<byte>(toml), options));
    }

    private static void ShouldFail(string toml, CsTomlSerializerOptions? options = null)
        => ShouldFail(Encoding.UTF8.GetBytes(toml), options);

    private static void ShouldBeInteger(TomlDocumentNode node, long expected)
    {
        node.GetTomlValue().ShouldBeAssignableTo<TomlInteger>();
        node.GetInt64().ShouldBe(expected);
    }

    [Test]
    [Arguments("x = +0\r\n")]
    [Arguments("x = -0\r\n")]
    [Arguments("x = +0\r\ny = 1\r\n")]
    [Arguments("x = 0\r\n")]
    public void SignedZero_FollowedByCrLf(string toml)
    {
        Parse(toml, document => ShouldBeInteger(document.RootNode["x"u8], 0));
    }

    [Test]
    [Arguments("x = +0\ry = 1\n")]
    [Arguments("x = -0\r")]
    public void SignedZero_FollowedByBareCr_Throws(string toml)
    {
        ShouldFail(toml);
    }

    [Test]
    [Arguments("x = 123#note\n", 123L)]
    [Arguments("x = 1#see\n", 1L)]
    [Arguments("x = 42#1.5\n", 42L)]
    [Arguments("x = +12#e1\n", 12L)]
    [Arguments("x = -7#E\n", -7L)]
    [Arguments("x = 123 #note\n", 123L)]
    public void Integer_FollowedByComment(string toml, long expected)
    {
        Parse(toml, document => ShouldBeInteger(document.RootNode["x"u8], expected));
    }

    [Test]
    [Arguments("x = [1]#end\n", 1L)]
    [Arguments("x = [12]#e\n", 12L)]
    [Arguments("x = [+12]#e\n", 12L)]
    [Arguments("x = [-3]#.\n", -3L)]
    public void Integer_FollowedByClosingBracketAndComment(string toml, long expected)
    {
        Parse(toml, document => ShouldBeInteger(document.RootNode["x"u8][0], expected));
    }

    [Test]
    [Arguments("x = {a = 12}#e\n", 12L)]
    [Arguments("x = {a = +12}#e\n", 12L)]
    [Arguments("x = {a = -1}#.\n", -1L)]
    public void Integer_FollowedByClosingBraceAndComment(string toml, long expected)
    {
        Parse(toml, document => ShouldBeInteger(document.RootNode["x"u8]["a"u8], expected));
    }

    [Test]
    [Arguments("x = 0#comment\n")]
    [Arguments("x = 0#e\n")]
    [Arguments("x = +0#e\n")]
    [Arguments("x = -0#.\n")]
    [Arguments("x = 0 #comment\n")]
    public void Zero_FollowedByComment(string toml)
    {
        Parse(toml, document => ShouldBeInteger(document.RootNode["x"u8], 0));
    }

    [Test]
    [Arguments("x = 1979-05-27 07:32:00# c\n")]
    [Arguments("x = 1979-05-27 07:32:00\t# c\n")]
    [Arguments("x = 1979-05-27 07:32:00 # c\n")]
    [Arguments("x = 1979-05-27 07:32:00\r\n")]
    [Arguments("x = 1979-05-27 07:32:00")]
    public void SpaceDelimitedLocalDateTime_FollowedByTerminator(string toml)
    {
        Parse(toml, document => document.RootNode["x"u8].GetDateTime().ShouldBe(new DateTime(1979, 5, 27, 7, 32, 0)));
    }

    [Test]
    [Arguments("x = [1979-05-27 07:32:00]\n")]
    [Arguments("x = [1979-05-27 07:32:00, 1979-05-28 07:32:00]\n")]
    [Arguments("x = [ 1979-05-27 07:32:00 ]\n")]
    public void SpaceDelimitedLocalDateTime_InArray(string toml)
    {
        Parse(toml, document => document.RootNode["x"u8][0].GetDateTime().ShouldBe(new DateTime(1979, 5, 27, 7, 32, 0)));
    }

    [Test]
    [Arguments("x = { d = 1979-05-27 07:32:00}\n")]
    [Arguments("x = { d = 1979-05-27 07:32:00 }\n")]
    [Arguments("x = { d = 1979-05-27 07:32:00, e = 1 }\n")]
    public void SpaceDelimitedLocalDateTime_InInlineTable(string toml)
    {
        Parse(toml, document => document.RootNode["x"u8]["d"u8].GetDateTime().ShouldBe(new DateTime(1979, 5, 27, 7, 32, 0)));
    }

    [Test]
    [Arguments("x = [1979-05-27 07:32:00Z]\n")]
    [Arguments("x = [1979-05-27 07:32:00+00:00]\n")]
    public void SpaceDelimitedOffsetDateTime_InArray(string toml)
    {
        Parse(toml, document => document.RootNode["x"u8][0].GetDateTimeOffset().ShouldBe(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero)));
    }

    [Test]
    [Arguments("x = 1979-05-27 \n")]
    [Arguments("x = 1979-05-27 \r\n")]
    [Arguments("x = 1979-05-27 ")]
    [Arguments("x = 1979-05-27  # c\n")]
    [Arguments("x = 1979-05-27 # c\n")]
    [Arguments("x = 1979-05-27\t# c\n")]
    public void LocalDate_FollowedByWhiteSpace(string toml)
    {
        Parse(toml, document => document.RootNode["x"u8].GetDateOnly().ShouldBe(new DateOnly(1979, 5, 27)));
    }

    [Test]
    [Arguments("x = [1979-05-27 ]\n")]
    [Arguments("x = [1979-05-27 , 1979-05-28]\n")]
    [Arguments("x = [1979-05-27  ]\n")]
    public void LocalDate_FollowedByWhiteSpace_InArray(string toml)
    {
        Parse(toml, document => document.RootNode["x"u8][0].GetDateOnly().ShouldBe(new DateOnly(1979, 5, 27)));
    }

    [Test]
    public void SpaceDelimitedLocalDateTime_SplitAcrossSegments()
    {
        var sequence = CreateSequence(
            "x = [1979-05-27 "u8.ToArray(),
            "07:32:00]\ny = [1979-05-27 "u8.ToArray(),
            "]"u8.ToArray());

        var document = CsTomlSerializer.Deserialize<TomlDocument>(sequence);
        document.RootNode["x"u8][0].GetDateTime().ShouldBe(new DateTime(1979, 5, 27, 7, 32, 0));
        document.RootNode["y"u8][0].GetDateOnly().ShouldBe(new DateOnly(1979, 5, 27));
    }

    [Test]
    [Arguments("x = [1.5e-3, 2.5e-3]\n")]
    [Arguments("x = [1.5e-3 , 2.5e-3]\n")]
    [Arguments("x = [1.5E-3,2.5E-3,3.5E-3]\n")]
    public void Float_WithNegativeExponent_InArray(string toml)
    {
        Parse(toml, document =>
        {
            document.RootNode["x"u8][0].GetDouble().ShouldBe(1.5e-3);
            document.RootNode["x"u8][1].GetDouble().ShouldBe(2.5e-3);
        });
    }

    [Test]
    [Arguments("x = 123e-5        \n", 123e-5)]
    [Arguments("x = 1.5e-3\t\t\t\t\t\t\t\t\n", 1.5e-3)]
    public void Float_WithNegativeExponent_FollowedByPadding(string toml, double expected)
    {
        Parse(toml, document => document.RootNode["x"u8].GetDouble().ShouldBe(expected));
    }

    [Test]
    [Arguments("x = { a = 1.5e-3, b = 2 }\n")]
    [Arguments("x = { a = 1.5e-3 , b = 2 }\n")]
    public void Float_WithNegativeExponent_InInlineTable(string toml)
    {
        Parse(toml, document =>
        {
            document.RootNode["x"u8]["a"u8].GetDouble().ShouldBe(1.5e-3);
            document.RootNode["x"u8]["b"u8].GetInt64().ShouldBe(2);
        });
    }

    [Test]
    public void Float_WithNegativeExponent_SplitAcrossSegments()
    {
        var sequence = CreateSequence(
            "x = [1.5e"u8.ToArray(),
            "-3, 2.5e-3]\ny = 1"u8.ToArray());

        var document = CsTomlSerializer.Deserialize<TomlDocument>(sequence);
        document.RootNode["x"u8][0].GetDouble().ShouldBe(1.5e-3);
        document.RootNode["x"u8][1].GetDouble().ShouldBe(2.5e-3);
        document.RootNode["y"u8].GetInt64().ShouldBe(1);
    }

    [Test]
    [Arguments("x = inxinf\n")]
    [Arguments("x = nxxnan\n")]
    [Arguments("x = infinf\n")]
    [Arguments("x = nannan\n")]
    [Arguments("x = [inx, 1]\n")]
    [Arguments("x = ina\n")]
    public void InfOrNan_Misspelled_Throws(string toml)
    {
        ShouldFail(toml);
    }

    [Test]
    public void InfOrNan_SplitAcrossSegments()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(CreateSequence(
            "a = in"u8.ToArray(),
            "f\nb = n"u8.ToArray(),
            "an\nc = -i"u8.ToArray(),
            "nf\n"u8.ToArray()));

        document.RootNode["a"u8].GetDouble().ShouldBe(double.PositiveInfinity);
        double.IsNaN(document.RootNode["b"u8].GetDouble()).ShouldBeTrue();
        document.RootNode["c"u8].GetDouble().ShouldBe(double.NegativeInfinity);

        Should.Throw<CsTomlSerializeException>(() => CsTomlSerializer.Deserialize<TomlDocument>(CreateSequence(
            "a = in"u8.ToArray(),
            "xinf\n"u8.ToArray())));
    }

    [Test]
    [Arguments("x = 1985-06-18 17:04:07+12\n")]
    [Arguments("x = 1985-06-18T17:04:07-0\n")]
    [Arguments("x = 1985-06-18T17:04:07+\n")]
    [Arguments("x = 1985-06-18T17:04:07+09:0\n")]
    [Arguments("x = 1985-06-18T17:04:07+09:000\n")]
    [Arguments("x = 1985-06-18T17:04:07.5+09\n")]
    public void OffsetDateTime_MalformedOffset_Throws(string toml)
    {
        ShouldFail(toml);
        ShouldFail(toml, Options.TomlSpecVersion110);
    }

    [Test]
    [Arguments("x = 1985-06-18T17:04+12\n")]
    [Arguments("x = 1985-06-18T17:04+\n")]
    [Arguments("x = 1985-06-18T17:04+09:0\n")]
    [Arguments("x = 1985-06-18T17:04.5+12\n")]
    [Arguments("x = 1985-06-18T17:04.5+\n")]
    public void OffsetDateTime_SecondsOmitted_MalformedOffset_Throws(string toml)
    {
        ShouldFail(toml, Options.TomlSpecVersion110);
    }

    [Test]
    public void OffsetDateTime_WithOffset()
    {
        Parse("x = 1985-06-18T17:04:07+09:00\n", document =>
            document.RootNode["x"u8].GetDateTimeOffset().ShouldBe(new DateTimeOffset(1985, 6, 18, 17, 4, 7, TimeSpan.FromHours(9))));
        Parse("x = 1985-06-18T17:04+09:00\n", document =>
            document.RootNode["x"u8].GetDateTimeOffset().ShouldBe(new DateTimeOffset(1985, 6, 18, 17, 4, 0, TimeSpan.FromHours(9))),
            Options.TomlSpecVersion110);
    }

    [Test]
    [Arguments("x = 1979-05-27T07:32xx\n")]
    [Arguments("x = 1979-05-27T07:32x\n")]
    [Arguments("x = 1979-05-27T07:32:5\n")]
    [Arguments("x = 1979-05-27T07:32xxZ\n")]
    [Arguments("x = 1979-05-27T07:32xZ\n")]
    public void DateTime_SecondsOmitted_TrailingGarbage_Throws(string toml)
    {
        ShouldFail(toml, Options.TomlSpecVersion110);
    }

    [Test]
    public void DateTime_SecondsOmitted()
    {
        Parse("x = 1979-05-27T07:32\n", document =>
            document.RootNode["x"u8].GetDateTime().ShouldBe(new DateTime(1979, 5, 27, 7, 32, 0)),
            Options.TomlSpecVersion110);
        Parse("x = 1979-05-27T07:32Z\n", document =>
            document.RootNode["x"u8].GetDateTimeOffset().ShouldBe(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero)),
            Options.TomlSpecVersion110);
    }

    [Test]
    public void LiteralStringKey_InvalidUtf8_Throws()
    {
        ShouldFail([(byte)'\'', 0xFF, 0xFE, (byte)'\'', (byte)' ', (byte)'=', (byte)' ', (byte)'1', (byte)'\n']);
        ShouldFail([(byte)'a', (byte)'.', (byte)'\'', 0xC3, (byte)'\'', (byte)' ', (byte)'=', (byte)' ', (byte)'1', (byte)'\n']);
        ShouldFail([(byte)'[', (byte)'\'', 0xFF, (byte)'\'', (byte)']', (byte)'\n']);
        ShouldFail([(byte)'[', (byte)'[', (byte)'\'', 0xFF, (byte)'\'', (byte)']', (byte)']', (byte)'\n']);
        ShouldFail([(byte)'x', (byte)'=', (byte)'{', (byte)'\'', 0xFF, (byte)'\'', (byte)'=', (byte)'1', (byte)'}', (byte)'\n']);
    }

    [Test]
    public void LiteralString_InvalidUtf8_Throws()
    {
        ShouldFail([(byte)'"', 0xFF, 0xFE, (byte)'"', (byte)' ', (byte)'=', (byte)' ', (byte)'1', (byte)'\n']);
        ShouldFail([(byte)'x', (byte)' ', (byte)'=', (byte)' ', (byte)'\'', 0xFF, 0xFE, (byte)'\'', (byte)'\n']);
    }

    [Test]
    public void LiteralStringKey_ValidUtf8()
    {
        Parse("'日本語' = 1\n", document => document.RootNode["日本語"].GetInt64().ShouldBe(1));
    }

    [Test]
    [Arguments("t = { a.b = # c\n1 }\n")]
    [Arguments("t = { a.b.c = # c\n1 }\n")]
    [Arguments("t = { a.b = # c\n# d\n1 }\n")]
    [Arguments("t = { a = # c\n1 }\n")]
    public void InlineTable_CommentBeforeValue_Throws(string toml)
    {
        ShouldFail(toml);
        ShouldFail(toml, Options.TomlSpecVersion110);
    }

    [Test]
    public void InlineTable_DottedKey_WithNewlinesAndComments()
    {
        Parse("t = { # c\n a.b = 1, # c\n c.d = 2 # c\n }\n", document =>
        {
            document.RootNode["t"u8]["a"u8]["b"u8].GetInt64().ShouldBe(1);
            document.RootNode["t"u8]["c"u8]["d"u8].GetInt64().ShouldBe(2);
        }, Options.TomlSpecVersion110);
    }

    [Test]
    [Arguments("x = 1_0e-1_0_0\n", 10e-100)]
    [Arguments("x = 1_0E-1_0_0\n", 10e-100)]
    [Arguments("x = 1_0e-1_0_0 # c\n", 10e-100)]
    [Arguments("x = 123e-00005\n", 123e-5)]
    [Arguments("x = 1.5e-00003\n", 1.5e-3)]
    [Arguments("x = 1.0e-10000\n", 0d)]
    public void Float_WithNegativeExponent_AsLongAsLocalDate(string toml, double expected)
    {
        Parse(toml, document => document.RootNode["x"u8].GetDouble().ShouldBe(expected));
    }

    [Test]
    [Arguments("x = [1_0e-1_0_0, 2_0e-1_0_0]\n")]
    [Arguments("x = [1_0e-1_0_0,2_0e-1_0_0]\n")]
    [Arguments("x = [ 1_0E-1_0_0 , 2_0E-1_0_0 ]\n")]
    public void Float_WithNegativeExponent_AsLongAsLocalDate_InArray(string toml)
    {
        Parse(toml, document =>
        {
            document.RootNode["x"u8][0].GetDouble().ShouldBe(10e-100);
            document.RootNode["x"u8][1].GetDouble().ShouldBe(20e-100);
        });
    }

    [Test]
    public void Float_WithNegativeExponent_AsLongAsLocalDate_SplitAcrossSegments()
    {
        var sequence = CreateSequence(
            "x = [1_0e"u8.ToArray(),
            "-1_0_0, 2_0e-1_0_0]\ny = 1"u8.ToArray());

        var document = CsTomlSerializer.Deserialize<TomlDocument>(sequence);
        document.RootNode["x"u8][0].GetDouble().ShouldBe(10e-100);
        document.RootNode["x"u8][1].GetDouble().ShouldBe(20e-100);
        document.RootNode["y"u8].GetInt64().ShouldBe(1);
    }

    [Test]
    [Arguments("x = 1979-05-27T07:32:00.\n")]
    [Arguments("x = 1979-05-27T07:32:00.Z\n")]
    [Arguments("x = 1979-05-27T07:32:00.+09:00\n")]
    [Arguments("x = 07:32:00.\n")]
    public void DateTime_FractionWithoutDigits_Throws(string toml)
    {
        ShouldFail(toml);
        ShouldFail(toml, Options.TomlSpecVersion110);
    }

    [Test]
    [Arguments("x = 1979-05-27T07:32.\n")]
    [Arguments("x = 1979-05-27T07:32.Z\n")]
    [Arguments("x = 1979-05-27T07:32.+09:00\n")]
    [Arguments("x = 07:32.\n")]
    public void DateTime_SecondsOmitted_FractionWithoutDigits_Throws(string toml)
    {
        ShouldFail(toml, Options.TomlSpecVersion110);
    }

    [Test]
    public void OffsetDateTime_WithFractionAndOffset()
    {
        Parse("x = 1979-05-27T07:32:00.5+09:00\n", document =>
            document.RootNode["x"u8].GetDateTimeOffset().ShouldBe(new DateTimeOffset(1979, 5, 27, 7, 32, 0, 500, TimeSpan.FromHours(9))));
    }
}
