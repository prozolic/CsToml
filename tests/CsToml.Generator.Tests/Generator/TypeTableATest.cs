using Shouldly;
using System.Collections.Concurrent;
using Utf8StringInterpolation;
using CsToml.Generator.Other;

namespace CsToml.Generator.Tests;

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
            writer.AppendLine("TableB = {Value = \"This is TypeTableB\", TableECollection = [ {TableF.Value = \"[1] This is TypeTableF\"}, {TableF.Value = \"[2] This is TypeTableF\"}, {TableF.Value = \"[3] This is TypeTableF\"} ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}");
            writer.Flush();

            var _ = CsTomlSerializer.Deserialize<TypeTableA>(buffer.WrittenSpan);

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(tableA, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Dict]");
            writer.AppendLine("1 = \"2\"");
            writer.AppendLine("3 = \"4\"");
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
            writer.AppendLine("Dict = {1 = \"2\", 3 = \"4\" }");
            writer.AppendLine("TableB.Value = \"This is TypeTableB\"");
            writer.AppendLine("TableB.TableECollection = [ {TableF.Value = \"[1] This is TypeTableF\"}, {TableF.Value = \"[2] This is TypeTableF\"}, {TableF.Value = \"[3] This is TypeTableF\"} ]");
            writer.AppendLine("TableB.TableC.Value = \"This is TypeTableC\"");
            writer.AppendLine("TableB.TableC.TableD.Value = \"This is TypeTableD\"");
            writer.Flush();

            var tableA = CsTomlSerializer.Deserialize<TypeTableA>(buffer.WrittenSpan);
            Validate(tableA);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Dict]");
            writer.AppendLine("1 = \"2\"");
            writer.AppendLine("3 = \"4\"");
            writer.AppendLine("[TableB]");
            writer.AppendLine("Value = \"This is TypeTableB\"");
            writer.AppendLine("TableECollection = [ {TableF.Value = \"[1] This is TypeTableF\"}, {TableF.Value = \"[2] This is TypeTableF\"}, {TableF.Value = \"[3] This is TypeTableF\"} ]");
            writer.AppendLine("[TableB.TableC]");
            writer.AppendLine("Value = \"This is TypeTableC\"");
            writer.AppendLine("[TableB.TableC.TableD]");
            writer.AppendLine("Value = \"This is TypeTableD\"");
            writer.Flush();

            var tableA = CsTomlSerializer.Deserialize<TypeTableA>(buffer.WrittenSpan);
            Validate(tableA);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Dict = {1 = \"2\", 3 = \"4\"}");
            writer.AppendLine("TableB = {Value = \"This is TypeTableB\", TableECollection = [ {TableF.Value = \"[1] This is TypeTableF\"}, {TableF.Value = \"[2] This is TypeTableF\"}, {TableF.Value = \"[3] This is TypeTableF\"} ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}");
            writer.Flush();

            var tableA = CsTomlSerializer.Deserialize<TypeTableA>(buffer.WrittenSpan);
            Validate(tableA);
        }

        static void Validate(TypeTableA typeTableA)
        {
            typeTableA.Dict.Count.ShouldBe(2);
            typeTableA.Dict[1].ShouldBe("2");
            typeTableA.Dict[3].ShouldBe("4");
            typeTableA.TableB.Value.ShouldBe("This is TypeTableB");
            typeTableA.TableB.TableC.Value.ShouldBe("This is TypeTableC");
            typeTableA.TableB.TableC.TableD.Value.ShouldBe("This is TypeTableD");
            typeTableA.TableB.TableECollection.Count.ShouldBe(3);
            typeTableA.TableB.TableECollection[0].TableF.Value.ShouldBe("[1] This is TypeTableF");
            typeTableA.TableB.TableECollection[1].TableF.Value.ShouldBe("[2] This is TypeTableF");
            typeTableA.TableB.TableECollection[2].TableF.Value.ShouldBe("[3] This is TypeTableF");
        }
    }
}
