﻿using CsToml.Error;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests.Seirialization;

public class TypeZeroTest
{
    [Fact]
    public void Serialize()
    {
        var type = new TypeZero();
        using var bytes = CsTomlSerializer.Serialize(type);

        bytes.ByteSpan.ToArray().Should().Equal(""u8.ToArray());
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
        var type = new TypeOne();
        type.Value = 999;
        using var bytes = CsTomlSerializer.Serialize(type);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().Should().Equal(expected);
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
        var type = new TypeTwo();
        type.Value = 999;
        type.Value2 = 123;
        using var bytes = CsTomlSerializer.Serialize(type);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Value2 = 123");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().Should().Equal(expected);
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
        var type = new TypeRecord();
        type.Value = 999;
        type.Str = "Test";
        using var bytes = CsTomlSerializer.Serialize(type);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Str = \"Test\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().Should().Equal(expected);
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
        var type = new TestStruct();
        type.Value = 999;
        type.Str = "Test";
        using var bytes = CsTomlSerializer.Serialize(type);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Str = \"Test\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().Should().Equal(expected);
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
        var type = new TestRecordStruct();
        type.Value = 999;
        type.Str = "Test";
        using var bytes = CsTomlSerializer.Serialize(type);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Str = \"Test\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().Should().Equal(expected);
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
        var type = new TypeIgnore();
        type.Value = 999;
        type.Str = "This is TypeIgnore";
        using var bytes = CsTomlSerializer.Serialize(type);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().Should().Equal(expected);
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
        var type = new WithArray();
        type.Value = [1, 2, 3, 4, 5];
        using var bytes = CsTomlSerializer.Serialize(type);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().Should().Equal(expected);
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
        var type = new WithArray2();
        type.Value = [[1,2,3],[4,5]];
        using var bytes = CsTomlSerializer.Serialize(type);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [ [ 1, 2, 3 ], [ 4, 5 ] ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().Should().Equal(expected);
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
        var primitive = new TomlPrimitive();
        primitive.Str = @"I'm a string.";
        primitive.Long = 123;
        primitive.Float = 123.456;
        primitive.Boolean = true;
        primitive.OffsetDateTime = new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero);
        primitive.LocalDateTime = new DateTime(1979, 5, 27, 7, 32, 0);
        primitive.LocalDate = new DateOnly(1979, 5, 27);
        primitive.LocalTime = new TimeOnly(7, 32, 30);

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
        using var bytes = CsTomlSerializer.Serialize(type);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().Should().Equal(expected);
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
        using var bytes = CsTomlSerializer.Serialize(table);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Table2.Table3.Value = \"This is TypeTable3\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().Should().Equal(expected);
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Table2.Table3.Value = \"This is TypeTable3\"");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeTable>(buffer.WrittenSpan);
        type.Table2.Table3.Value.Should().Be("This is TypeTable3");
    }
}
