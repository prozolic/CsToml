using CsToml.Generator.Other;
using Shouldly;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeTableATest
{
    private TypeTableA tableA;

    public TypeTableATest()
    {
        tableA = new TypeTableA
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
    }

    [Fact]
    public void Serialize()
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

    [Fact]
    public void SerializeWithHeaderOption()
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

    [Fact]
    public void SerializeWithArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(tableA, Option.ArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Dict = {1 = \"2\", 3 = \"4\"}");
        writer.AppendLine("TableB = {Value = \"This is TypeTableB\", TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}, TableECollection = [ {TableF.Value = \"[1] This is TypeTableF\"}, {TableF.Value = \"[2] This is TypeTableF\"}, {TableF.Value = \"[3] This is TypeTableF\"} ]}");
        writer.Flush();

        var _ = CsTomlSerializer.Deserialize<TypeTableA>(buffer.WrittenSpan);

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(tableA, Option.HeaderAndArrayHeader);

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

        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Dict = {1 = \"2\", 3 = \"4\"}");
            writer.AppendLine("TableB = {Value = \"This is TypeTableB\", TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}, TableECollection = [ {TableF.Value = \"[1] This is TypeTableF\"}, {TableF.Value = \"[2] This is TypeTableF\"}, {TableF.Value = \"[3] This is TypeTableF\"} ]}");
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

[TomlSerializedObject]
internal partial class TypeTableAList
{
    [TomlValueOnSerialized(NullHandling = TomlNullHandling.Ignore)]
    public List<TypeTableA>? Values { get; set; }
}

public class TypeTableAListTest
{
    private TypeTableAList typeTableAList;

    public TypeTableAListTest()
    {
        typeTableAList = new TypeTableAList() {
            Values = [new TypeTableA
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
            },
            new TypeTableA
            {
                Dict = new ConcurrentDictionary<int, string>()
                {
                    [5] = "6",
                    [7] = "8",
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
                        new TypeTableE(){ TableF = new TypeTableF() { Value = "[4] This is TypeTableF" } },
                        new TypeTableE(){ TableF = new TypeTableF() { Value = "[5] This is TypeTableF" } },
                    ],
                }
            },
            new TypeTableA
            {
                Dict = new ConcurrentDictionary<int, string>(),
                TableB = new TypeTableB()
                {
                    Value = "This is TypeTableB",
                    TableC = new TypeTableC
                    {
                        Value = "This is TypeTableC",
                        TableD = new TypeTableD() { Value = "This is TypeTableD" }
                    },
                    TableECollection = [
                        new TypeTableE(){ TableF = new TypeTableF() { Value = "[6] This is TypeTableF" } },
                        new TypeTableE(){ TableF = new TypeTableF() { Value = "[7] This is TypeTableF" } },
                    ],
                }
            },
            new TypeTableA
            {
                Dict = new ConcurrentDictionary<int, string>()
                {
                    [9] = "10",
                },
                TableB = new TypeTableB()
                {
                    Value = "This is TypeTableB",
                    TableC = new TypeTableC
                    {
                        Value = "This is TypeTableC",
                        TableD = new TypeTableD() { Value = "This is TypeTableD" }
                    },
                    TableECollection = [],
                }
            }]
        };
    }

    [Fact]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(typeTableAList);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Values = [ {Dict = {1 = \"2\", 3 = \"4\"}, TableB = {Value = \"This is TypeTableB\", TableECollection = [ {TableF.Value = \"[1] This is TypeTableF\"}, {TableF.Value = \"[2] This is TypeTableF\"}, {TableF.Value = \"[3] This is TypeTableF\"} ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}}, {Dict = {5 = \"6\", 7 = \"8\"}, TableB = {Value = \"This is TypeTableB\", TableECollection = [ {TableF.Value = \"[4] This is TypeTableF\"}, {TableF.Value = \"[5] This is TypeTableF\"} ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}}, {Dict = {}, TableB = {Value = \"This is TypeTableB\", TableECollection = [ {TableF.Value = \"[6] This is TypeTableF\"}, {TableF.Value = \"[7] This is TypeTableF\"} ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}}, {Dict = {9 = \"10\"}, TableB = {Value = \"This is TypeTableB\", TableECollection = [ ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}} ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeTableAList, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Values = [ {Dict = {1 = \"2\", 3 = \"4\"}, TableB = {Value = \"This is TypeTableB\", TableECollection = [ {TableF.Value = \"[1] This is TypeTableF\"}, {TableF.Value = \"[2] This is TypeTableF\"}, {TableF.Value = \"[3] This is TypeTableF\"} ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}}, {Dict = {5 = \"6\", 7 = \"8\"}, TableB = {Value = \"This is TypeTableB\", TableECollection = [ {TableF.Value = \"[4] This is TypeTableF\"}, {TableF.Value = \"[5] This is TypeTableF\"} ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}}, {Dict = {}, TableB = {Value = \"This is TypeTableB\", TableECollection = [ {TableF.Value = \"[6] This is TypeTableF\"}, {TableF.Value = \"[7] This is TypeTableF\"} ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}}, {Dict = {9 = \"10\"}, TableB = {Value = \"This is TypeTableB\", TableECollection = [ ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}} ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeTableAList, Option.ArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[[Values]]");
        writer.AppendLine("Dict = {1 = \"2\", 3 = \"4\"}");
        writer.AppendLine("TableB = {Value = \"This is TypeTableB\", TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}, TableECollection = [ {TableF.Value = \"[1] This is TypeTableF\"}, {TableF.Value = \"[2] This is TypeTableF\"}, {TableF.Value = \"[3] This is TypeTableF\"} ]}");
        writer.AppendLine("");
        writer.AppendLine("[[Values]]");
        writer.AppendLine("Dict = {5 = \"6\", 7 = \"8\"}");
        writer.AppendLine("TableB = {Value = \"This is TypeTableB\", TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}, TableECollection = [ {TableF.Value = \"[4] This is TypeTableF\"}, {TableF.Value = \"[5] This is TypeTableF\"} ]}");
        writer.AppendLine("");
        writer.AppendLine("[[Values]]");
        writer.AppendLine("Dict = {}");
        writer.AppendLine("TableB = {Value = \"This is TypeTableB\", TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}, TableECollection = [ {TableF.Value = \"[6] This is TypeTableF\"}, {TableF.Value = \"[7] This is TypeTableF\"} ]}");
        writer.AppendLine("");
        writer.AppendLine("[[Values]]");
        writer.AppendLine("Dict = {9 = \"10\"}");
        writer.AppendLine("TableB = {Value = \"This is TypeTableB\", TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}, TableECollection = [ ]}");
        writer.AppendLine("");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeTableAList, Option.HeaderAndArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[[Values]]");
        writer.AppendLine("Dict = {1 = \"2\", 3 = \"4\"}");
        writer.AppendLine("TableB = {Value = \"This is TypeTableB\", TableECollection = [ {TableF.Value = \"[1] This is TypeTableF\"}, {TableF.Value = \"[2] This is TypeTableF\"}, {TableF.Value = \"[3] This is TypeTableF\"} ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}");
        writer.AppendLine("");
        writer.AppendLine("[[Values]]");
        writer.AppendLine("Dict = {5 = \"6\", 7 = \"8\"}");
        writer.AppendLine("TableB = {Value = \"This is TypeTableB\", TableECollection = [ {TableF.Value = \"[4] This is TypeTableF\"}, {TableF.Value = \"[5] This is TypeTableF\"} ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}");
        writer.AppendLine("");
        writer.AppendLine("[[Values]]");
        writer.AppendLine("Dict = {}");
        writer.AppendLine("TableB = {Value = \"This is TypeTableB\", TableECollection = [ {TableF.Value = \"[6] This is TypeTableF\"}, {TableF.Value = \"[7] This is TypeTableF\"} ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}");
        writer.AppendLine("");
        writer.AppendLine("[[Values]]");
        writer.AppendLine("Dict = {9 = \"10\"}");
        writer.AppendLine("TableB = {Value = \"This is TypeTableB\", TableECollection = [ ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}");
        writer.AppendLine("");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Values = [ {Dict = {1 = \"2\", 3 = \"4\"}, TableB = {Value = \"This is TypeTableB\", TableECollection = [ {TableF.Value = \"[1] This is TypeTableF\"}, {TableF.Value = \"[2] This is TypeTableF\"}, {TableF.Value = \"[3] This is TypeTableF\"} ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}}, {Dict = {5 = \"6\", 7 = \"8\"}, TableB = {Value = \"This is TypeTableB\", TableECollection = [ {TableF.Value = \"[4] This is TypeTableF\"}, {TableF.Value = \"[5] This is TypeTableF\"} ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}}, {Dict = {}, TableB = {Value = \"This is TypeTableB\", TableECollection = [ {TableF.Value = \"[6] This is TypeTableF\"}, {TableF.Value = \"[7] This is TypeTableF\"} ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}}, {Dict = {9 = \"10\"}, TableB = {Value = \"This is TypeTableB\", TableECollection = [ ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}} ]");
            writer.Flush();

            var typeTableAList = CsTomlSerializer.Deserialize<TypeTableAList>(buffer.WrittenSpan);
            Validate(typeTableAList);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[[Values]]");
            writer.AppendLine("Dict = {1 = \"2\", 3 = \"4\"}");
            writer.AppendLine("TableB = {Value = \"This is TypeTableB\", TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}, TableECollection = [ {TableF.Value = \"[1] This is TypeTableF\"}, {TableF.Value = \"[2] This is TypeTableF\"}, {TableF.Value = \"[3] This is TypeTableF\"} ]}");
            writer.AppendLine("");
            writer.AppendLine("[[Values]]");
            writer.AppendLine("Dict = {5 = \"6\", 7 = \"8\"}");
            writer.AppendLine("TableB = {Value = \"This is TypeTableB\", TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}, TableECollection = [ {TableF.Value = \"[4] This is TypeTableF\"}, {TableF.Value = \"[5] This is TypeTableF\"} ]}");
            writer.AppendLine("");
            writer.AppendLine("[[Values]]");
            writer.AppendLine("Dict = {}");
            writer.AppendLine("TableB = {Value = \"This is TypeTableB\", TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}, TableECollection = [ {TableF.Value = \"[6] This is TypeTableF\"}, {TableF.Value = \"[7] This is TypeTableF\"} ]}");
            writer.AppendLine("");
            writer.AppendLine("[[Values]]");
            writer.AppendLine("Dict = {9 = \"10\"}");
            writer.AppendLine("TableB = {Value = \"This is TypeTableB\", TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}, TableECollection = [ ]}");
            writer.AppendLine("");
            writer.Flush();

            var typeTableAList = CsTomlSerializer.Deserialize<TypeTableAList>(buffer.WrittenSpan);
            Validate(typeTableAList);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[[Values]]");
            writer.AppendLine("Dict = {1 = \"2\", 3 = \"4\"}");
            writer.AppendLine("TableB = {Value = \"This is TypeTableB\", TableECollection = [ {TableF.Value = \"[1] This is TypeTableF\"}, {TableF.Value = \"[2] This is TypeTableF\"}, {TableF.Value = \"[3] This is TypeTableF\"} ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}");
            writer.AppendLine("");
            writer.AppendLine("[[Values]]");
            writer.AppendLine("Dict = {5 = \"6\", 7 = \"8\"}");
            writer.AppendLine("TableB = {Value = \"This is TypeTableB\", TableECollection = [ {TableF.Value = \"[4] This is TypeTableF\"}, {TableF.Value = \"[5] This is TypeTableF\"} ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}");
            writer.AppendLine("");
            writer.AppendLine("[[Values]]");
            writer.AppendLine("Dict = {}");
            writer.AppendLine("TableB = {Value = \"This is TypeTableB\", TableECollection = [ {TableF.Value = \"[6] This is TypeTableF\"}, {TableF.Value = \"[7] This is TypeTableF\"} ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}");
            writer.AppendLine("");
            writer.AppendLine("[[Values]]");
            writer.AppendLine("Dict = {9 = \"10\"}");
            writer.AppendLine("TableB = {Value = \"This is TypeTableB\", TableECollection = [ ], TableC = {Value = \"This is TypeTableC\", TableD = {Value = \"This is TypeTableD\"}}}");
            writer.AppendLine("");
            writer.Flush();

            var typeTableAList = CsTomlSerializer.Deserialize<TypeTableAList>(buffer.WrittenSpan);
            Validate(typeTableAList);
        }

        static void Validate(TypeTableAList typeTableAList)
        {
            typeTableAList.Values.ShouldNotBeNull();
            typeTableAList.Values.Count.ShouldBe(4);

            typeTableAList.Values[0].Dict.Count.ShouldBe(2);
            typeTableAList.Values[0].Dict[1].ShouldBe("2");
            typeTableAList.Values[0].Dict[3].ShouldBe("4");
            typeTableAList.Values[0].TableB.Value.ShouldBe("This is TypeTableB");
            typeTableAList.Values[0].TableB.TableC.Value.ShouldBe("This is TypeTableC");
            typeTableAList.Values[0].TableB.TableC.TableD.Value.ShouldBe("This is TypeTableD");
            typeTableAList.Values[0].TableB.TableECollection.Count.ShouldBe(3);
            typeTableAList.Values[0].TableB.TableECollection[0].TableF.Value.ShouldBe("[1] This is TypeTableF");
            typeTableAList.Values[0].TableB.TableECollection[1].TableF.Value.ShouldBe("[2] This is TypeTableF");
            typeTableAList.Values[0].TableB.TableECollection[2].TableF.Value.ShouldBe("[3] This is TypeTableF");

            typeTableAList.Values[0].Dict.Count.ShouldBe(2);
            typeTableAList.Values[1].Dict[5].ShouldBe("6");
            typeTableAList.Values[1].Dict[7].ShouldBe("8");
            typeTableAList.Values[1].TableB.Value.ShouldBe("This is TypeTableB");
            typeTableAList.Values[1].TableB.TableC.Value.ShouldBe("This is TypeTableC");
            typeTableAList.Values[1].TableB.TableC.TableD.Value.ShouldBe("This is TypeTableD");
            typeTableAList.Values[1].TableB.TableECollection.Count.ShouldBe(2);
            typeTableAList.Values[1].TableB.TableECollection[0].TableF.Value.ShouldBe("[4] This is TypeTableF");
            typeTableAList.Values[1].TableB.TableECollection[1].TableF.Value.ShouldBe("[5] This is TypeTableF");

            typeTableAList.Values[2].Dict.Count.ShouldBe(0);
            typeTableAList.Values[2].TableB.Value.ShouldBe("This is TypeTableB");
            typeTableAList.Values[2].TableB.TableC.Value.ShouldBe("This is TypeTableC");
            typeTableAList.Values[2].TableB.TableC.TableD.Value.ShouldBe("This is TypeTableD");
            typeTableAList.Values[2].TableB.TableECollection.Count.ShouldBe(2);
            typeTableAList.Values[2].TableB.TableECollection[0].TableF.Value.ShouldBe("[6] This is TypeTableF");
            typeTableAList.Values[2].TableB.TableECollection[1].TableF.Value.ShouldBe("[7] This is TypeTableF");

            typeTableAList.Values[3].Dict.Count.ShouldBe(1);
            typeTableAList.Values[3].Dict[9].ShouldBe("10");
            typeTableAList.Values[3].TableB.Value.ShouldBe("This is TypeTableB");
            typeTableAList.Values[3].TableB.TableC.Value.ShouldBe("This is TypeTableC");
            typeTableAList.Values[3].TableB.TableC.TableD.Value.ShouldBe("This is TypeTableD");
            typeTableAList.Values[3].TableB.TableECollection.Count.ShouldBe(0);
        }
    }
}