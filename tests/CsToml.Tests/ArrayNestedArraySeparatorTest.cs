using CsToml.Error;
using CsToml.Utility;
using System.Buffers;
using System.Text;

namespace CsToml.Tests;

public class ArrayNestedArraySeparatorTest
{
    private const string NotSeparatedByCommas = "Values are not separated by commas.";

    private static ReadOnlySequence<byte> OneByteSegments(byte[] bytes)
    {
        var first = new ByteSequenceSegment(bytes[..1]);
        var last = first;
        for (var i = 1; i < bytes.Length; i++)
        {
            last = last.AddNext(bytes[i..(i + 1)]);
        }
        return new ReadOnlySequence<byte>(first, 0, last, last.Length);
    }

    private static void ShouldFail(string toml, long line, string message, CsTomlSerializerOptions? options = null)
    {
        var bytes = Encoding.UTF8.GetBytes(toml);
        Verify(Should.Throw<CsTomlSerializeException>(() => CsTomlSerializer.Deserialize<TomlDocument>(bytes.AsSpan(), options)));
        Verify(Should.Throw<CsTomlSerializeException>(() => CsTomlSerializer.Deserialize<TomlDocument>(OneByteSegments(bytes), options)));

        void Verify(CsTomlSerializeException exception)
        {
            var parseException = exception.ParseExceptions!.ShouldHaveSingleItem();
            parseException.LineNumber.ShouldBe(line);
            parseException.InnerException!.Message.ShouldBe(message);
        }
    }

    private static void ShouldParse(string toml, string expected, CsTomlSerializerOptions? options = null)
    {
        var bytes = Encoding.UTF8.GetBytes(toml);
        Verify(CsTomlSerializer.Deserialize<TomlDocument>(bytes.AsSpan(), options));
        Verify(CsTomlSerializer.Deserialize<TomlDocument>(OneByteSegments(bytes), options));

        void Verify(TomlDocument document)
            => document.RootNode["x"u8].GetTomlValue().ToString().ShouldBe(expected);
    }

    [Test]
    [Arguments("x = [[1][2]]\n", 1)]
    [Arguments("x = [[1] [2]]\n", 1)]
    [Arguments("x = [[1]\n[2]]\n", 2)]
    [Arguments("x = [[1]\r\n[2]]\r\n", 2)]
    [Arguments("x = [[1] # c\n [2]]\n", 2)]
    [Arguments("x = [[][]]\n", 1)]
    [Arguments("x = [\"a\"[1]]\n", 1)]
    [Arguments("x = [{}[1]]\n", 1)]
    [Arguments("x = [1 [2]]\n", 1)]
    [Arguments("x = [[[1][2]],[3]]\n", 1)]
    [Arguments("x = [[[1],[2]][3]]\n", 1)]
    [Arguments("t = { a = [[1][2]] }\n", 1)]
    public void MissingCommaBeforeNestedArray_Throws(string toml, int line)
    {
        ShouldFail(toml, line, NotSeparatedByCommas);
        ShouldFail(toml, line, NotSeparatedByCommas, Options.TomlSpecVersion110);
    }

    [Test]
    public void MissingCommaBeforeNestedArray_InMultilineInlineTable_V110_Throws()
    {
        ShouldFail("t = {\n a = [[1]\n[2]] }\n", 3, NotSeparatedByCommas, Options.TomlSpecVersion110);
    }

    [Test]
    public void MissingCommaBeforeNestedArray_ReportsEachLine()
    {
        var exception = Should.Throw<CsTomlSerializeException>(
            () => CsTomlSerializer.Deserialize<TomlDocument>("x = [[1]\n[2]]\ny = 1\nz = [3 [4]]\n"u8));

        exception.ParseExceptions!.Select(e => e.LineNumber).ShouldBe([2L, 4L]);
    }

    [Test]
    [Arguments("x = [[1],[2]]\n", "[[1], [2]]")]
    [Arguments("x = [ [1] , [2] , ]\n", "[[1], [2]]")]
    [Arguments("x = [[]]\n", "[[]]")]
    [Arguments("x = [[[1]]]\n", "[[[1]]]")]
    [Arguments("x = [\n[1]\n,\n[2]\n]\n", "[[1], [2]]")]
    [Arguments("x = [[1] # c\n, [2]]\n", "[[1], [2]]")]
    [Arguments("x = [1, [2]]\n", "[1, [2]]")]
    public void NestedArraysSeparatedByComma_Parse(string toml, string expected)
    {
        ShouldParse(toml, expected);
        ShouldParse(toml, expected, Options.TomlSpecVersion110);
    }

    [Test]
    [Arguments("x = [,[1]]\n", "The comma is defined first.")]
    [Arguments("x = [[1],,[2]]\n", "Commas are used more than once.")]
    [Arguments("x = [[1],[2]", "The array is not closed with closing brackets.")]
    public void OtherSeparatorErrors_StillThrow(string toml, string message)
    {
        ShouldFail(toml, 1, message);
        ShouldFail(toml, 1, message, Options.TomlSpecVersion110);
    }
}
