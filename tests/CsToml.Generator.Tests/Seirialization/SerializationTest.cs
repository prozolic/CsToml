using CsToml.Error;
using Shouldly;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
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


            bytes.ByteSpan.ToArray().ShouldBe(""u8.ToArray());
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            bytes.ByteSpan.ToArray().ShouldBe(""u8.ToArray());
        }
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.AppendLine("Value2 = 123");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
        type.Value.ShouldBe(999);
        type.Value2.ShouldBe(123);
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.AppendLine("Str = \"Test\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
        type.Value.ShouldBe(999);
        type.Str.ShouldBe("Test");
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.AppendLine("Str = \"Test\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
        type.Value.ShouldBe(999);
        type.Str.ShouldBe("Test");
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.AppendLine("Str = \"Test\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
        type.Value.ShouldBe(999);
        type.Str.ShouldBe("Test");
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
        type.Value.ShouldBe(999);
        type.Str.ShouldBeNull();
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [1,3,5]");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<WithArray>(buffer.WrittenSpan);
        type.Value.ShouldBe([1, 3, 5]);
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ [ 1, 2, 3 ], [ 4, 5 ] ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [[1,2,3],[4,5]]");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<WithArray2>(buffer.WrittenSpan);
        type.Value[0].ShouldBe([1, 2, 3]);
        type.Value[1].ShouldBe([4, 5]);
        type.Value.Length.ShouldBe(2);
    }
}

public class WithNullableArrayTest
{
    [Fact]
    public void Serialize()
    {
        var type = new WithNullableArray
        {
            Value = [1, 2, 3, 4, 5]
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        var nullableValue = CsTomlSerializer.Deserialize<WithNullableArray>(""u8);
        nullableValue.Value.ShouldBeNull();

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [1,3,5]");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<WithNullableArray>(buffer.WrittenSpan);
        type.Value.ShouldBe([1, 3, 5]);
    }
}

public class WithNullableArray2Test
{
    [Fact]
    public void Serialize()
    {
        var type = new WithNullableArray2
        {
            Value = [1, 2, 3, 4, 5]
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        var nullableValue = CsTomlSerializer.Deserialize<WithNullableArray2>(""u8);
        nullableValue.Value.ShouldBeNull();

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [1,3,5]");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<WithNullableArray>(buffer.WrittenSpan);
        type.Value.ShouldBe([1, 3, 5]);
    }
}

public class WithCollectionTest
{
    [Fact]
    public void Serialize()
    {
        var type = new WithCollection
        {
            Value = [1, 2, 3, 4, 5]
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [1,3,5]");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<WithCollection>(buffer.WrittenSpan);
        type.Value.ShouldBe([1, 3, 5]);
    }
}

public class WithNullableCollectionTest
{
    [Fact]
    public void Serialize()
    {
        var type = new WithNullableCollection
        {
            Value = [1, 2, 3, 4, 5]
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        var nullableValue = CsTomlSerializer.Deserialize<WithNullableCollection>(""u8);
        nullableValue.Value.ShouldBeNull();

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [1,3,5]");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<WithNullableCollection>(buffer.WrittenSpan);
        type.Value.ShouldBe([1, 3, 5]);
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
        primitive.Str.ShouldBe("I'm a string.");
        primitive.Long.ShouldBe(123);
        primitive.Float.ShouldBe(123.456);
        primitive.Boolean.ShouldBeTrue();
        primitive.OffsetDateTime.ShouldBe(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero));
        primitive.LocalDateTime.ShouldBe(new DateTime(1979, 5, 27, 7, 32, 0));
        primitive.LocalDate.ShouldBe(new DateOnly(1979, 5, 27));
        primitive.LocalTime.ShouldBe(new TimeOnly(7, 32, 30));
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void SerializeNull()
    {
        var throwAction = () => { using var bytes = CsTomlSerializer.Serialize(new NullableType()); };
        throwAction.ShouldThrow<CsTomlException>();
    }

    [Fact]
    public void Deserialize()
    {
        var type = CsTomlSerializer.Deserialize<NullableType>(@"Value = 999"u8);
        type.Value.ShouldBe(999);
    }

    [Fact]
    public void DeserializeNull()
    {
        var type = CsTomlSerializer.Deserialize<NullableType>(@""u8);
        type.Value.ShouldBeNull();
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
        type.One.ShouldBe(new Tuple<int>(1));
        type.Two.ShouldBe(new Tuple<int, int>(1, 2));
        type.Three.ShouldBe(new Tuple<int, int, int>(1, 2, 3));
        type.Four.ShouldBe(new Tuple<int, int, int, int>(1, 2, 3, 4));
        type.Five.ShouldBe(new Tuple<int, int, int, int, int>(1, 2, 3, 4, 5));
        type.Six.ShouldBe(new Tuple<int, int, int, int, int, int>(1, 2, 3, 4, 5, 6));
        type.Seven.ShouldBe(new Tuple<int, int, int, int, int, int, int>(1, 2, 3, 4, 5, 6, 7));
        type.Eight.ShouldBe(new Tuple<int, int, int, int, int, int, int, Tuple<int>>(1, 2, 3, 4, 5, 6, 7, new Tuple<int>(8)));
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
        type.One.ShouldBe(new ValueTuple<int>(1));
        type.Two.ShouldBe(new ValueTuple<int, int>(1, 2));
        type.Three.ShouldBe(new ValueTuple<int, int, int>(1, 2, 3));
        type.Four.ShouldBe(new ValueTuple<int, int, int, int>(1, 2, 3, 4));
        type.Five.ShouldBe(new ValueTuple<int, int, int, int, int>(1, 2, 3, 4, 5));
        type.Six.ShouldBe(new ValueTuple<int, int, int, int, int, int>(1, 2, 3, 4, 5, 6));
        type.Seven.ShouldBe(new ValueTuple<int, int, int, int, int, int, int>(1, 2, 3, 4, 5, 6, 7));
        type.Eight.ShouldBe(new ValueTuple<int, int, int, int, int, int, int, ValueTuple<int>>(1, 2, 3, 4, 5, 6, 7, new ValueTuple<int>(8)));
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }

        {
            using var bytes = CsTomlSerializer.Serialize(table, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Table2]");
            writer.AppendLine("[Table2.Table3]");
            writer.AppendLine("Value = \"This is TypeTable3\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
            type.Table2.Table3.Value.ShouldBe("This is TypeTable3");
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Table2]");
            writer.AppendLine("[Table2.Table3]");
            writer.AppendLine("Value = \"This is TypeTable3\"");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeTable>(buffer.WrittenSpan);
            type.Table2.Table3.Value.ShouldBe("This is TypeTable3");
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Table2 = [ {Table3.Value = \"[1] This is TypeTable3\"}, {Table3.Value = \"[2] This is TypeTable3\"}, {Table3.Value = \"[3] This is TypeTable3\"}, {Table3.Value = \"[4] This is TypeTable3\"}, {Table3.Value = \"[5] This is TypeTable3\"} ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Table2 = [ {Table3.Value = \"[1] This is TypeTable3\"}, {Table3.Value = \"[2] This is TypeTable3\"}, {Table3.Value = \"[3] This is TypeTable3\"}, {Table3.Value = \"[4] This is TypeTable3\"}, {Table3.Value = \"[5] This is TypeTable3\"} ]");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeTomlSerializedObjectList>(buffer.WrittenSpan);
        type.Table2.Count.ShouldBe(5);
        type.Table2[0].Table3.Value.ShouldBe("[1] This is TypeTable3");
        type.Table2[1].Table3.Value.ShouldBe("[2] This is TypeTable3");
        type.Table2[2].Table3.Value.ShouldBe("[3] This is TypeTable3");
        type.Table2[3].Table3.Value.ShouldBe("[4] This is TypeTable3");
        type.Table2[4].Table3.Value.ShouldBe("[5] This is TypeTable3");

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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.AppendLine("Table = [ {Table2.Table3.Value = \"[1] This is TypeTable3\"}, {Table2.Table3.Value = \"[2] This is TypeTable3\"}, {Table2.Table3.Value = \"[3] This is TypeTable3\"}, {Table2.Table3.Value = \"[4] This is TypeTable3\"}, {Table2.Table3.Value = \"[5] This is TypeTable3\"} ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
        type.Table.Count.ShouldBe(5);
        type.Table[0].Table2.Table3.Value.ShouldBe("[1] This is TypeTable3");
        type.Table[1].Table2.Table3.Value.ShouldBe("[2] This is TypeTable3");
        type.Table[2].Table2.Table3.Value.ShouldBe("[3] This is TypeTable3");
        type.Table[3].Table2.Table3.Value.ShouldBe("[4] This is TypeTable3");
        type.Table[4].Table2.Table3.Value.ShouldBe("[5] This is TypeTable3");

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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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

        int[] expected = [1, 2, 3, 4, 5];
        type.Value.ShouldBe(expected);
        type.Value2.ShouldBe(expected);
        type.Value3.ShouldBe(expected);
        type.Value4.ShouldBe(expected);
        type.Value5.ShouldBe(expected);
        type.Value6.ShouldBe(expected);
        type.Value7.ShouldBe(expected);
        type.Value8.ShouldBe(expected);
        type.Value9.ShouldBe(expected);
        type.Value10.ShouldBe(expected);
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Header]");
            writer.AppendLine("Table2 = [ {Table3.Value = \"[1] This is TypeTable3\"}, {Table3.Value = \"[2] This is TypeTable3\"}, {Table3.Value = \"[3] This is TypeTable3\"} ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
            type.Header.Table2[0].Table3.Value.ShouldBe("[1] This is TypeTable3");
            type.Header.Table2[1].Table3.Value.ShouldBe("[2] This is TypeTable3");
            type.Header.Table2[2].Table3.Value.ShouldBe("[3] This is TypeTable3");
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Header.Table2 = [ {Table3.Value = \"[1] This is TypeTable3\"}, {Table3.Value = \"[2] This is TypeTable3\"}, {Table3.Value = \"[3] This is TypeTable3\"} ]");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeArrayOfTable>(buffer.WrittenSpan);
            type.Header.Table2[0].Table3.Value.ShouldBe("[1] This is TypeTable3");
            type.Header.Table2[1].Table3.Value.ShouldBe("[2] This is TypeTable3");
            type.Header.Table2[2].Table3.Value.ShouldBe("[3] This is TypeTable3");
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Header]");
            writer.AppendLine("Table2 = [ {Table3.Value = \"[1] This is TypeTable3\"}, {Table3.Value = \"[2] This is TypeTable3\"}, {Table3.Value = \"[3] This is TypeTable3\"} ]");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeArrayOfTable>(buffer.WrittenSpan);
            type.Header.Table2[0].Table3.Value.ShouldBe("[1] This is TypeTable3");
            type.Header.Table2[1].Table3.Value.ShouldBe("[2] This is TypeTable3");
            type.Header.Table2[2].Table3.Value.ShouldBe("[3] This is TypeTable3");
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
        value.ShouldBe(999);
        string value2 = dynamicDict["key"][1];
        value2.ShouldBe("Value");
        object[] value3 = dynamicDict["key"][2]["key"][0];
        value3.ShouldBe(new object[] { 1, 2, 3 });
        string value4 = dynamicDict["key"][2]["key"][1]["key"];
        value4.ShouldBe("value");
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
            value.ShouldBe(999);
            string value2 = dynamicDict["key"][1];
            value2.ShouldBe("Value");
            object[] value3 = dynamicDict["key"][2]["key"][0];
            value3.ShouldBe(new object[] { 1, 2, 3 });
            string value4 = dynamicDict["key"][2]["key"][1]["key"];
            value4.ShouldBe("value");
        }
        {
            dynamic dynamicDict = type.Value2;

            long value = dynamicDict["key"][0];
            value.ShouldBe(999);
            string value2 = dynamicDict["key"][1];
            value2.ShouldBe("Value");
            object[] value3 = dynamicDict["key"][2]["key"][0];
            value3.ShouldBe(new object[] { 1, 2, 3 });
            string value4 = dynamicDict["key"][2]["key"][1]["key"];
            value4.ShouldBe("value");
        }
        {
            dynamic dynamicDict = type.Value3;

            long value = dynamicDict["key"][0];
            value.ShouldBe(999);
            string value2 = dynamicDict["key"][1];
            value2.ShouldBe("Value");
            object[] value3 = dynamicDict["key"][2]["key"][0];
            value3.ShouldBe(new object[] { 1, 2, 3 });
            string value4 = dynamicDict["key"][2]["key"][1]["key"];
            value4.ShouldBe("value");
        }
        {
            dynamic dynamicDict = type.Value4;

            long value = dynamicDict["key"][0];
            value.ShouldBe(999);
            string value2 = dynamicDict["key"][1];
            value2.ShouldBe("Value");
            object[] value3 = dynamicDict["key"][2]["key"][0];
            value3.ShouldBe(new object[] { 1, 2, 3 });
            string value4 = dynamicDict["key"][2]["key"][1]["key"];
            value4.ShouldBe("value");
        }
        {
            dynamic dynamicDict = type.Value5;

            long value = dynamicDict["key"][0];
            value.ShouldBe(999);
            string value2 = dynamicDict["key"][1];
            value2.ShouldBe("Value");
            object[] value3 = dynamicDict["key"][2]["key"][0];
            value3.ShouldBe(new object[] { 1, 2, 3 });
            string value4 = dynamicDict["key"][2]["key"][1]["key"];
            value4.ShouldBe("value");
        }
        {
            dynamic dynamicDict = type.Value6;

            long value = dynamicDict["key"][0];
            value.ShouldBe(999);
            string value2 = dynamicDict["key"][1];
            value2.ShouldBe("Value");
            object[] value3 = dynamicDict["key"][2]["key"][0];
            value3.ShouldBe(new object[] { 1, 2, 3 });
            string value4 = dynamicDict["key"][2]["key"][1]["key"];
            value4.ShouldBe("value");
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = {123 = \"Value\", -1 = \"Value\", 123456789 = \"Value\"}");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
        value.ShouldBe("Value");
        string value2 = dynamicDict[-1];
        value2.ShouldBe("Value2");
        string value3 = dynamicDict[123456789];
        value3.ShouldBe("Value3");
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = {123 = \"Value\", 123456789 = \"Value\", -1 = \"Value\"}");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
        value.ShouldBe("Value");
        string value2 = (string)type.Value["-1"]!;
        value2.ShouldBe("Value2");
        string value3 = (string)type.Value["123456789"]!;
        value3.ShouldBe("Value3");
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("alias = \"This is TypeAlias\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        var type = CsTomlSerializer.Deserialize<TypeAlias>("alias = \"This is TypeAlias\""u8);
        type.Value.ShouldBe("This is TypeAlias");
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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

            tableA.Dict.Count.ShouldBe(2);
            tableA.Dict[1].ShouldBe("2");
            tableA.Dict[3].ShouldBe("4");
            tableA.TableB.Value.ShouldBe("This is TypeTableB");
            tableA.TableB.TableC.Value.ShouldBe("This is TypeTableC");
            tableA.TableB.TableC.TableD.Value.ShouldBe("This is TypeTableD");
            tableA.TableB.TableECollection.Count.ShouldBe(3);
            tableA.TableB.TableECollection[0].TableF.Value.ShouldBe("[1] This is TypeTableF");
            tableA.TableB.TableECollection[1].TableF.Value.ShouldBe("[2] This is TypeTableF");
            tableA.TableB.TableECollection[2].TableF.Value.ShouldBe("[3] This is TypeTableF");
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

            tableA.Dict.Count.ShouldBe(2);
            tableA.Dict[1].ShouldBe("2");
            tableA.Dict[3].ShouldBe("4");
            tableA.TableB.Value.ShouldBe("This is TypeTableB");
            tableA.TableB.TableC.Value.ShouldBe("This is TypeTableC");
            tableA.TableB.TableC.TableD.Value.ShouldBe("This is TypeTableD");
            tableA.TableB.TableECollection.Count.ShouldBe(3);
            tableA.TableB.TableECollection[0].TableF.Value.ShouldBe("[1] This is TypeTableF");
            tableA.TableB.TableECollection[1].TableF.Value.ShouldBe("[2] This is TypeTableF");
            tableA.TableB.TableECollection[2].TableF.Value.ShouldBe("[3] This is TypeTableF");
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ [ \"key\", \"value\" ], [ \"key2\", \"value2\" ], [ \"key3\", \"value3\" ] ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
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
        type.Value.SequenceEqual(expected).ShouldBeTrue();
    }
}

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


public class InitTest
{
    [Fact]
    public void Serialize()
    {
        var primitive = new Init()
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

        var primitive = CsTomlSerializer.Deserialize<Init>(buffer.WrittenSpan);
        primitive.Str.ShouldBe("I'm a string.");
        primitive.IntValue.ShouldBe(123);
        primitive.FloatValue.ShouldBe(123.456);
        primitive.BooleanValue.ShouldBeTrue();
    }
}

public class Init2Test
{
    [Fact]
    public void Serialize()
    {
        var primitive = new Init2()
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

        var primitive = CsTomlSerializer.Deserialize<Init2>(buffer.WrittenSpan);
        primitive.Str.ShouldBe("I'm a string.");
        primitive.IntValue.ShouldBe(123);
        primitive.FloatValue.ShouldBe(123.456);
        primitive.BooleanValue.ShouldBeTrue();
    }
}

public class ConstructorAndInitTest
{
    [Fact]
    public void Serialize()
    {
        var primitive = new ConstructorAndInit(123, 123.456)
        {
            Str = @"I'm a string.",
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

        var primitive = CsTomlSerializer.Deserialize<ConstructorAndInit>(buffer.WrittenSpan);
        primitive.Str.ShouldBe("I'm a string.");
        primitive.IntValue.ShouldBe(123);
        primitive.FloatValue.ShouldBe(123.456);
        primitive.BooleanValue.ShouldBeTrue();
    }
}

public class NullableReferenceTypesTest
{
    [Fact]
    public void Deserialize()
    {
        var types = CsTomlSerializer.Deserialize<NullableReferenceTypes>(""u8);
        types.Str.ShouldBeNull();
        types.NullableStr.ShouldBeNull();
        types.Uri.ShouldBeNull();
        types.NullableUri.ShouldBeNull();
        types.Version.ShouldBeNull();
        types.NullableVersion.ShouldBeNull();
        types.StringBuilder.ShouldBeNull();
        types.NullableStringBuilder.ShouldBeNull();
        types.Type.ShouldBeNull();
        types.NullableType.ShouldBeNull();
        types.BitArray.ShouldBeNull();
        types.NullableBitArray.ShouldBeNull();

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("NullableStr = \"I'm a string.\"");
        writer.AppendLine("Uri = 'https://github.com/prozolic/CsToml'");
        writer.AppendLine("NullableUri = 'https://github.com/prozolic/CsToml'");
        writer.AppendLine("Version = \"1.3.1\"");
        writer.AppendLine("NullableVersion = \"1.3.1\"");
        writer.AppendLine("StringBuilder = \"I'm a StringBuilder.\"");
        writer.AppendLine("NullableStringBuilder = \"I'm a StringBuilder.\"");
        writer.AppendLine("Type = \"System.String\"");
        writer.AppendLine("NullableType = \"System.String\"");
        writer.AppendLine("BitArray = [true, false, true]");
        writer.AppendLine("NullableBitArray = [true, false, true]");
        writer.Flush();

        var types2 = CsTomlSerializer.Deserialize<NullableReferenceTypes>(buffer.WrittenSpan);
        types2.Str.ShouldBe("I'm a string.");
        types2.NullableStr!.ShouldBe("I'm a string.");
        types2.Uri.ShouldBe(new Uri("https://github.com/prozolic/CsToml"));
        types2.NullableUri!.ShouldBe(new Uri("https://github.com/prozolic/CsToml"));
        types2.Version.ShouldBe(new Version("1.3.1"));
        types2.NullableVersion!.ShouldBe(new Version("1.3.1"));
        types2.StringBuilder.ToString().ShouldBe("I'm a StringBuilder.");
        types2.NullableStringBuilder!.ToString().ShouldBe("I'm a StringBuilder.");
        types2.Type.ShouldBe(typeof(string));
        types2.NullableType!.ShouldBe(typeof(string));
        types2.BitArray[0].ShouldBeTrue();
        types2.BitArray[1].ShouldBeFalse();
        types2.BitArray[2].ShouldBeTrue();
        types2.BitArray.Length.ShouldBe(3);
        types2.NullableBitArray![0].ShouldBeTrue();
        types2.NullableBitArray![1].ShouldBeFalse();
        types2.NullableBitArray![2].ShouldBeTrue();
        types2.NullableBitArray!.Length.ShouldBe(3);
    }
}

public class TypeImmutableTest
{
    [Fact]
    public void Serialize()
    {
        int[] array = [1, 2, 3, 4, 5];
        var set = new HashSet<int>(array);
        var queue = new Queue<int>(array);
        var immutableQueue = ImmutableQueue<int>.Empty;
        for (var i = queue.Count - 1; i >= 0; i--)
        {
            immutableQueue = immutableQueue.Enqueue(queue.Dequeue());
        }

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

        var type = new TypeImmutable()
        {
            ImmutableArray = ImmutableCollectionsMarshal.AsImmutableArray(array),
            ImmutableList = array.ToImmutableList(),
            ImmutableHashSet = set.ToImmutableHashSet(),
            ImmutableSortedSet = set.ToImmutableSortedSet(),
            ImmutableQueue = immutableQueue,
            ImmutableDictionary = dict.ToImmutableDictionary(),
            ImmutableSortedDictionary = dict.ToImmutableSortedDictionary(),
        };
        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("ImmutableArray = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableList = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableHashSet = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableSortedSet = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableQueue = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableDictionary = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
            writer.AppendLine("ImmutableSortedDictionary = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("ImmutableArray = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableList = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableHashSet = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableSortedSet = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableQueue = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableDictionary = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
            writer.AppendLine("ImmutableSortedDictionary = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("ImmutableArray = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableList = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableHashSet = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableSortedSet = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableQueue = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableDictionary = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
        writer.AppendLine("ImmutableSortedDictionary = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
        writer.Flush();

        var typeImmutable = CsTomlSerializer.Deserialize<TypeImmutable>(buffer.WrittenSpan);

        var array = typeImmutable.ImmutableArray;
        array.ShouldBe([1, 2, 3, 4, 5]);
        var list = typeImmutable.ImmutableList;
        list.ShouldBe([1, 2, 3, 4, 5]);
        var hashset = typeImmutable.ImmutableHashSet;
        hashset.ShouldBe([1, 2, 3, 4, 5]);
        var queue = typeImmutable.ImmutableHashSet;
        queue.ShouldBe([1, 2, 3, 4, 5]);

        {
            dynamic dict = typeImmutable.ImmutableDictionary;
            long value = dict["key"][0];
            value.ShouldBe(999);
            string value2 = dict["key"][1];
            value2.ShouldBe("Value");
            object[] value3 = dict["key"][2]["key"][0];
            value3.ShouldBe(new object[] { 1, 2, 3 });
            string value4 = dict["key"][2]["key"][1]["key"];
            value4.ShouldBe("value");
        }
        {
            dynamic dict = typeImmutable.ImmutableSortedDictionary;
            long value = dict["key"][0];
            value.ShouldBe(999);
            string value2 = dict["key"][1];
            value2.ShouldBe("Value");
            object[] value3 = dict["key"][2]["key"][0];
            value3.ShouldBe(new object[] { 1, 2, 3 });
            string value4 = dict["key"][2]["key"][1]["key"];
            value4.ShouldBe("value");
        }
    }
}

#if NET9_0_OR_GREATER

public class TypeOrderedDictionaryTest
{
    [Fact]
    public void Serialize()
    {
        var dict = new OrderedDictionary<string, object?>()
        {
            ["key"] = new object[]
            {
                999,
                "Value",
                new OrderedDictionary<string, object?>()
                {
                    ["key"] = new object[]
                    {
                        new long[] {1, 2, 3},
                        new OrderedDictionary<string, object?>()
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
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}] }");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]}");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeOrderedDictionary>(buffer.WrittenSpan);
        dynamic dynamicDict = type.Value;

        long value = dynamicDict["key"][0];
        value.ShouldBe(999);
        string value2 = dynamicDict["key"][1];
        value2.ShouldBe("Value");
        object[] value3 = dynamicDict["key"][2]["key"][0];
        value3.ShouldBe(new object[] { 1, 2, 3 });
        string value4 = dynamicDict["key"][2]["key"][1]["key"];
        value4.ShouldBe("value");
    }
}

public class TypeReadOnlySetFormatterTest
{
    [Fact]
    public void Serialize()
    {
        var type = new TypeReadOnlySetFormatter
        {
            Value = new ReadOnlySet<long>(new HashSet<long>([1, 2, 3, 4, 5])),
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [1,3,5]");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeReadOnlySetFormatter>(buffer.WrittenSpan);
        type.Value.ShouldBe(new ReadOnlySet<long>(new HashSet<long>([1, 3, 5])));
    }
}

#endif
