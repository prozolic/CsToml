namespace CsToml.Tests;

public class TrailingZeroTest
{
    [Fact]
    public void ZeroAtEndOfFileWithoutNewLine()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>("v = 0"u8);
        document.RootNode["v"].GetInt64().ShouldBe(0);
    }

    [Fact]
    public void ZeroAtEndOfFileWithNewLine()
    {
        CsTomlSerializer.Deserialize<TomlDocument>("v = 0\n"u8).RootNode["v"].GetInt64().ShouldBe(0);
        CsTomlSerializer.Deserialize<TomlDocument>("v = 0\r\n"u8).RootNode["v"].GetInt64().ShouldBe(0);
    }

    [Fact]
    public void ZeroFollowedByWhiteSpaceAtEndOfFile()
    {
        CsTomlSerializer.Deserialize<TomlDocument>("v = 0 "u8).RootNode["v"].GetInt64().ShouldBe(0);
        CsTomlSerializer.Deserialize<TomlDocument>("v = 0\t"u8).RootNode["v"].GetInt64().ShouldBe(0);
    }

    [Fact]
    public void SignedZeroAtEndOfFileWithoutNewLine()
    {
        CsTomlSerializer.Deserialize<TomlDocument>("v = +0"u8).RootNode["v"].GetInt64().ShouldBe(0);
        CsTomlSerializer.Deserialize<TomlDocument>("v = -0"u8).RootNode["v"].GetInt64().ShouldBe(0);
    }

    [Fact]
    public void NonZeroAtEndOfFileWithoutNewLine()
    {
        CsTomlSerializer.Deserialize<TomlDocument>("v = 1"u8).RootNode["v"].GetInt64().ShouldBe(1);
        CsTomlSerializer.Deserialize<TomlDocument>("v = 10"u8).RootNode["v"].GetInt64().ShouldBe(10);
        CsTomlSerializer.Deserialize<TomlDocument>("v = 0x1"u8).RootNode["v"].GetInt64().ShouldBe(1);
        CsTomlSerializer.Deserialize<TomlDocument>("v = 0o7"u8).RootNode["v"].GetInt64().ShouldBe(7);
        CsTomlSerializer.Deserialize<TomlDocument>("v = 0b1"u8).RootNode["v"].GetInt64().ShouldBe(1);
        CsTomlSerializer.Deserialize<TomlDocument>("v = 0.0"u8).RootNode["v"].GetDouble().ShouldBe(0.0);
        CsTomlSerializer.Deserialize<TomlDocument>("v = 0e0"u8).RootNode["v"].GetDouble().ShouldBe(0.0);
    }

    [Fact]
    public void ZeroIsTheLastValueOfMultipleKeyValues()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>("a = 1\nb = 0"u8);

        document.RootNode["a"].GetInt64().ShouldBe(1);
        document.RootNode["b"].GetInt64().ShouldBe(0);
    }

    [Fact]
    public void ZeroAtEndOfFileInTable()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>("[table]\nv = 0"u8);

        document.RootNode["table"]["v"].GetInt64().ShouldBe(0);
    }

    [Fact]
    public void ZeroAtEndOfFileWithDottedKey()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>("a.b = 0"u8);

        document.RootNode["a"]["b"].GetInt64().ShouldBe(0);
    }

    [Fact]
    public void ZeroAtEndOfArrayWithoutNewLine()
    {
        var array = CsTomlSerializer.Deserialize<TomlDocument>("v = [0]"u8).RootNode["v"];
        array.GetArray().Count.ShouldBe(1);
        array[0].GetInt64().ShouldBe(0);

        var array2 = CsTomlSerializer.Deserialize<TomlDocument>("v = [1, 0]"u8).RootNode["v"];
        array2.GetArray().Count.ShouldBe(2);
        array2[0].GetInt64().ShouldBe(1);
        array2[1].GetInt64().ShouldBe(0);

        CsTomlSerializer.Deserialize<TomlDocument>("v = [ 0 ]"u8).RootNode["v"][0].GetInt64().ShouldBe(0);
        CsTomlSerializer.Deserialize<TomlDocument>("v = [[0]]"u8).RootNode["v"][0][0].GetInt64().ShouldBe(0);
    }

    [Fact]
    public void ZeroAtEndOfInlineTableWithoutNewLine()
    {
        CsTomlSerializer.Deserialize<TomlDocument>("v = {a = 0}"u8).RootNode["v"]["a"].GetInt64().ShouldBe(0);
        CsTomlSerializer.Deserialize<TomlDocument>("v = { a = 0 }"u8).RootNode["v"]["a"].GetInt64().ShouldBe(0);
        CsTomlSerializer.Deserialize<TomlDocument>("v = {a = 1, b = 0}"u8).RootNode["v"]["b"].GetInt64().ShouldBe(0);
        CsTomlSerializer.Deserialize<TomlDocument>("v = {a = {b = 0}}"u8).RootNode["v"]["a"]["b"].GetInt64().ShouldBe(0);
    }

    [Fact]
    public void ZeroAtEndOfFileIsSerializedBack()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>("v = 0"u8);

        using var serializedText = CsTomlSerializer.Serialize(document);
        CsTomlSerializer.Deserialize<TomlDocument>(serializedText.ByteSpan).RootNode["v"].GetInt64().ShouldBe(0);
    }
}
