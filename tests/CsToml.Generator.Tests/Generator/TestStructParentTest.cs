using CsToml.Formatter.Resolver;
using Shouldly;
using Utf8StringInterpolation;
using CsToml.Generator.Other;

namespace CsToml.Generator.Tests;

public class TestStructParentTest
{
    private TestStructParent parent;

    public TestStructParentTest()
    {
        parent = new TestStructParent()
        {
            Value = "I'm a string.",
            TestStruct = new TestStruct { Value = 0, Str = "Test" },
            TestStructList = new List<TestStruct>()
            {
                new TestStruct { Value = 1, Str = "Test"},
                new TestStruct { Value = 2, Str = "Test2"},
                new TestStruct { Value = 3, Str = "Test3"},
            }
        };
    }

    [Fact]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(parent);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = \"I'm a string.\"");
        writer.AppendLine("TestStructList = [ {Value = 1, Str = \"Test\"}, {Value = 2, Str = \"Test2\"}, {Value = 3, Str = \"Test3\"} ]");
        writer.AppendLine("TestStruct = {Value = 0, Str = \"Test\"}");
        writer.Flush();

        var _ = CsTomlSerializer.Deserialize<TestStructParent>(buffer.WrittenSpan);

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(parent, options: Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = \"I'm a string.\"");
        writer.AppendLine("TestStructList = [ {Value = 1, Str = \"Test\"}, {Value = 2, Str = \"Test2\"}, {Value = 3, Str = \"Test3\"} ]");
        writer.AppendLine("[TestStruct]");
        writer.AppendLine("Value = 0");
        writer.AppendLine("Str = \"Test\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(parent, options: Option.ArrayHeader);
        var _ = CsTomlSerializer.Deserialize<TestStructParent>(bytes.ByteSpan);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = \"I'm a string.\"");
        writer.AppendLine("TestStruct = {Value = 0, Str = \"Test\"}");
        writer.AppendLine("[[TestStructList]]");
        writer.AppendLine("Value = 1");
        writer.AppendLine("Str = \"Test\"");
        writer.AppendLine();
        writer.AppendLine("[[TestStructList]]");
        writer.AppendLine("Value = 2");
        writer.AppendLine("Str = \"Test2\"");
        writer.AppendLine();
        writer.AppendLine("[[TestStructList]]");
        writer.AppendLine("Value = 3");
        writer.AppendLine("Str = \"Test3\"");
        writer.AppendLine();
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(parent, options: Option.HeaderAndArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = \"I'm a string.\"");
        writer.AppendLine("[[TestStructList]]");
        writer.AppendLine("Value = 1");
        writer.AppendLine("Str = \"Test\"");
        writer.AppendLine();
        writer.AppendLine("[[TestStructList]]");
        writer.AppendLine("Value = 2");
        writer.AppendLine("Str = \"Test2\"");
        writer.AppendLine();
        writer.AppendLine("[[TestStructList]]");
        writer.AppendLine("Value = 3");
        writer.AppendLine("Str = \"Test3\"");
        writer.AppendLine();
        writer.AppendLine("[TestStruct]");
        writer.AppendLine("Value = 0");
        writer.AppendLine("Str = \"Test\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = \"I'm a string.\"");
            writer.AppendLine("TestStructList = [ {Value = 1, Str = \"Test\"}, {Value = 2, Str = \"Test2\"}, {Value = 3, Str = \"Test3\"} ]");
            writer.AppendLine("TestStruct.Value = 0");
            writer.AppendLine("TestStruct.Str = \"Test\"");
            writer.Flush();

            var parent = CsTomlSerializer.Deserialize<TestStructParent>(buffer.WrittenSpan);
            Validate(ref parent);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = \"I'm a string.\"");
            writer.AppendLine("TestStructList = [ {Value = 1, Str = \"Test\"}, {Value = 2, Str = \"Test2\"}, {Value = 3, Str = \"Test3\"} ]");
            writer.AppendLine("[TestStruct]");
            writer.AppendLine("Value = 0");
            writer.AppendLine("Str = \"Test\"");
            writer.Flush();

            var parent = CsTomlSerializer.Deserialize<TestStructParent>(buffer.WrittenSpan);
            Validate(ref parent);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = \"I'm a string.\"");
            writer.AppendLine("TestStructList = [ {Value = 1, Str = \"Test\"}, {Value = 2, Str = \"Test2\"}, {Value = 3, Str = \"Test3\"} ]");
            writer.AppendLine("TestStruct = {Value = 0, Str = \"Test\"}");
            writer.Flush();

            var parent = CsTomlSerializer.Deserialize<TestStructParent>(buffer.WrittenSpan);
            Validate(ref parent);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = \"I'm a string.\"");
            writer.AppendLine("TestStruct = {Value = 0, Str = \"Test\"}");
            writer.AppendLine("[[TestStructList]]");
            writer.AppendLine("Value = 1");
            writer.AppendLine("Str = \"Test\"");
            writer.AppendLine();
            writer.AppendLine("[[TestStructList]]");
            writer.AppendLine("Value = 2");
            writer.AppendLine("Str = \"Test2\"");
            writer.AppendLine();
            writer.AppendLine("[[TestStructList]]");
            writer.AppendLine("Value = 3");
            writer.AppendLine("Str = \"Test3\"");
            writer.AppendLine();
            writer.Flush();

            var parent = CsTomlSerializer.Deserialize<TestStructParent>(buffer.WrittenSpan);
            Validate(ref parent);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = \"I'm a string.\"");
            writer.AppendLine("[[TestStructList]]");
            writer.AppendLine("Value = 1");
            writer.AppendLine("Str = \"Test\"");
            writer.AppendLine();
            writer.AppendLine("[[TestStructList]]");
            writer.AppendLine("Value = 2");
            writer.AppendLine("Str = \"Test2\"");
            writer.AppendLine();
            writer.AppendLine("[[TestStructList]]");
            writer.AppendLine("Value = 3");
            writer.AppendLine("Str = \"Test3\"");
            writer.AppendLine();
            writer.AppendLine("[TestStruct]");
            writer.AppendLine("Value = 0");
            writer.AppendLine("Str = \"Test\"");
            writer.Flush();

            var parent = CsTomlSerializer.Deserialize<TestStructParent>(buffer.WrittenSpan);
            Validate(ref parent);
        }

        static void Validate(ref TestStructParent parent)
        {
            parent.Value.ShouldBe("I'm a string.");
            parent.TestStructList.ShouldBe(new List<TestStruct>
            {
                new TestStruct { Value = 1, Str = "Test"},
                new TestStruct { Value = 2, Str = "Test2"},
                new TestStruct { Value = 3, Str = "Test3"},
            });
            parent.TestStruct.ShouldBe(new TestStruct { Value = 0, Str = "Test" });
        }
    }
}
