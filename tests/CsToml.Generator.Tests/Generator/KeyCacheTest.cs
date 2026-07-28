using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class KeyCacheTest
{
    [Fact]
    public void SerializeWithHeaderOptionWhenSamePropertyNameWithoutAliasName()
    {
        var target = new KeyCacheWithoutAliasName() { Conflict = new KeyCacheInner() { Inner = new KeyCacheLeaf() { Value = "value" } } };
        using var bytes = CsTomlSerializer.Serialize(target, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[Conflict]");
        writer.AppendLine("[Conflict.Inner]");
        writer.AppendLine("Value = \"value\"");
        writer.Flush();

        bytes.ByteSpan.ToArray().ShouldBe(buffer.ToArray());
    }

    [Fact]
    public void SerializeWithHeaderOptionWhenSamePropertyNameWithAliasName()
    {
        var target = new KeyCacheWithAliasName() { Conflict = new KeyCacheInner() { Inner = new KeyCacheLeaf() { Value = "value" } } };
        using var bytes = CsTomlSerializer.Serialize(target, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[conflict]");
        writer.AppendLine("[conflict.Inner]");
        writer.AppendLine("Value = \"value\"");
        writer.Flush();

        bytes.ByteSpan.ToArray().ShouldBe(buffer.ToArray());
    }

    [Fact]
    public void SerializeWithHeaderOptionWhenKeyIsNotCSharpIdentifier()
    {
        var target = new KeyCacheEscapedKey() { Escaped = new KeyCacheLeaf() { Value = "value" } };
        using var bytes = CsTomlSerializer.Serialize(target, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[ba-re_Key]");
        writer.AppendLine("Value = \"value\"");
        writer.Flush();

        bytes.ByteSpan.ToArray().ShouldBe(buffer.ToArray());
    }

    [Fact]
    public void SerializeWithHeaderOptionWhenKeysDifferOnlyByEscapedCharacter()
    {
        var target = new KeyCacheEscapedKey2() { Hyphen = new KeyCacheLeaf() { Value = "hyphen" }, Underscore = new KeyCacheLeaf() { Value = "underscore" } };
        using var bytes = CsTomlSerializer.Serialize(target, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[a-b]");
        writer.AppendLine("Value = \"hyphen\"");
        writer.AppendLine("[a_b]");
        writer.AppendLine("Value = \"underscore\"");
        writer.Flush();

        bytes.ByteSpan.ToArray().ShouldBe(buffer.ToArray());
    }
}

[TomlSerializedObject]
public partial class KeyCacheWithoutAliasName
{
    [TomlValueOnSerialized]
    public KeyCacheInner? Conflict { get; set; }
}

[TomlSerializedObject]
public partial class KeyCacheWithAliasName
{
    [TomlValueOnSerialized(AliasName = "conflict")]
    public KeyCacheInner? Conflict { get; set; }
}

[TomlSerializedObject]
public partial class KeyCacheEscapedKey
{
    [TomlValueOnSerialized(AliasName = "ba-re_Key")]
    public KeyCacheLeaf? Escaped { get; set; }
}

[TomlSerializedObject]
public partial class KeyCacheEscapedKey2
{
    [TomlValueOnSerialized(AliasName = "a-b")]
    public KeyCacheLeaf? Hyphen { get; set; }

    [TomlValueOnSerialized(AliasName = "a_b")]
    public KeyCacheLeaf? Underscore { get; set; }
}

[TomlSerializedObject]
public partial class KeyCacheInner
{
    [TomlValueOnSerialized]
    public KeyCacheLeaf? Inner { get; set; }
}

[TomlSerializedObject]
public partial class KeyCacheLeaf
{
    [TomlValueOnSerialized]
    public string? Value { get; set; }
}
