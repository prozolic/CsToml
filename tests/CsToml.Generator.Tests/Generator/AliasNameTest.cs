using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests.Seirialization;

public class AliasNameTest
{
    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("ba-re_Key = \"this is ba-re_Key\"");
        writer.AppendLine("\"\" = 'this is \"\"'");
        writer.AppendLine("\"あいうえお\" = \"this is あいうえお\"");
        writer.AppendLine("\"127.0.0.1\" = \"this is 127.0.0.1\"");
        writer.AppendLine("\"https://github.com/prozolic/CsToml\" = \"this is https://github.com/prozolic/CsToml\"");
        writer.AppendLine("'<\\i\\c*\\s*\\\\>' = 'this is <\\i\\c*\\s*\\\\>'");
        writer.Flush();

        var aliasName = CsTomlSerializer.Deserialize<AliasName>(buffer.WrittenSpan);
        aliasName.BareKey.ShouldBe("this is ba-re_Key");
        aliasName.Empty.ShouldBe(@"this is """"");
        aliasName.Hiragana.ShouldBe("this is あいうえお");
        aliasName.IpAddress.ShouldBe("this is 127.0.0.1");
        aliasName.Url.ShouldBe("this is https://github.com/prozolic/CsToml");
        aliasName.Literal.ShouldBe(@"this is <\i\c*\s*\\>");
    }

    [Fact]
    public void Serialize()
    {
        var aliasName = new AliasName()
        {
            BareKey = "this is ba-re_Key",
            Empty = @"this is """"",
            Hiragana = "this is あいうえお",
            IpAddress = "this is 127.0.0.1",
            Url = "this is https://github.com/prozolic/CsToml",
            Literal = @"this is <\i\c*\s*\\>"
        };
        using var bytes = CsTomlSerializer.Serialize(aliasName);
        var aliasName2 = CsTomlSerializer.Deserialize<AliasName>(bytes.ByteSpan);
        aliasName.Equals(aliasName2).ShouldBeTrue();

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("ba-re_Key = \"this is ba-re_Key\"");
        writer.AppendLine("\"\" = 'this is \"\"'");
        writer.AppendLine("\"あいうえお\" = \"this is あいうえお\"");
        writer.AppendLine("\"127.0.0.1\" = \"this is 127.0.0.1\"");
        writer.AppendLine("\"https://github.com/prozolic/CsToml\" = \"this is https://github.com/prozolic/CsToml\"");
        writer.AppendLine("'<\\i\\c*\\s*\\\\>' = 'this is <\\i\\c*\\s*\\\\>'");
        writer.Flush();
        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }
}
