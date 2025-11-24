using CsToml.Formatter.Resolver;
using Shouldly;
using Utf8StringInterpolation;
using CsToml.Generator.Other;

namespace CsToml.Generator.Tests;

public class GenericTypeTest
{
    [Fact]
    public void Deserialize()
    {
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 123");
            writer.AppendLine("NullableValue = 456");
            writer.Flush();

            var value = CsTomlSerializer.Deserialize<GenericType<int>>(buffer.WrittenSpan);
            value.Value.ShouldBe(123);
            value.NullableValue.ShouldBe(456);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = \"value\"");
            writer.AppendLine("NullableValue = \"nullablevalue\"");
            writer.Flush();

            var value = CsTomlSerializer.Deserialize<GenericType<string>>(buffer.WrittenSpan);
            value.Value.ShouldBe("value");
            value.NullableValue.ShouldBe("nullablevalue");
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value.Value = \"This is Simple\"");
            writer.AppendLine("NullableValue.Value = \"This is Nullable Simple\"");
            writer.Flush();

            var value = CsTomlSerializer.Deserialize<GenericType<Simple>>(buffer.WrittenSpan);
            value.Value.Value.ShouldBe("This is Simple");
            value.NullableValue!.Value.ShouldBe("This is Nullable Simple");
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Value]");
            writer.AppendLine("Value = \"This is Simple\"");
            writer.AppendLine("[NullableValue]");
            writer.AppendLine("Value = \"This is Nullable Simple\"");
            writer.Flush();

            var value = CsTomlSerializer.Deserialize<GenericType<Simple>>(buffer.WrittenSpan);
            value.Value.Value.ShouldBe("This is Simple");
            value.NullableValue!.Value.ShouldBe("This is Nullable Simple");
        }
    }

    [Fact]
    public void Serialize()
    {
        {
            var value = new GenericType<int>()
            {
                Value = 123,
                NullableValue = 456
            };
            using var bytes = CsTomlSerializer.Serialize(value);
            var __ = CsTomlSerializer.Deserialize<GenericType<int>>(bytes.ByteSpan);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 123");
            writer.AppendLine("NullableValue = 456");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            var value = new GenericType<string>()
            {
                Value = "value",
                NullableValue = "nullablevalue"
            };
            using var bytes = CsTomlSerializer.Serialize(value);
            var __ = CsTomlSerializer.Deserialize<GenericType<string>>(bytes.ByteSpan);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = \"value\"");
            writer.AppendLine("NullableValue = \"nullablevalue\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            var value = new GenericType<Simple>()
            {
                Value = new Simple() { Value = "This is Simple" },
                NullableValue = new Simple() { Value = "This is Nullable Simple" }
            };
            using var bytes = CsTomlSerializer.Serialize(value);
            var __ = CsTomlSerializer.Deserialize<GenericType<Simple>>(bytes.ByteSpan);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value.Value = \"This is Simple\"");
            writer.AppendLine("NullableValue.Value = \"This is Nullable Simple\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            var value = new GenericType<Simple>()
            {
                Value = new Simple() { Value = "This is Simple" },
                NullableValue = new Simple() { Value = "This is Nullable Simple" }
            };
            using var bytes = CsTomlSerializer.Serialize(value, options: Option.Header);
            var __ = CsTomlSerializer.Deserialize<GenericType<Simple>>(bytes.ByteSpan);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Value]");
            writer.AppendLine("Value = \"This is Simple\"");
            writer.AppendLine("[NullableValue]");
            writer.AppendLine("Value = \"This is Nullable Simple\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }
}
