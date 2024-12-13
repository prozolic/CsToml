using CsToml.Error;
using FluentAssertions;
using System.Collections;
using System.Collections.Concurrent;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests.Seirialization;

public static class Option
{
    public static CsTomlSerializerOptions Header { get; set; } = CsTomlSerializerOptions.Default with 
    { 
        SerializeOptions = new () { TableStyle = TomlTableStyle.Header} 
    };
}

public class TypeZeroTest
{
    [Fact]
    public void Serialize()
    {
        var type = new TypeZero();
        {
            using var bytes = CsTomlSerializer.Serialize(type);

            bytes.ByteSpan.ToArray().Should().Equal(""u8.ToArray());
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            bytes.ByteSpan.ToArray().Should().Equal(""u8.ToArray());
        }
    }

    [Fact]
    public void Deserialize()
    {
        var type = CsTomlSerializer.Deserialize<TypeZero>(""u8);
        type.Should().NotBeNull();
    }
}

public class TypeOneTest
{
    [Fact]
    public void Serialize()
    {
        var type = new TypeOne
        {
            Value = 999
        };
        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        var type = CsTomlSerializer.Deserialize<TypeOne>(@"Value = 999"u8);
        type.Value.Should().Be(999);
    }
}

public class TypeTwoTest
{
    [Fact]
    public void Serialize()
    {
        var type = new TypeTwo
        {
            Value = 999,
            Value2 = 123
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.AppendLine("Value2 = 123");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.AppendLine("Value2 = 123");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Value2 = 123");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeTwo>(buffer.WrittenSpan);
        type.Value.Should().Be(999);
        type.Value2.Should().Be(123);
    }
}

public class TypeRecordTest
{
    [Fact]
    public void Serialize()
    {
        var type = new TypeRecord
        {
            Value = 999,
            Str = "Test"
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.AppendLine("Str = \"Test\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.AppendLine("Str = \"Test\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Str = \"Test\"");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeRecord>(buffer.WrittenSpan);
        type.Value.Should().Be(999);
        type.Str.Should().Be("Test");
    }
}

public class TestStructTest
{
    [Fact]
    public void Serialize()
    {
        var type = new TestStruct
        {
            Value = 999,
            Str = "Test"
        };
        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.AppendLine("Str = \"Test\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.AppendLine("Str = \"Test\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Str = \"Test\"");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TestStruct>(buffer.WrittenSpan);
        type.Value.Should().Be(999);
        type.Str.Should().Be("Test");
    }
}

public class TestRecordStructTest
{
    [Fact]
    public void Serialize()
    {
        var type = new TestRecordStruct
        {
            Value = 999,
            Str = "Test"
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.AppendLine("Str = \"Test\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.AppendLine("Str = \"Test\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Str = \"Test\"");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TestRecordStruct>(buffer.WrittenSpan);
        type.Value.Should().Be(999);
        type.Str.Should().Be("Test");
    }
}

public class TypeIgnoreTest
{
    [Fact]
    public void Serialize()
    {
        var type = new TypeIgnore
        {
            Value = 999,
            Str = "This is TypeIgnore"
        };
        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Str = \"Test\"");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeIgnore>(buffer.WrittenSpan);
        type.Value.Should().Be(999);
        type.Str.Should().BeNull();
    }
}

public class WithArrayTest
{
    [Fact]
    public void Serialize()
    {
        var type = new WithArray
        {
            Value = [1, 2, 3, 4, 5]
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [1,3,5]");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<WithArray>(buffer.WrittenSpan);
        type.Value.Should().Equal([1, 3, 5]);
    }
}

public class WithArray2Test
{
    [Fact]
    public void Serialize()
    {
        var type = new WithArray2
        {
            Value = [[1, 2, 3], [4, 5]]
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ [ 1, 2, 3 ], [ 4, 5 ] ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ [ 1, 2, 3 ], [ 4, 5 ] ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [[1,2,3],[4,5]]");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<WithArray2>(buffer.WrittenSpan);
        type.Value[0].Should().Equal([1, 2, 3]);
        type.Value[1].Should().Equal([4, 5]);
        type.Value.Length.Should().Be(2);
    }
}

public class TomlPrimitiveTest
{
    [Fact]
    public void Serialize()
    {
        var primitive = new TomlPrimitive
        {
            Str = @"I'm a string.",
            Long = 123,
            Float = 123.456,
            Boolean = true,
            OffsetDateTime = new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero),
            LocalDateTime = new DateTime(1979, 5, 27, 7, 32, 0),
            LocalDate = new DateOnly(1979, 5, 27),
            LocalTime = new TimeOnly(7, 32, 30)
        };

        {
            using var bytes = CsTomlSerializer.Serialize(primitive);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Str = \"I'm a string.\"");
            writer.AppendLine("Long = 123");
            writer.AppendLine("Float = 123.456");
            writer.AppendLine("Boolean = true");
            writer.AppendLine("LocalDateTime = 1979-05-27T07:32:00");
            writer.AppendLine("OffsetDateTime = 1979-05-27T07:32:00Z");
            writer.AppendLine("LocalDate = 1979-05-27");
            writer.AppendLine("LocalTime = 07:32:30");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(primitive, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Str = \"I'm a string.\"");
            writer.AppendLine("Long = 123");
            writer.AppendLine("Float = 123.456");
            writer.AppendLine("Boolean = true");
            writer.AppendLine("LocalDateTime = 1979-05-27T07:32:00");
            writer.AppendLine("OffsetDateTime = 1979-05-27T07:32:00Z");
            writer.AppendLine("LocalDate = 1979-05-27");
            writer.AppendLine("LocalTime = 07:32:30");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("Long = 123");
        writer.AppendLine("Float = 123.456");
        writer.AppendLine("Boolean = true");
        writer.AppendLine("LocalDateTime = 1979-05-27T07:32:00");
        writer.AppendLine("OffsetDateTime = 1979-05-27T07:32:00Z");
        writer.AppendLine("LocalDate = 1979-05-27");
        writer.AppendLine("LocalTime = 07:32:30");
        writer.Flush();

        var primitive = CsTomlSerializer.Deserialize<TomlPrimitive>(buffer.WrittenSpan);
        primitive.Str.Should().Be("I'm a string.");
        primitive.Long.Should().Be(123);
        primitive.Float.Should().Be(123.456);
        primitive.Boolean.Should().BeTrue();
        primitive.OffsetDateTime.Should().Be(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero));
        primitive.LocalDateTime.Should().Be(new DateTime(1979, 5, 27, 7, 32, 0));
        primitive.LocalDate.Should().Be(new DateOnly(1979, 5, 27));
        primitive.LocalTime.Should().Be(new TimeOnly(7, 32, 30));
    }
}

public class NullableTypeTest
{
    [Fact]
    public void Serialize()
    {
        var type = new NullableType()
        {
            Value = 999
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void SerializeNull()
    {
        var throwAction = () => { using var bytes = CsTomlSerializer.Serialize(new NullableType()); };
        throwAction.Should().Throw<CsTomlException>();
    }

    [Fact]
    public void Deserialize()
    {
        var type = CsTomlSerializer.Deserialize<NullableType>(@"Value = 999"u8);
        type.Value.Should().Be(999);
    }

    [Fact]
    public void DeserializeNull()
    {
        var type = CsTomlSerializer.Deserialize<NullableType>(@""u8);
        type.Value.Should().BeNull();
    }
}

public class WithTupleTest
{
    [Fact]
    public void Serialize()
    {
        var value = new WithTuple()
        {
            One = new Tuple<int>(1),
            Two = new Tuple<int, int>(1, 2),
            Three = new Tuple<int, int, int>(1, 2, 3),
            Four = new Tuple<int, int, int, int>(1, 2, 3, 4),
            Five = new Tuple<int, int, int, int, int>(1, 2, 3, 4, 5),
            Six = new Tuple<int, int, int, int, int, int>(1, 2, 3, 4, 5, 6),
            Seven = new Tuple<int, int, int, int, int, int, int>(1, 2, 3, 4, 5, 6, 7),
            Eight = new Tuple<int, int, int, int, int, int, int, Tuple<int>>(1, 2, 3, 4, 5, 6, 7, new Tuple<int>(8))
        };

        {
            using var bytes = CsTomlSerializer.Serialize(value);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("One = 1");
            writer.AppendLine("Two = [ 1, 2 ]");
            writer.AppendLine("Three = [ 1, 2, 3 ]");
            writer.AppendLine("Four = [ 1, 2, 3, 4 ]");
            writer.AppendLine("Five = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Six = [ 1, 2, 3, 4, 5, 6 ]");
            writer.AppendLine("Seven = [ 1, 2, 3, 4, 5, 6, 7 ]");
            writer.AppendLine("Eight = [ 1, 2, 3, 4, 5, 6, 7, 8 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(value,Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("One = 1");
            writer.AppendLine("Two = [ 1, 2 ]");
            writer.AppendLine("Three = [ 1, 2, 3 ]");
            writer.AppendLine("Four = [ 1, 2, 3, 4 ]");
            writer.AppendLine("Five = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Six = [ 1, 2, 3, 4, 5, 6 ]");
            writer.AppendLine("Seven = [ 1, 2, 3, 4, 5, 6, 7 ]");
            writer.AppendLine("Eight = [ 1, 2, 3, 4, 5, 6, 7, 8 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("One = 1");
        writer.AppendLine("Two = [ 1, 2 ]");
        writer.AppendLine("Three = [ 1, 2, 3 ]");
        writer.AppendLine("Four = [ 1, 2, 3, 4 ]");
        writer.AppendLine("Five = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Six = [ 1, 2, 3, 4, 5, 6 ]");
        writer.AppendLine("Seven = [ 1, 2, 3, 4, 5, 6, 7 ]");
        writer.AppendLine("Eight = [ 1, 2, 3, 4, 5, 6, 7, 8 ]");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<WithTuple>(buffer.WrittenSpan);
        type.One.Should().Be(new Tuple<int>(1));
        type.Two.Should().Be(new Tuple<int, int>(1, 2));
        type.Three.Should().Be(new Tuple<int, int, int>(1, 2, 3));
        type.Four.Should().Be(new Tuple<int, int, int, int>(1, 2, 3, 4));
        type.Five.Should().Be(new Tuple<int, int, int, int, int>(1, 2, 3, 4, 5));
        type.Six.Should().Be(new Tuple<int, int, int, int, int, int>(1, 2, 3, 4, 5, 6));
        type.Seven.Should().Be(new Tuple<int, int, int, int, int, int, int>(1, 2, 3, 4, 5, 6, 7));
        type.Eight.Should().Be(new Tuple<int, int, int, int, int, int, int, Tuple<int>>(1, 2, 3, 4, 5, 6, 7, new Tuple<int>(8)));
    }

}

public class WithValueTupleTest
{
    [Fact]
    public void Serialize()
    {
        var value = new WithValueTuple()
        {
            One = new ValueTuple<int>(1),
            Two = new ValueTuple<int, int>(1, 2),
            Three = new ValueTuple<int, int, int>(1, 2, 3),
            Four = new ValueTuple<int, int, int, int>(1, 2, 3, 4),
            Five = new ValueTuple<int, int, int, int, int>(1, 2, 3, 4, 5),
            Six = new ValueTuple<int, int, int, int, int, int>(1, 2, 3, 4, 5, 6),
            Seven = new ValueTuple<int, int, int, int, int, int, int>(1, 2, 3, 4, 5, 6, 7),
            Eight = new ValueTuple<int, int, int, int, int, int, int, ValueTuple<int>>(1, 2, 3, 4, 5, 6, 7, new ValueTuple<int>(8))
        };
        {
            using var bytes = CsTomlSerializer.Serialize(value);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("One = 1");
            writer.AppendLine("Two = [ 1, 2 ]");
            writer.AppendLine("Three = [ 1, 2, 3 ]");
            writer.AppendLine("Four = [ 1, 2, 3, 4 ]");
            writer.AppendLine("Five = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Six = [ 1, 2, 3, 4, 5, 6 ]");
            writer.AppendLine("Seven = [ 1, 2, 3, 4, 5, 6, 7 ]");
            writer.AppendLine("Eight = [ 1, 2, 3, 4, 5, 6, 7, 8 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(value,Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("One = 1");
            writer.AppendLine("Two = [ 1, 2 ]");
            writer.AppendLine("Three = [ 1, 2, 3 ]");
            writer.AppendLine("Four = [ 1, 2, 3, 4 ]");
            writer.AppendLine("Five = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Six = [ 1, 2, 3, 4, 5, 6 ]");
            writer.AppendLine("Seven = [ 1, 2, 3, 4, 5, 6, 7 ]");
            writer.AppendLine("Eight = [ 1, 2, 3, 4, 5, 6, 7, 8 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("One = 1");
        writer.AppendLine("Two = [ 1, 2 ]");
        writer.AppendLine("Three = [ 1, 2, 3 ]");
        writer.AppendLine("Four = [ 1, 2, 3, 4 ]");
        writer.AppendLine("Five = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Six = [ 1, 2, 3, 4, 5, 6 ]");
        writer.AppendLine("Seven = [ 1, 2, 3, 4, 5, 6, 7 ]");
        writer.AppendLine("Eight = [ 1, 2, 3, 4, 5, 6, 7, 8 ]");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<WithValueTuple>(buffer.WrittenSpan);
        type.One.Should().Be(new ValueTuple<int>(1));
        type.Two.Should().Be(new ValueTuple<int, int>(1, 2));
        type.Three.Should().Be(new ValueTuple<int, int, int>(1, 2, 3));
        type.Four.Should().Be(new ValueTuple<int, int, int, int>(1, 2, 3, 4));
        type.Five.Should().Be(new ValueTuple<int, int, int, int, int>(1, 2, 3, 4, 5));
        type.Six.Should().Be(new ValueTuple<int, int, int, int, int, int>(1, 2, 3, 4, 5, 6));
        type.Seven.Should().Be(new ValueTuple<int, int, int, int, int, int, int>(1, 2, 3, 4, 5, 6, 7));
        type.Eight.Should().Be(new ValueTuple<int, int, int, int, int, int, int, ValueTuple<int>>(1, 2, 3, 4, 5, 6, 7, new ValueTuple<int>(8)));
    }

}

public class TypeTableTest
{
    [Fact]
    public void Serialize()
    {
        var table = new TypeTable();
        table.Table2 = new TypeTable2();
        table.Table2.Table3 = new TypeTable3() { Value = "This is TypeTable3" };

        {
            using var bytes = CsTomlSerializer.Serialize(table);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Table2.Table3.Value = \"This is TypeTable3\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }

        {
            using var bytes = CsTomlSerializer.Serialize(table, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Table2]");
            writer.AppendLine("[Table2.Table3]");
            writer.AppendLine("Value = \"This is TypeTable3\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Table2.Table3.Value = \"This is TypeTable3\"");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeTable>(buffer.WrittenSpan);
            type.Table2.Table3.Value.Should().Be("This is TypeTable3");
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Table2]");
            writer.AppendLine("[Table2.Table3]");
            writer.AppendLine("Value = \"This is TypeTable3\"");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeTable>(buffer.WrittenSpan);
            type.Table2.Table3.Value.Should().Be("This is TypeTable3");
        }
    }
}

public class TypeTomlSerializedObjectListTest
{
    [Fact]
    public void Serialize()
    {
        var type = new TypeTomlSerializedObjectList() {  Table2 = [
            new TypeTable2() { Table3 = new TypeTable3() { Value = "[1] This is TypeTable3" } },
            new TypeTable2() { Table3 = new TypeTable3() { Value = "[2] This is TypeTable3" } },
            new TypeTable2() { Table3 = new TypeTable3() { Value = "[3] This is TypeTable3" } },
            new TypeTable2() { Table3 = new TypeTable3() { Value = "[4] This is TypeTable3" } },
            new TypeTable2() { Table3 = new TypeTable3() { Value = "[5] This is TypeTable3" } }]  };
        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Table2 = [ {Table3.Value = \"[1] This is TypeTable3\"}, {Table3.Value = \"[2] This is TypeTable3\"}, {Table3.Value = \"[3] This is TypeTable3\"}, {Table3.Value = \"[4] This is TypeTable3\"}, {Table3.Value = \"[5] This is TypeTable3\"} ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Table2 = [ {Table3.Value = \"[1] This is TypeTable3\"}, {Table3.Value = \"[2] This is TypeTable3\"}, {Table3.Value = \"[3] This is TypeTable3\"}, {Table3.Value = \"[4] This is TypeTable3\"}, {Table3.Value = \"[5] This is TypeTable3\"} ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Table2 = [ {Table3.Value = \"[1] This is TypeTable3\"}, {Table3.Value = \"[2] This is TypeTable3\"}, {Table3.Value = \"[3] This is TypeTable3\"}, {Table3.Value = \"[4] This is TypeTable3\"}, {Table3.Value = \"[5] This is TypeTable3\"} ]");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeTomlSerializedObjectList>(buffer.WrittenSpan);
        type.Table2.Count.Should().Be(5);
        type.Table2[0].Table3.Value.Should().Be("[1] This is TypeTable3");
        type.Table2[1].Table3.Value.Should().Be("[2] This is TypeTable3");
        type.Table2[2].Table3.Value.Should().Be("[3] This is TypeTable3");
        type.Table2[3].Table3.Value.Should().Be("[4] This is TypeTable3");
        type.Table2[4].Table3.Value.Should().Be("[5] This is TypeTable3");

    }
}

public class TypeTomlSerializedObjectListTest2
{
    [Fact]
    public void Serialize()
    {
        var type = new TypeTomlSerializedObjectList2()
        {
            Value = 999,
            Table = [
            new TypeTable(){ Table2 = new TypeTable2() { Table3 = new TypeTable3() { Value = "[1] This is TypeTable3" } } },
            new TypeTable(){ Table2 = new TypeTable2() { Table3 = new TypeTable3() { Value = "[2] This is TypeTable3" } } },
            new TypeTable(){ Table2 = new TypeTable2() { Table3 = new TypeTable3() { Value = "[3] This is TypeTable3" } } },
            new TypeTable(){ Table2 = new TypeTable2() { Table3 = new TypeTable3() { Value = "[4] This is TypeTable3" } } },
            new TypeTable(){ Table2 = new TypeTable2() { Table3 = new TypeTable3() { Value = "[5] This is TypeTable3" } } }]
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.AppendLine("Table = [ {Table2.Table3.Value = \"[1] This is TypeTable3\"}, {Table2.Table3.Value = \"[2] This is TypeTable3\"}, {Table2.Table3.Value = \"[3] This is TypeTable3\"}, {Table2.Table3.Value = \"[4] This is TypeTable3\"}, {Table2.Table3.Value = \"[5] This is TypeTable3\"} ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.AppendLine("Table = [ {Table2.Table3.Value = \"[1] This is TypeTable3\"}, {Table2.Table3.Value = \"[2] This is TypeTable3\"}, {Table2.Table3.Value = \"[3] This is TypeTable3\"}, {Table2.Table3.Value = \"[4] This is TypeTable3\"}, {Table2.Table3.Value = \"[5] This is TypeTable3\"} ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Table = [ {Table2.Table3.Value = \"[1] This is TypeTable3\"}, {Table2.Table3.Value = \"[2] This is TypeTable3\"}, {Table2.Table3.Value = \"[3] This is TypeTable3\"}, {Table2.Table3.Value = \"[4] This is TypeTable3\"}, {Table2.Table3.Value = \"[5] This is TypeTable3\"} ]");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeTomlSerializedObjectList2>(buffer.WrittenSpan);
        type.Table.Count.Should().Be(5);
        type.Table[0].Table2.Table3.Value.Should().Be("[1] This is TypeTable3");
        type.Table[1].Table2.Table3.Value.Should().Be("[2] This is TypeTable3");
        type.Table[2].Table2.Table3.Value.Should().Be("[3] This is TypeTable3");
        type.Table[3].Table2.Table3.Value.Should().Be("[4] This is TypeTable3");
        type.Table[4].Table2.Table3.Value.Should().Be("[5] This is TypeTable3");

    }
}

public class TypeCollectionTest
{
    [Fact]
    public void Serialize()
    {
        var type = new TypeCollection()
        {
            Value = [1, 2, 3, 4, 5],
            Value2 = new Stack<int>([5, 4, 3, 2, 1]),
            Value3 = new HashSet<int>([1, 2, 3, 4, 5]),
            Value4 = new SortedSet<int>([1, 2, 3, 4, 5]),
            Value5 = new Queue<int>([1, 2, 3, 4, 5]),
            Value6 = new LinkedList<int>([1, 2, 3, 4, 5]),
            Value7 = new System.Collections.Concurrent.ConcurrentQueue<int>([1, 2, 3, 4, 5]),
            Value8 = new System.Collections.Concurrent.ConcurrentStack<int>([5, 4, 3, 2, 1]),
            Value9 = new System.Collections.Concurrent.ConcurrentBag<int>([5, 4, 3, 2, 1]),
            Value10 = new System.Collections.ObjectModel.ReadOnlyCollection<int>([1, 2, 3, 4, 5])
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value2 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value3 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value4 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value5 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value6 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value7 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value8 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value9 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value10 = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value2 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value3 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value4 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value5 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value6 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value7 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value8 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value9 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value10 = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value2 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value3 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value4 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value5 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value6 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value7 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value8 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value9 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value10 = [ 1, 2, 3, 4, 5 ]");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeCollection>(buffer.WrittenSpan);
        type.Value.Should().Equal([1, 2, 3, 4, 5]);
        type.Value2.Should().Equal([1, 2, 3, 4, 5]);
        type.Value3.Should().Equal([1, 2, 3, 4, 5]);
        type.Value4.Should().Equal([1, 2, 3, 4, 5]);
        type.Value5.Should().Equal([1, 2, 3, 4, 5]);
        type.Value6.Should().Equal([1, 2, 3, 4, 5]);
        type.Value7.Should().Equal([1, 2, 3, 4, 5]);
        type.Value8.Should().Equal([1, 2, 3, 4, 5]);
        type.Value9.Should().Equal([1, 2, 3, 4, 5]);
        type.Value10.Should().Equal([1, 2, 3, 4, 5]);
    }
}

public class TypeArrayOfTablesTest
{
    [Fact]
    public void Serialize()
    {
        var type = new TypeArrayOfTable()
        {
            Header = new TypeTomlSerializedObjectList()
            {
                Table2 = [
                    new TypeTable2(){ Table3 = new TypeTable3() { Value = "[1] This is TypeTable3" } },
                    new TypeTable2(){ Table3 = new TypeTable3() { Value = "[2] This is TypeTable3" } },
                    new TypeTable2(){ Table3 = new TypeTable3() { Value = "[3] This is TypeTable3" } }
                ],
            }
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Header.Table2 = [ {Table3.Value = \"[1] This is TypeTable3\"}, {Table3.Value = \"[2] This is TypeTable3\"}, {Table3.Value = \"[3] This is TypeTable3\"} ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Header]");
            writer.AppendLine("Table2 = [ {Table3.Value = \"[1] This is TypeTable3\"}, {Table3.Value = \"[2] This is TypeTable3\"}, {Table3.Value = \"[3] This is TypeTable3\"} ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[[Header.Table2]]");
            writer.AppendLine("Table3.Value = \"[1] This is TypeTable3\"");
            writer.AppendLine();
            writer.AppendLine("[[Header.Table2]]");
            writer.AppendLine("Table3.Value = \"[2] This is TypeTable3\"");
            writer.AppendLine();
            writer.AppendLine("[[Header.Table2]]");
            writer.AppendLine("Table3.Value = \"[3] This is TypeTable3\"");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeArrayOfTable>(buffer.WrittenSpan);
            type.Header.Table2[0].Table3.Value.Should().Be("[1] This is TypeTable3");
            type.Header.Table2[1].Table3.Value.Should().Be("[2] This is TypeTable3");
            type.Header.Table2[2].Table3.Value.Should().Be("[3] This is TypeTable3");
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Header.Table2 = [ {Table3.Value = \"[1] This is TypeTable3\"}, {Table3.Value = \"[2] This is TypeTable3\"}, {Table3.Value = \"[3] This is TypeTable3\"} ]");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeArrayOfTable>(buffer.WrittenSpan);
            type.Header.Table2[0].Table3.Value.Should().Be("[1] This is TypeTable3");
            type.Header.Table2[1].Table3.Value.Should().Be("[2] This is TypeTable3");
            type.Header.Table2[2].Table3.Value.Should().Be("[3] This is TypeTable3");
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Header]");
            writer.AppendLine("Table2 = [ {Table3.Value = \"[1] This is TypeTable3\"}, {Table3.Value = \"[2] This is TypeTable3\"}, {Table3.Value = \"[3] This is TypeTable3\"} ]");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeArrayOfTable>(buffer.WrittenSpan);
            type.Header.Table2[0].Table3.Value.Should().Be("[1] This is TypeTable3");
            type.Header.Table2[1].Table3.Value.Should().Be("[2] This is TypeTable3");
            type.Header.Table2[2].Table3.Value.Should().Be("[3] This is TypeTable3");
        }
    }
}

public class TypeDictionaryTest
{
    [Fact]
    public void Serialize()
    {
        var dict = new Dictionary<string, object?>()
        {
            ["key"] = new object[]
            {
                999,
                "Value",
                new Dictionary<string, object?>()
                {
                    ["key"] = new object[]
                    {
                        new long[] {1, 2, 3},
                        new Dictionary<string, object?>()
                        {
                            ["key"] = "value"
                        }
                    }
                }
            }
        };

        var type = new TypeDictionary() { Value = dict };
        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]}");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeDictionary>(buffer.WrittenSpan);
        dynamic dynamicDict = type.Value;

        long value = dynamicDict["key"][0];
        value.Should().Be(999);
        string value2 = dynamicDict["key"][1];
        value2.Should().Be("Value");
        object[] value3 = dynamicDict["key"][2]["key"][0];
        value3.Should().Equal(new object[] { 1, 2, 3 });
        string value4 = dynamicDict["key"][2]["key"][1]["key"];
        value4.Should().Be("value");
    }
}

public class TypeDictionaryTest2
{
    [Fact]
    public void Serialize()
    {
        var dict = new Dictionary<object, object>()
        {
            ["key"] = new object[]
            {
                999,
                "Value",
                new Dictionary<object, object?>()
                {
                    ["key"] = new object[]
                    {
                        new long[] {1, 2, 3},
                        new Dictionary<object, object?>()
                        {
                            ["key"] = "value"
                        }
                    }
                }
            }
        };

        var type = new TypeDictionary2() { 
            Value = dict,
            Value2 = dict.AsReadOnly(),
            Value3 = new SortedDictionary<object, object>(dict),
            Value4 = new System.Collections.Concurrent.ConcurrentDictionary<object, object>(dict),
            Value5 = dict,
            Value6 = dict,
        };
        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
            writer.AppendLine("Value2 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
            writer.AppendLine("Value3 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
            writer.AppendLine("Value4 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
            writer.AppendLine("Value5 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
            writer.AppendLine("Value6 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
            writer.AppendLine("Value2 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
            writer.AppendLine("Value3 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
            writer.AppendLine("Value4 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
            writer.AppendLine("Value5 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
            writer.AppendLine("Value6 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]}");
        writer.AppendLine("Value2 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]}");
        writer.AppendLine("Value3 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]}");
        writer.AppendLine("Value4 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]}");
        writer.AppendLine("Value5 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]}");
        writer.AppendLine("Value6 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]}");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeDictionary2>(buffer.WrittenSpan);
        {
            dynamic dynamicDict = type.Value;

            long value = dynamicDict["key"][0];
            value.Should().Be(999);
            string value2 = dynamicDict["key"][1];
            value2.Should().Be("Value");
            object[] value3 = dynamicDict["key"][2]["key"][0];
            value3.Should().Equal(new object[] { 1, 2, 3 });
            string value4 = dynamicDict["key"][2]["key"][1]["key"];
            value4.Should().Be("value");
        }
        {
            dynamic dynamicDict = type.Value2;

            long value = dynamicDict["key"][0];
            value.Should().Be(999);
            string value2 = dynamicDict["key"][1];
            value2.Should().Be("Value");
            object[] value3 = dynamicDict["key"][2]["key"][0];
            value3.Should().Equal(new object[] { 1, 2, 3 });
            string value4 = dynamicDict["key"][2]["key"][1]["key"];
            value4.Should().Be("value");
        }
        {
            dynamic dynamicDict = type.Value3;

            long value = dynamicDict["key"][0];
            value.Should().Be(999);
            string value2 = dynamicDict["key"][1];
            value2.Should().Be("Value");
            object[] value3 = dynamicDict["key"][2]["key"][0];
            value3.Should().Equal(new object[] { 1, 2, 3 });
            string value4 = dynamicDict["key"][2]["key"][1]["key"];
            value4.Should().Be("value");
        }
        {
            dynamic dynamicDict = type.Value4;

            long value = dynamicDict["key"][0];
            value.Should().Be(999);
            string value2 = dynamicDict["key"][1];
            value2.Should().Be("Value");
            object[] value3 = dynamicDict["key"][2]["key"][0];
            value3.Should().Equal(new object[] { 1, 2, 3 });
            string value4 = dynamicDict["key"][2]["key"][1]["key"];
            value4.Should().Be("value");
        }
        {
            dynamic dynamicDict = type.Value5;

            long value = dynamicDict["key"][0];
            value.Should().Be(999);
            string value2 = dynamicDict["key"][1];
            value2.Should().Be("Value");
            object[] value3 = dynamicDict["key"][2]["key"][0];
            value3.Should().Equal(new object[] { 1, 2, 3 });
            string value4 = dynamicDict["key"][2]["key"][1]["key"];
            value4.Should().Be("value");
        }
        {
            dynamic dynamicDict = type.Value6;

            long value = dynamicDict["key"][0];
            value.Should().Be(999);
            string value2 = dynamicDict["key"][1];
            value2.Should().Be("Value");
            object[] value3 = dynamicDict["key"][2]["key"][0];
            value3.Should().Equal(new object[] { 1, 2, 3 });
            string value4 = dynamicDict["key"][2]["key"][1]["key"];
            value4.Should().Be("value");
        }

    }
}

public class TypeDictionaryTest3
{
    [Fact]
    public void Serialize()
    {
        var type = new TypeDictionary3()
        {
            Value = new Dictionary<long, string>()
            {
                [123] = "Value",
                [-1] = "Value",
                [123456789] = "Value",
            }
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = {123 = \"Value\", -1 = \"Value\", 123456789 = \"Value\"}");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = {123 = \"Value\", -1 = \"Value\", 123456789 = \"Value\"}");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = {123 = \"Value\", -1 = \"Value2\", 123456789 = \"Value3\" }");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeDictionary3>(buffer.WrittenSpan);
        var dynamicDict = type.Value;

        string value = dynamicDict[123];
        value.Should().Be("Value");
        string value2 = dynamicDict[-1];
        value2.Should().Be("Value2");
        string value3 = dynamicDict[123456789];
        value3.Should().Be("Value3");
    }
}

public class TypeHashtableTest
{
    [Fact]
    public void Serialize()
    {
        var type = new TypeHashtable()
        {
            Value = new Hashtable()
            {
                [123] = "Value",
                [-1] = "Value",
                [123456789] = "Value",
            }
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = {123 = \"Value\", 123456789 = \"Value\", -1 = \"Value\"}");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = {123 = \"Value\", 123456789 = \"Value\", -1 = \"Value\"}");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = {123 = \"Value\", -1 = \"Value2\", 123456789 = \"Value3\" }");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeHashtable>(buffer.WrittenSpan);
        string value = (string)type.Value["123"]!;
        value.Should().Be("Value");
        string value2 = (string)type.Value["-1"]!;
        value2.Should().Be("Value2");
        string value3 = (string)type.Value["123456789"]!;
        value3.Should().Be("Value3");
    }
}


public class TypeAliasTest
{
    [Fact]
    public void Serialize()
    {
        var type = new TypeAlias() { Value = "This is TypeAlias" };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("alias = \"This is TypeAlias\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("alias = \"This is TypeAlias\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        var type = CsTomlSerializer.Deserialize<TypeAlias>("alias = \"This is TypeAlias\""u8);
        type.Value.Should().Be("This is TypeAlias");
    }
}


public class TypeTableATest
{
    [Fact]
    public void Serialize()
    {
        var tableA = new TypeTableA
        {
            Dict = new ConcurrentDictionary<int, string>()
            {
                [1] = "2",
                [3] = "4",
            },
            TableB = new TypeTableB()
            {
                Value = "This is TypeTableB",
                TableC = new TypeTableC
                {
                    Value = "This is TypeTableC",
                    TableD = new TypeTableD() { Value = "This is TypeTableD" }
                },
                TableECollection = [
                    new TypeTableE(){ TableF = new TypeTableF() { Value = "[1] This is TypeTableF" } },
                    new TypeTableE(){ TableF = new TypeTableF() { Value = "[2] This is TypeTableF" } },
                    new TypeTableE(){ TableF = new TypeTableF() { Value = "[3] This is TypeTableF" } }
                ],
            }
        };

        {
            using var bytes = CsTomlSerializer.Serialize(tableA);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Dict = {1 = \"2\", 3 = \"4\"}");
            writer.AppendLine("TableB.Value = \"This is TypeTableB\"");
            writer.AppendLine("TableB.TableECollection = [ {TableF.Value = \"[1] This is TypeTableF\"}, {TableF.Value = \"[2] This is TypeTableF\"}, {TableF.Value = \"[3] This is TypeTableF\"} ]");
            writer.AppendLine("TableB.TableC.Value = \"This is TypeTableC\"");
            writer.AppendLine("TableB.TableC.TableD.Value = \"This is TypeTableD\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(tableA, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Dict = {1 = \"2\", 3 = \"4\"}");
            writer.AppendLine("[TableB]");
            writer.AppendLine("Value = \"This is TypeTableB\"");
            writer.AppendLine("TableECollection = [ {TableF.Value = \"[1] This is TypeTableF\"}, {TableF.Value = \"[2] This is TypeTableF\"}, {TableF.Value = \"[3] This is TypeTableF\"} ]");
            writer.AppendLine("[TableB.TableC]");
            writer.AppendLine("Value = \"This is TypeTableC\"");
            writer.AppendLine("[TableB.TableC.TableD]");
            writer.AppendLine("Value = \"This is TypeTableD\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Dict = {1 = \"2\", 3 = \"4\"}");
            writer.AppendLine("TableB.Value = \"This is TypeTableB\"");
            writer.AppendLine("TableB.TableECollection = [ {TableF.Value = \"[1] This is TypeTableF\"}, {TableF.Value = \"[2] This is TypeTableF\"}, {TableF.Value = \"[3] This is TypeTableF\"} ]");
            writer.AppendLine("TableB.TableC.Value = \"This is TypeTableC\"");
            writer.AppendLine("TableB.TableC.TableD.Value = \"This is TypeTableD\"");
            writer.Flush();

            var tableA = CsTomlSerializer.Deserialize<TypeTableA>(buffer.WrittenSpan);

            tableA.Dict.Count.Should().Be(2);
            tableA.Dict[1].Should().Be("2");
            tableA.Dict[3].Should().Be("4");
            tableA.TableB.Value.Should().Be("This is TypeTableB");
            tableA.TableB.TableC.Value.Should().Be("This is TypeTableC");
            tableA.TableB.TableC.TableD.Value.Should().Be("This is TypeTableD");
            tableA.TableB.TableECollection.Count.Should().Be(3);
            tableA.TableB.TableECollection[0].TableF.Value.Should().Be("[1] This is TypeTableF");
            tableA.TableB.TableECollection[1].TableF.Value.Should().Be("[2] This is TypeTableF");
            tableA.TableB.TableECollection[2].TableF.Value.Should().Be("[3] This is TypeTableF");
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Dict = {1 = \"2\", 3 = \"4\"}");
            writer.AppendLine("TableB.Value = \"This is TypeTableB\"");
            writer.AppendLine("TableB.TableECollection = [ {TableF.Value = \"[1] This is TypeTableF\"}, {TableF.Value = \"[2] This is TypeTableF\"}, {TableF.Value = \"[3] This is TypeTableF\"} ]");
            writer.AppendLine("TableB.TableC.Value = \"This is TypeTableC\"");
            writer.AppendLine("TableB.TableC.TableD.Value = \"This is TypeTableD\"");
            writer.Flush();

            var tableA = CsTomlSerializer.Deserialize<TypeTableA>(buffer.WrittenSpan);

            tableA.Dict.Count.Should().Be(2);
            tableA.Dict[1].Should().Be("2");
            tableA.Dict[3].Should().Be("4");
            tableA.TableB.Value.Should().Be("This is TypeTableB");
            tableA.TableB.TableC.Value.Should().Be("This is TypeTableC");
            tableA.TableB.TableC.TableD.Value.Should().Be("This is TypeTableD");
            tableA.TableB.TableECollection.Count.Should().Be(3);
            tableA.TableB.TableECollection[0].TableF.Value.Should().Be("[1] This is TypeTableF");
            tableA.TableB.TableECollection[1].TableF.Value.Should().Be("[2] This is TypeTableF");
            tableA.TableB.TableECollection[2].TableF.Value.Should().Be("[3] This is TypeTableF");
        }
    }

}


public class TypeSortedListTest
{
    [Fact]
    public void Serialize()
    {
        var sortedList = new SortedList<string, string>()
        {
            ["key"] = "value",
            ["key2"] = "value2",
            ["key3"] = "value3",
        };

        var type = new TypeSortedList() { Value = sortedList };
        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ [ \"key\", \"value\" ], [ \"key2\", \"value2\" ], [ \"key3\", \"value3\" ] ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ [ \"key\", \"value\" ], [ \"key2\", \"value2\" ], [ \"key3\", \"value3\" ] ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().Should().Equal(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [ [ \"key\", \"value\" ], [ \"key2\", \"value2\" ], [ \"key3\", \"value3\" ] ]");
        writer.Flush();

        var expected = new SortedList<string, string>()
        {
            ["key"] = "value",
            ["key2"] = "value2",
            ["key3"] = "value3",
        };

        var type = CsTomlSerializer.Deserialize<TypeSortedList>(buffer.WrittenSpan);
        type.Value.SequenceEqual(expected).Should().BeTrue();
    }
}