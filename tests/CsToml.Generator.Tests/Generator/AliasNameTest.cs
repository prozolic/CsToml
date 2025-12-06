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

public class AliasNameListTest
{
    private AliasNameList aliasNameList;

    public AliasNameListTest()
    {
        aliasNameList = new AliasNameList()
        {
            Name = "alias name list",
            DisplayName = "Alias Name List",
            Values = [
                new AliasName()
                {
                    BareKey = "this is ba-re_Key",
                    Empty = @"this is """"",
                    Hiragana = "this is あいうえお",
                    IpAddress = "this is 127.0.0.1",
                    Url = "this is https://github.com/prozolic/CsToml",
                    Literal = @"this is <\i\c*\s*\\>"
                },
                new AliasName()
                {
                    BareKey = "this is ba-re_Key",
                    Empty = @"this is """"",
                    Hiragana = "this is あいうえお",
                    IpAddress = "this is 127.0.0.1",
                    Url = "this is https://github.com/prozolic/CsToml",
                    Literal = @"this is <\i\c*\s*\\>"
                }
            ]
        };
    }

    [Fact]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(aliasNameList);
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Name = \"alias name list\"");
        writer.AppendLine("DISPLAYNAME = \"Alias Name List\"");
        writer.AppendLine("Values = [ {ba-re_Key = \"this is ba-re_Key\", \"\" = 'this is \"\"', \"あいうえお\" = \"this is あいうえお\", \"127.0.0.1\" = \"this is 127.0.0.1\", \"https://github.com/prozolic/CsToml\" = \"this is https://github.com/prozolic/CsToml\", '<\\i\\c*\\s*\\\\>' = 'this is <\\i\\c*\\s*\\\\>'}, {ba-re_Key = \"this is ba-re_Key\", \"\" = 'this is \"\"', \"あいうえお\" = \"this is あいうえお\", \"127.0.0.1\" = \"this is 127.0.0.1\", \"https://github.com/prozolic/CsToml\" = \"this is https://github.com/prozolic/CsToml\", '<\\i\\c*\\s*\\\\>' = 'this is <\\i\\c*\\s*\\\\>'} ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(aliasNameList, Option.Header);
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Name = \"alias name list\"");
        writer.AppendLine("DISPLAYNAME = \"Alias Name List\"");
        writer.AppendLine("Values = [ {ba-re_Key = \"this is ba-re_Key\", \"\" = 'this is \"\"', \"あいうえお\" = \"this is あいうえお\", \"127.0.0.1\" = \"this is 127.0.0.1\", \"https://github.com/prozolic/CsToml\" = \"this is https://github.com/prozolic/CsToml\", '<\\i\\c*\\s*\\\\>' = 'this is <\\i\\c*\\s*\\\\>'}, {ba-re_Key = \"this is ba-re_Key\", \"\" = 'this is \"\"', \"あいうえお\" = \"this is あいうえお\", \"127.0.0.1\" = \"this is 127.0.0.1\", \"https://github.com/prozolic/CsToml\" = \"this is https://github.com/prozolic/CsToml\", '<\\i\\c*\\s*\\\\>' = 'this is <\\i\\c*\\s*\\\\>'} ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(aliasNameList, Option.ArrayHeader);
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Name = \"alias name list\"");
        writer.AppendLine("DISPLAYNAME = \"Alias Name List\"");
        writer.AppendLine("[[Values]]");
        writer.AppendLine("ba-re_Key = \"this is ba-re_Key\"");
        writer.AppendLine("\"\" = 'this is \"\"'");
        writer.AppendLine("\"あいうえお\" = \"this is あいうえお\"");
        writer.AppendLine("\"127.0.0.1\" = \"this is 127.0.0.1\"");
        writer.AppendLine("\"https://github.com/prozolic/CsToml\" = \"this is https://github.com/prozolic/CsToml\"");
        writer.AppendLine("'<\\i\\c*\\s*\\\\>' = 'this is <\\i\\c*\\s*\\\\>'");
        writer.AppendLine();
        writer.AppendLine("[[Values]]");
        writer.AppendLine("ba-re_Key = \"this is ba-re_Key\"");
        writer.AppendLine("\"\" = 'this is \"\"'");
        writer.AppendLine("\"あいうえお\" = \"this is あいうえお\"");
        writer.AppendLine("\"127.0.0.1\" = \"this is 127.0.0.1\"");
        writer.AppendLine("\"https://github.com/prozolic/CsToml\" = \"this is https://github.com/prozolic/CsToml\"");
        writer.AppendLine("'<\\i\\c*\\s*\\\\>' = 'this is <\\i\\c*\\s*\\\\>'");
        writer.AppendLine();
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(aliasNameList, Option.HeaderAndArrayHeader);
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Name = \"alias name list\"");
        writer.AppendLine("DISPLAYNAME = \"Alias Name List\"");
        writer.AppendLine("[[Values]]");
        writer.AppendLine("ba-re_Key = \"this is ba-re_Key\"");
        writer.AppendLine("\"\" = 'this is \"\"'");
        writer.AppendLine("\"あいうえお\" = \"this is あいうえお\"");
        writer.AppendLine("\"127.0.0.1\" = \"this is 127.0.0.1\"");
        writer.AppendLine("\"https://github.com/prozolic/CsToml\" = \"this is https://github.com/prozolic/CsToml\"");
        writer.AppendLine("'<\\i\\c*\\s*\\\\>' = 'this is <\\i\\c*\\s*\\\\>'");
        writer.AppendLine();
        writer.AppendLine("[[Values]]");
        writer.AppendLine("ba-re_Key = \"this is ba-re_Key\"");
        writer.AppendLine("\"\" = 'this is \"\"'");
        writer.AppendLine("\"あいうえお\" = \"this is あいうえお\"");
        writer.AppendLine("\"127.0.0.1\" = \"this is 127.0.0.1\"");
        writer.AppendLine("\"https://github.com/prozolic/CsToml\" = \"this is https://github.com/prozolic/CsToml\"");
        writer.AppendLine("'<\\i\\c*\\s*\\\\>' = 'this is <\\i\\c*\\s*\\\\>'");
        writer.AppendLine();
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Name = \"alias name list\"");
            writer.AppendLine("DISPLAYNAME = \"Alias Name List\"");
            writer.AppendLine("Values = [ {ba-re_Key = \"this is ba-re_Key\", \"\" = 'this is \"\"', \"あいうえお\" = \"this is あいうえお\", \"127.0.0.1\" = \"this is 127.0.0.1\", \"https://github.com/prozolic/CsToml\" = \"this is https://github.com/prozolic/CsToml\", '<\\i\\c*\\s*\\\\>' = 'this is <\\i\\c*\\s*\\\\>'}, {ba-re_Key = \"this is ba-re_Key\", \"\" = 'this is \"\"', \"あいうえお\" = \"this is あいうえお\", \"127.0.0.1\" = \"this is 127.0.0.1\", \"https://github.com/prozolic/CsToml\" = \"this is https://github.com/prozolic/CsToml\", '<\\i\\c*\\s*\\\\>' = 'this is <\\i\\c*\\s*\\\\>'} ]");
            writer.Flush();

            var aliasNameList = CsTomlSerializer.Deserialize<AliasNameList>(buffer.WrittenSpan);
            Validate(aliasNameList);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Name = \"alias name list\"");
            writer.AppendLine("DISPLAYNAME = \"Alias Name List\"");
            writer.AppendLine("[[Values]]");
            writer.AppendLine("ba-re_Key = \"this is ba-re_Key\"");
            writer.AppendLine("\"\" = 'this is \"\"'");
            writer.AppendLine("\"あいうえお\" = \"this is あいうえお\"");
            writer.AppendLine("\"127.0.0.1\" = \"this is 127.0.0.1\"");
            writer.AppendLine("\"https://github.com/prozolic/CsToml\" = \"this is https://github.com/prozolic/CsToml\"");
            writer.AppendLine("'<\\i\\c*\\s*\\\\>' = 'this is <\\i\\c*\\s*\\\\>'");
            writer.AppendLine();
            writer.AppendLine("[[Values]]");
            writer.AppendLine("ba-re_Key = \"this is ba-re_Key\"");
            writer.AppendLine("\"\" = 'this is \"\"'");
            writer.AppendLine("\"あいうえお\" = \"this is あいうえお\"");
            writer.AppendLine("\"127.0.0.1\" = \"this is 127.0.0.1\"");
            writer.AppendLine("\"https://github.com/prozolic/CsToml\" = \"this is https://github.com/prozolic/CsToml\"");
            writer.AppendLine("'<\\i\\c*\\s*\\\\>' = 'this is <\\i\\c*\\s*\\\\>'");
            writer.AppendLine();
            writer.Flush();

            var aliasNameList = CsTomlSerializer.Deserialize<AliasNameList>(buffer.WrittenSpan);
            Validate(aliasNameList);
        }

        static void Validate(AliasNameList aliasNameList)
        {
            aliasNameList.ShouldNotBeNull();
            aliasNameList.Name.ShouldBe("alias name list");
            aliasNameList.DisplayName.ShouldBe("Alias Name List");
            aliasNameList.Values.Count.ShouldBe(2);

            aliasNameList.Values[0].BareKey.ShouldBe("this is ba-re_Key");
            aliasNameList.Values[0].Empty.ShouldBe(@"this is """"");
            aliasNameList.Values[0].Hiragana.ShouldBe("this is あいうえお");
            aliasNameList.Values[0].IpAddress.ShouldBe("this is 127.0.0.1");
            aliasNameList.Values[0].Url.ShouldBe("this is https://github.com/prozolic/CsToml");
            aliasNameList.Values[0].Literal.ShouldBe(@"this is <\i\c*\s*\\>");

            aliasNameList.Values[1].BareKey.ShouldBe("this is ba-re_Key");
            aliasNameList.Values[1].Empty.ShouldBe(@"this is """"");
            aliasNameList.Values[1].Hiragana.ShouldBe("this is あいうえお");
            aliasNameList.Values[1].IpAddress.ShouldBe("this is 127.0.0.1");
            aliasNameList.Values[1].Url.ShouldBe("this is https://github.com/prozolic/CsToml");
            aliasNameList.Values[1].Literal.ShouldBe(@"this is <\i\c*\s*\\>");
        }
    }
}
