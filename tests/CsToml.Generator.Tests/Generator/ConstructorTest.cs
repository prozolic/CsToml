using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class ConstructorTest
{
    [Fact]
    public void Serialize()
    {
        var primitive = new Constructor
        {
            Str = @"I'm a string.",
            Long = 123,
            Float = 123.456,
            Boolean = true,
        };

        using var bytes = CsTomlSerializer.Serialize(primitive);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("Long = 123");
        writer.AppendLine("Float = 123.456");
        writer.AppendLine("Boolean = true");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("Long = 123");
        writer.AppendLine("Float = 123.456");
        writer.AppendLine("Boolean = true");
        writer.Flush();

        var primitive = CsTomlSerializer.Deserialize<Constructor>(buffer.WrittenSpan);
        primitive.Str.ShouldBe("I'm a string.");
        primitive.Long.ShouldBe(123);
        primitive.Float.ShouldBe(123.456);
        primitive.Boolean.ShouldBeTrue();
    }
}

public class Constructor2Test
{
    [Fact]
    public void Serialize()
    {
        var primitive = new Constructor2("I'm a string.", 123.456)
        {
            IntValue = 123,
            BooleanValue = true,
        };
        using var bytes = CsTomlSerializer.Serialize(primitive);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("IntValue = 123");
        writer.AppendLine("FloatValue = 123.456");
        writer.AppendLine("BooleanValue = true");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("IntValue = 123");
        writer.AppendLine("FloatValue = 123.456");
        writer.AppendLine("BooleanValue = true");
        writer.Flush();

        var primitive = CsTomlSerializer.Deserialize<Constructor3>(buffer.WrittenSpan);
        primitive.Str.ShouldBe("I'm a string.");
        primitive.IntValue.ShouldBe(123);
        primitive.FloatValue.ShouldBe(123.456);
        primitive.BooleanValue.ShouldBeTrue();
    }
}

public class Constructor3Test
{
    [Fact]
    public void Serialize()
    {
        var primitive = new Constructor3("I'm a string.", 123, 123.456, true);
        using var bytes = CsTomlSerializer.Serialize(primitive);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("IntValue = 123");
        writer.AppendLine("FloatValue = 123.456");
        writer.AppendLine("BooleanValue = true");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("IntValue = 123");
        writer.AppendLine("FloatValue = 123.456");
        writer.AppendLine("BooleanValue = true");
        writer.Flush();

        var primitive = CsTomlSerializer.Deserialize<Constructor3>(buffer.WrittenSpan);
        primitive.Str.ShouldBe("I'm a string.");
        primitive.IntValue.ShouldBe(123);
        primitive.FloatValue.ShouldBe(123.456);
        primitive.BooleanValue.ShouldBeTrue();
    }
}

public class Constructor4Test
{
    [Fact]
    public void Serialize()
    {
        var primitive = new Constructor4(true, "I'm a string.", 123.456, 123);
        using var bytes = CsTomlSerializer.Serialize(primitive);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("IntValue = 123");
        writer.AppendLine("FloatValue = 123.456");
        writer.AppendLine("BooleanValue = true");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("IntValue = 123");
        writer.AppendLine("FloatValue = 123.456");
        writer.AppendLine("BooleanValue = true");
        writer.Flush();

        var primitive = CsTomlSerializer.Deserialize<Constructor4>(buffer.WrittenSpan);
        primitive.Str.ShouldBe("I'm a string.");
        primitive.IntValue.ShouldBe(123);
        primitive.FloatValue.ShouldBe(123.456);
        primitive.BooleanValue.ShouldBeTrue();
    }
}

public class Constructor5Test
{
    [Fact]
    public void Serialize()
    {
        var primitive = new Constructor5()
        {
            Str = @"I'm a string.",
            IntValue = 123,
            FloatValue = 123.456,
            BooleanValue = true,
        };
        using var bytes = CsTomlSerializer.Serialize(primitive);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("IntValue = 123");
        writer.AppendLine("FloatValue = 123.456");
        writer.AppendLine("BooleanValue = true");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("IntValue = 123");
        writer.AppendLine("FloatValue = 123.456");
        writer.AppendLine("BooleanValue = true");
        writer.Flush();

        var primitive = CsTomlSerializer.Deserialize<Constructor5>(buffer.WrittenSpan);
        primitive.Str.ShouldBe("I'm a string.");
        primitive.IntValue.ShouldBe(123);
        primitive.FloatValue.ShouldBe(123.456);
        primitive.BooleanValue.ShouldBeTrue();
    }
}

public class Constructor6Test
{
    [Fact]
    public void Serialize()
    {
        var primitive = new Constructor6()
        {
            Str = @"I'm a string.",
            IntValue = 123,
            FloatValue = 123.456,
            BooleanValue = true,
        };
        using var bytes = CsTomlSerializer.Serialize(primitive);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("IntValue = 123");
        writer.AppendLine("FloatValue = 123.456");
        writer.AppendLine("BooleanValue = true");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("IntValue = 123");
        writer.AppendLine("FloatValue = 123.456");
        writer.AppendLine("BooleanValue = true");
        writer.Flush();

        var primitive = CsTomlSerializer.Deserialize<Constructor6>(buffer.WrittenSpan);
        primitive.Str.ShouldBe("I'm a string.");
        primitive.IntValue.ShouldBe(123);
        primitive.FloatValue.ShouldBe(123.456);
        primitive.BooleanValue.ShouldBeTrue();
    }
}

public class Constructor7Test
{
    [Fact]
    public void Serialize()
    {
        var primitive = new Constructor7("I'm a string.", 123);
        using var bytes = CsTomlSerializer.Serialize(primitive);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("IntValue = 123");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("IntValue = 123");
        writer.Flush();

        var primitive = CsTomlSerializer.Deserialize<Constructor7>(buffer.WrittenSpan);
        primitive.Str.ShouldBe("I'm a string.");
        primitive.IntValue.ShouldBe(123);
    }
}

public class Constructor8Test
{
    [Fact]
    public void Serialize()
    {
        var primitive = new Constructor8("I'm a string.", 123)
        {
            FloatValue = 123.456,
            BooleanValue = true,
        };
        using var bytes = CsTomlSerializer.Serialize(primitive);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("IntValue = 123");
        writer.AppendLine("FloatValue = 123.456");
        writer.AppendLine("BooleanValue = true");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("IntValue = 123");
        writer.AppendLine("FloatValue = 123.456");
        writer.AppendLine("BooleanValue = true");
        writer.Flush();

        var primitive = CsTomlSerializer.Deserialize<Constructor8>(buffer.WrittenSpan);
        primitive.Str.ShouldBe("I'm a string.");
        primitive.IntValue.ShouldBe(123);
        primitive.FloatValue.ShouldBe(123.456);
        primitive.BooleanValue.ShouldBeTrue();
    }
}

