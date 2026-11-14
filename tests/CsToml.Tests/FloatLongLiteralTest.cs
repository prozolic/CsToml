using CsToml.Error;
using System.Text;

namespace CsToml.Tests;

public class FloatLongLiteralTest
{
    private static double Parse(string literal)
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(Encoding.UTF8.GetBytes($"v = {literal}"));
        return document.RootNode["v"].GetDouble();
    }

    [Test]
    [Arguments(62)]
    [Arguments(63)]
    public void StackBufferBoundary(int fractionDigits)
    {
        Parse("0." + new string('1', fractionDigits)).ShouldBe(0.1111111111111111);
    }

    [Test]
    public void BufferFullAtDot()
    {
        Parse("1" + new string('0', 63) + ".5").ShouldBe(1e63);
    }

    [Test]
    public void BufferFullAtExponent()
    {
        Parse("-0." + new string('7', 61) + "E5").ShouldBe(-77777.77777777778);
    }

    [Test]
    public void BufferFullAtExponentSign()
    {
        Parse("0." + new string('7', 61) + "e-5").ShouldBe(7.777777777777777e-06);
    }

    [Test]
    public void ThousandsOfDigits()
    {
        Parse("0." + new string('3', 5000)).ShouldBe(0.3333333333333333);
    }

    [Test]
    public void DotAtTheEnd_Throws()
    {
        var exception = Should.Throw<CsTomlSerializeException>(() => Parse(new string('1', 70) + "."));
        exception.ParseExceptions!.Single().InnerException!.Message.ShouldBe("Dot is used at the end.");
    }
}
