using CsToml.Error;
using CsToml.Utility;
using System.Buffers;
using System.Text;

namespace CsToml.Tests;

public class Utf8HelperTest
{
    [Test]
    [Arguments("abc-_09")]
    [Arguments("café")]
    [Arguments("²")]
    [Arguments("日本語")]
    [Arguments("\U00010000")]
    public void UnquotedKey_AllowedCharacters(string key)
    {
        Utf8Helper.ContainInvalidSequencesInUnquotedKey(Encoding.UTF8.GetBytes(key)).ShouldBeFalse();
    }

    [Test]
    [Arguments("a b")]
    [Arguments("a.b")]
    [Arguments("×")]
    [Arguments(";")]
    [Arguments("　")]
    [Arguments("")]
    [Arguments("\U000F0000")]
    public void UnquotedKey_DisallowedCharacters(string key)
    {
        Utf8Helper.ContainInvalidSequencesInUnquotedKey(Encoding.UTF8.GetBytes(key)).ShouldBeTrue();
    }

    [Test]
    [Arguments("80")]
    [Arguments("C3")]
    [Arguments("61E381")]
    [Arguments("F09080")]
    [Arguments("C0AF")]
    [Arguments("E080AF")]
    [Arguments("EDA080")]
    [Arguments("F4908080")]
    [Arguments("FF")]
    public void UnquotedKey_InvalidUtf8(string hex)
    {
        Utf8Helper.ContainInvalidSequencesInUnquotedKey(Convert.FromHexString(hex)).ShouldBeTrue();
    }

    [Test]
    [Arguments(0x00, "00")]
    [Arguments(0x7F, "7F")]
    [Arguments(0x80, "C280")]
    [Arguments(0x7FF, "DFBF")]
    [Arguments(0x800, "E0A080")]
    [Arguments(0xD7FF, "ED9FBF")]
    [Arguments(0xE000, "EE8080")]
    [Arguments(0xFFFF, "EFBFBF")]
    [Arguments(0x10000, "F0908080")]
    [Arguments(0x10FFFF, "F48FBFBF")]
    public void ParseFromCodePointToUtf8(int codePoint, string expected)
    {
        var destination = new byte[4];
        Utf8Helper.ParseFromCodePointToUtf8(codePoint, destination, out var written);
        Convert.ToHexString(destination, 0, written).ShouldBe(expected);
    }

    [Test]
    [Arguments(-1)]
    [Arguments(0xD800)]
    [Arguments(0xDFFF)]
    [Arguments(0x110000)]
    public void ParseFromCodePointToUtf8_NotScalarValue_Throws(int codePoint)
    {
        Should.Throw<CsTomlException>(() => Utf8Helper.ParseFromCodePointToUtf8(codePoint, new byte[4], out _));
    }

    [Test]
    [Arguments("41", "41")]
    [Arguments("e9", "C3A9")]
    [Arguments("FF", "C3BF")]
    [Arguments("0041", "41")]
    [Arguments("00E9", "C3A9")]
    [Arguments("3042", "E38182")]
    [Arguments("00000041", "41")]
    [Arguments("0001F600", "F09F9880")]
    [Arguments("0010ffff", "F48FBFBF")]
    public void ParseFromHexDigitsToUtf8(string digits, string expected)
    {
        var destination = new byte[4];
        var written = ParseHexDigits(destination, Encoding.ASCII.GetBytes(digits));
        Convert.ToHexString(destination, 0, written).ShouldBe(expected);
    }

    [Test]
    [Arguments("G0")]
    [Arguments("00G0")]
    [Arguments("D800")]
    [Arguments("DFFF")]
    [Arguments("0000G000")]
    [Arguments("0000D800")]
    [Arguments("00110000")]
    [Arguments("FFFFFFFF")]
    public void ParseFromHexDigitsToUtf8_Invalid_Throws(string digits)
    {
        var source = Encoding.ASCII.GetBytes(digits);
        Should.Throw<CsTomlException>(() => ParseHexDigits(new byte[4], source));
    }

    private static int ParseHexDigits(byte[] destination, byte[] source)
    {
        int written;
        switch (source.Length)
        {
            case 2:
                Utf8Helper.ParseFrom8bitCodePointToUtf8(destination, source, out written);
                break;
            case 4:
                Utf8Helper.ParseFrom16bitCodePointToUtf8(destination, source, out written);
                break;
            default:
                Utf8Helper.ParseFrom32bitCodePointToUtf8(destination, source, out written);
                break;
        }
        return written;
    }

    [Test]
    [Arguments(0x00, true)]
    [Arguments(0x08, true)]
    [Arguments(0x09, false)]
    [Arguments(0x0A, true)]
    [Arguments(0x1F, true)]
    [Arguments(0x20, false)]
    [Arguments(0x7E, false)]
    [Arguments(0x7F, true)]
    [Arguments(0x80, false)]
    public void ContainsEscapeChar(int value, bool expected)
    {
        Utf8Helper.ContainsEscapeChar([(byte)'a', (byte)value, (byte)'b']).ShouldBe(expected);
    }

    [Test]
    [Arguments("")]
    [Arguments("plain ASCII text")]
    [Arguments("José, 日本語, \U0001F600")]
    public void Utf16RoundTrip(string text)
    {
        ShouldRoundTrip(text);
    }

    [Test]
    public void Utf16RoundTrip_LongerThanStackBuffer()
    {
        ShouldRoundTrip(string.Concat(Enumerable.Repeat("aé日\U0001F600", 300)));
    }

    private static void ShouldRoundTrip(string text)
    {
        var expected = Encoding.UTF8.GetBytes(text);

        var bufferWriter = new ArrayBufferWriter<byte>();
        Utf8Helper.FromUtf16(bufferWriter, text);
        bufferWriter.WrittenSpan.ToArray().ShouldBe(expected);

        var destination = new byte[expected.Length];
        Utf8Helper.FromUtf16(text, destination, out var charsRead, out var bytesWritten);
        charsRead.ShouldBe(text.Length);
        bytesWritten.ShouldBe(expected.Length);
        destination.ShouldBe(expected);

        Utf8Helper.ToUtf16(expected).ShouldBe(text);
    }

    [Test]
    public void FromUtf16_LoneSurrogate_Throws()
    {
        var text = "a" + (char)0xD800 + "b";
        Should.Throw<CsTomlException>(() => Utf8Helper.FromUtf16(new ArrayBufferWriter<byte>(), text));
        Should.Throw<CsTomlException>(() => Utf8Helper.FromUtf16(text, new byte[16], out _, out _));
    }

    [Test]
    public void FromUtf16_DestinationTooSmall_Throws()
    {
        Should.Throw<CsTomlException>(() => Utf8Helper.FromUtf16("abc", new byte[2], out _, out _));
    }

    [Test]
    [Arguments("FF")]
    [Arguments("C3")]
    [Arguments("EDA080")]
    public void ToUtf16_InvalidUtf8_Throws(string hex)
    {
        Should.Throw<CsTomlException>(() => Utf8Helper.ToUtf16(Convert.FromHexString(hex)));
    }

    [Test]
    [Arguments(@"a = ""\uD800""")]
    [Arguments(@"a = ""\U0000DFFF""")]
    [Arguments(@"a = """"""\uD800""""""")]
    [Arguments(@"""\uD800"" = 1")]
    public void SurrogateEscape_Throws(string toml)
    {
        Should.Throw<CsTomlSerializeException>(() => CsTomlSerializer.Deserialize<TomlDocument>(Encoding.UTF8.GetBytes(toml)));
    }
}
