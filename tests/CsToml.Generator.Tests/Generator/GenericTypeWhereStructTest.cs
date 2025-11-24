using CsToml.Formatter.Resolver;
using Shouldly;
using Utf8StringInterpolation;
using CsToml.Generator.Other;

namespace CsToml.Generator.Tests;

public class GenericTypeWhereStructTest
{
    [Fact]
    public void Deserialize()
    {
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 123");
            writer.AppendLine("NullableValue = 456");
            writer.Flush();

            var value = CsTomlSerializer.Deserialize<GenericTypeWhereStruct<int>>(buffer.WrittenSpan);
            value.Value.ShouldBe(123);
            value.NullableValue.ShouldBe(456);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 10, \"ten\" ]");
            writer.AppendLine("NullableValue = [ 11, \"eleven\" ]");
            writer.Flush();

            var value = CsTomlSerializer.Deserialize<GenericTypeWhereStruct<ValueTuple<int, string>>>(buffer.WrittenSpan);
            value.Value.ShouldBe((10, "ten"));
            value.NullableValue.GetValueOrDefault().ShouldBe((11, "eleven"));
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value.Value = \"This is SimpleStruct\"");
            writer.AppendLine("NullableValue.Value = \"This is Nullable SimpleStruct\"");
            writer.Flush();

            var value = CsTomlSerializer.Deserialize<GenericTypeWhereStruct<SimpleStruct>>(buffer.WrittenSpan);
            value.Value.Value.ShouldBe("This is SimpleStruct");
            value.NullableValue.GetValueOrDefault().Value.ShouldBe("This is Nullable SimpleStruct");
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Value]");
            writer.AppendLine("Value = \"This is SimpleStruct\"");
            writer.AppendLine("[NullableValue]");
            writer.AppendLine("Value = \"This is Nullable SimpleStruct\"");
            writer.Flush();

            var value = CsTomlSerializer.Deserialize<GenericTypeWhereStruct<SimpleStruct>>(buffer.WrittenSpan);
            value.Value.Value.ShouldBe("This is SimpleStruct");
            value.NullableValue.GetValueOrDefault().Value.ShouldBe("This is Nullable SimpleStruct");
        }
    }

    [Fact]
    public void Serialize()
    {
        {
            var intValue = new GenericTypeWhereStruct<int>()
            {
                Value = 123,
                NullableValue = 456
            };
            using var bytes = CsTomlSerializer.Serialize(intValue);
            var __ = CsTomlSerializer.Deserialize<GenericTypeWhereStruct<int>>(bytes.ByteSpan);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 123");
            writer.AppendLine("NullableValue = 456");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            var intValue = new GenericTypeWhereStruct<ValueTuple<int, string>>()
            {
                Value = (10, "ten"),
                NullableValue = (11, "eleven")
            };
            using var bytes = CsTomlSerializer.Serialize(intValue);
            var __ = CsTomlSerializer.Deserialize<GenericTypeWhereStruct<ValueTuple<int, string>>>(bytes.ByteSpan);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 10, \"ten\" ]");
            writer.AppendLine("NullableValue = [ 11, \"eleven\" ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            var value = new GenericTypeWhereStruct<SimpleStruct>()
            {
                Value = new SimpleStruct() { Value = "This is SimpleStruct" },
                NullableValue = new SimpleStruct() { Value = "This is Nullable SimpleStruct" }
            };
            using var bytes = CsTomlSerializer.Serialize(value);
            var __ = CsTomlSerializer.Deserialize<GenericTypeWhereStruct<SimpleStruct>>(bytes.ByteSpan);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value.Value = \"This is SimpleStruct\"");
            writer.AppendLine("NullableValue.Value = \"This is Nullable SimpleStruct\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            var value = new GenericTypeWhereStruct<SimpleStruct>()
            {
                Value = new SimpleStruct() { Value = "This is SimpleStruct" },
                NullableValue = new SimpleStruct() { Value = "This is Nullable SimpleStruct" }
            };
            using var bytes = CsTomlSerializer.Serialize(value, options: Option.Header);
            var __ = CsTomlSerializer.Deserialize<GenericTypeWhereStruct<SimpleStruct>>(bytes.ByteSpan);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Value]");
            writer.AppendLine("Value = \"This is SimpleStruct\"");
            writer.AppendLine("[NullableValue]");
            writer.AppendLine("Value = \"This is Nullable SimpleStruct\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }
}
