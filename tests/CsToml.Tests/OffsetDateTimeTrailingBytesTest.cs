using CsToml.Error;
using CsToml.Utility;
using System.Buffers;
using System.Text;

namespace CsToml.Tests;

public class OffsetDateTimeTrailingBytesTest
{
    private static byte[] CreateToml(string value)
        => Encoding.UTF8.GetBytes($"a = {value}\nb = [{value}]\nc = {{d = {value}}}\ne = {value}#c\nf = {value}");

    private static ReadOnlySequence<byte> Split(byte[] toml)
    {
        var first = new ByteSequenceSegment(toml.AsMemory(0, 1));
        var last = first;
        for (var i = 1; i < toml.Length; i++)
        {
            last = last.AddNext(toml.AsMemory(i, 1));
        }
        return new ReadOnlySequence<byte>(first, 0, last, last.Length);
    }

    private static void ShouldFail(string value, CsTomlSerializerOptions? options = null)
    {
        var toml = CreateToml(value);
        Should.Throw<CsTomlSerializeException>(() => CsTomlSerializer.Deserialize<TomlDocument>(toml.AsSpan(), options))
            .ParseExceptions!.Count.ShouldBe(5);
        Should.Throw<CsTomlSerializeException>(() => CsTomlSerializer.Deserialize<TomlDocument>(Split(toml), options))
            .ParseExceptions!.Count.ShouldBe(5);
    }

    private static void ShouldBe(string value, string expected, CsTomlSerializerOptions? options = null)
    {
        var toml = CreateToml(value);
        TomlDocument[] documents =
        [
            CsTomlSerializer.Deserialize<TomlDocument>(toml.AsSpan(), options),
            CsTomlSerializer.Deserialize<TomlDocument>(Split(toml), options),
        ];

        foreach (var document in documents)
        {
            document.RootNode["a"u8].GetDateTimeOffset().ToString("o").ShouldBe(expected);
            document.RootNode["b"u8][0].GetDateTimeOffset().ToString("o").ShouldBe(expected);
            document.RootNode["c"u8]["d"u8].GetDateTimeOffset().ToString("o").ShouldBe(expected);
            document.RootNode["e"u8].GetDateTimeOffset().ToString("o").ShouldBe(expected);
            document.RootNode["f"u8].GetDateTimeOffset().ToString("o").ShouldBe(expected);
        }
    }

    [Test]
    [Arguments("1979-05-27T07:32:00xZ")]
    [Arguments("1979-05-27T07:32:000Z")]
    [Arguments("1979-05-27T07:32:00ZZ")]
    [Arguments("1979-05-27T07:32:00-Z")]
    [Arguments("1979-05-27T07:32:00+09:00Z")]
    [Arguments("1979-05-27 07:32:00xz")]
    [Arguments("1979-05-27T07:32:00abcdefZ")]
    public void OffsetDateTime_BytesBetweenSecondsAndZ_Throws(string value)
    {
        ShouldFail(value);
        ShouldFail(value, Options.TomlSpecVersion110);
    }

    [Test]
    [Arguments("1979-05-27T07:32:00.5+09:00:00")]
    [Arguments("1979-05-27T07:32:00.5+09:00123")]
    [Arguments("1979-05-27T07:32:00.123456+09:00+09:00")]
    [Arguments("1979-05-27 07:32:00.5-07:00:00")]
    [Arguments("1979-05-27T07:32:00.5+09:000")]
    [Arguments("1979-05-27T07:32:00.5-09:00x")]
    public void OffsetDateTime_BytesAfterNumOffset_Throws(string value)
    {
        ShouldFail(value);
        ShouldFail(value, Options.TomlSpecVersion110);
    }

    [Test]
    [Arguments("1979-05-27T07:32.5+09:00:00")]
    [Arguments("1979-05-27T07:32.5+09:000")]
    [Arguments("1979-05-27 07:32.5-07:00x")]
    public void OffsetDateTime_SecondsOmitted_BytesAfterNumOffset_Throws(string value)
    {
        ShouldFail(value, Options.TomlSpecVersion110);
    }

    [Test]
    [Arguments("1979-05-27T07:32:00Z", "1979-05-27T07:32:00.0000000+00:00")]
    [Arguments("1979-05-27 07:32:00z", "1979-05-27T07:32:00.0000000+00:00")]
    [Arguments("1979-05-27t07:32:00.25+09:00", "1979-05-27T07:32:00.2500000+09:00")]
    [Arguments("1979-05-27 07:32:00.123456-07:00", "1979-05-27T07:32:00.1234560-07:00")]
    public void OffsetDateTime_SplitAcrossSegments(string value, string expected)
    {
        ShouldBe(value, expected);
        ShouldBe(value, expected, Options.TomlSpecVersion110);
    }

    [Test]
    [Arguments("1979-05-27T07:32.5+09:00", "1979-05-27T07:32:00.5000000+09:00")]
    [Arguments("1979-05-27 07:32.123456-07:00", "1979-05-27T07:32:00.1234560-07:00")]
    public void OffsetDateTime_SecondsOmitted_WithFraction(string value, string expected)
    {
        ShouldBe(value, expected, Options.TomlSpecVersion110);
    }
}
