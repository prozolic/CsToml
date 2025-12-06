using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeZeroTest
{
    private TypeZero type;

    public TypeZeroTest()
    {
        type = new TypeZero();
    }

    [Fact]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(type);
        bytes.ByteSpan.ToArray().ShouldBe(""u8.ToArray());
    }

    [Fact]
    public void SerializeWithHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(type, Option.Header);
        bytes.ByteSpan.ToArray().ShouldBe(""u8.ToArray());
    }

    [Fact]
    public void SerializeWithArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(type, Option.ArrayHeader);
        bytes.ByteSpan.ToArray().ShouldBe(""u8.ToArray());
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(type, Option.HeaderAndArrayHeader);
        bytes.ByteSpan.ToArray().ShouldBe(""u8.ToArray());
    }

    [Fact]
    public void Deserialize()
    {
        var type = CsTomlSerializer.Deserialize<TypeZero>(""u8);
        type.ShouldNotBeNull();
    }
}

public class TypeOneTest
{
    private TypeOne type;

    public TypeOneTest()
    {
        type = new TypeOne
        {
            Value = 999
        };
    }

    [Fact]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(type);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderOption()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.Flush();
        var expected = buffer.ToArray();

        using var bytes = CsTomlSerializer.Serialize(type, Option.Header);
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithArrayHeaderOption()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.Flush();
        var expected = buffer.ToArray();

        using var bytes = CsTomlSerializer.Serialize(type, Option.ArrayHeader);
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.Flush();
        var expected = buffer.ToArray();

        using var bytes = CsTomlSerializer.Serialize(type, Option.HeaderAndArrayHeader);
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        var type = CsTomlSerializer.Deserialize<TypeOne>(@"Value = 999"u8);
        type.Value.ShouldBe(999);
    }
}

public class TypeTwoTest
{
    private TypeTwo type;

    public TypeTwoTest()
    {
        type = new TypeTwo
        {
            Value = 999,
            Value2 = 123
        };
    }

    [Fact]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(type);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Value2 = 123");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Value2 = 123");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(type, Option.ArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Value2 = 123");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(type, Option.HeaderAndArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Value2 = 123");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Value2 = 123");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeTwo>(buffer.WrittenSpan);
        type.Value.ShouldBe(999);
        type.Value2.ShouldBe(123);
    }
}