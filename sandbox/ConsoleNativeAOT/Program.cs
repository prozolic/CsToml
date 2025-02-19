using CsToml;
using CsToml.Formatter;
using CsToml.Formatter.Resolver;
using System.Buffers;
using System.Text;
using ConsoleNativeAOT;
using System.Reflection.Metadata;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using CsToml.Values;
using System.Collections.Immutable;

Console.WriteLine("Hello, World!");

TomlValueFormatterResolver.Register(new TestStruct2Formatter()); // Custom type formatter registration.
TomlValueFormatterResolver.Register(new CustomFormatter()); // Custom type formatter registration.

var tomlText = @"
str = ""value""
int = 123
flt = 3.1415
boolean = true
odt1 = 1979-05-27T07:32:00Z
ldt1 = 1979-05-27T07:32:00
ldt2 = 1979-05-27T00:32:00.999999
ld1 = 1979-05-27
lt1 = 07:32:00

key = ""value""
first.second.third = ""value""
number = 123456
intArray = [1, 2, 3, 4, 5]
Pair =  [1, ""value""]
inlineTable = { key = 1 , key2 = ""value"" , key3 = [ 123, 456, 789], key4 = { key = ""inlinetable"" }}
Dict = {key = 1, key2 = 2, key3 = 3, key4 = 4, key5 = 5}
Dict2 = {key = 10, key2 = 20, key3 = 30, key4 = 40, key5 = 50}
Dict3 = {123 = ""Value"", -1 = ""Value"", 123456789 = ""Value""}
TwoValueTuple = [ 1, 2 ]
TestStructArr = [{Value = 999, Str = ""Test""}, {Value = 9999, Str = ""Test2""}]
TestStruct2 = 12345

dotted.keys = ""value""
configurations = [1, {key = [ { key2 = [""VALUE""]}]}]

[Table.test]
key = ""value""
first.second.third = ""value""
number = 123456

[[arrayOfTables.test]]
key = ""value""
first.second.third = ""value""
number = 123456

[[arrayOfTables.test]]

[[arrayOfTables.test]]
key2 = ""value""
first2.second2.third2 = ""value""
number2 = 123456

"u8;

Console.WriteLine("CsTomlSerializer.Deserialize<TomlDocument>");
var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
var dottedKeys = document!.RootNode["dotted"u8]["keys"u8].GetString();
var configurations = document!.RootNode["configurations"u8][1]["key"u8][0]["key2"u8][0].GetString();

var rootNode = document.RootNode;
Console.WriteLine(rootNode["str"u8].GetString());
Console.WriteLine(rootNode["int"u8].GetInt64());
Console.WriteLine(rootNode["flt"u8].GetDouble());
Console.WriteLine(rootNode["boolean"u8].GetBool());
Console.WriteLine("CsTomlSerializer.Deserialize<TomlDocument> END");

Console.WriteLine("CsTomlSerializer.Deserialize<TestClass>");
var value = CsTomlSerializer.Deserialize<TestClass>(tomlText);
Console.WriteLine(value.str);
Console.WriteLine(value.IntValue);
Console.WriteLine(value.flt);
Console.WriteLine(value.boolean);
Console.WriteLine(value.Pair);
Console.WriteLine(value.Dict);
Console.WriteLine(value.Dict2);
Console.WriteLine(value.Dict3);
Console.WriteLine(value.TwoValueTuple);
Console.WriteLine(value.TestStructArr);
Console.WriteLine(value.TestStruct2.Value);
Console.WriteLine("CsTomlSerializer.Deserialize<TestClass> END");

Console.WriteLine("CsTomlSerializer.Serialize<TestClass>");
var testClass = new TestClass()
{
    str = "value",
    IntValue = 123,
    flt = 3.1415f,
    boolean = true,
    Dict = new Dictionary<string, object?>()
    {
        ["key"] = 2,
        ["key2"] = 3,
        ["key3"] = 4,
        ["key4"] = 5,
    },
    Dict2 = new Dictionary<string, object?>()
    {
        ["key"] = 2,
        ["key2"] = 3,
        ["key3"] = 4,
        ["key4"] = 5,
    },
    Dict3 = new Dictionary<long, string>()
    {
        [123] = "Value",
        [-1] = "Value",
        [123456789] = "Value",
    },
    Pair = new KeyValuePair<int, StringBuilder>(1, new StringBuilder("value")),
    TwoValueTuple = (1, 2),
    TestStructArr = new List<TestStruct?>()
    {
         new TestStruct() { Value = 999, Str = "Test" },
         new TestStruct() { Value = 999, Str = "Test" },
    }.ToImmutableHashSet(),
    TestStruct2 = new Custom() { Value = 12345 }
};

using var text = CsTomlSerializer.Serialize(testClass);
Console.WriteLine(text.ToString());
Console.WriteLine("CsTomlSerializer.Serialize<TestClass> END");

var deserializedTestClass = CsTomlSerializer.Deserialize<TestClass>(text.ByteSpan);

var testEnum = CsTomlSerializer.Deserialize<TestEnum>(@"color = ""Red"""u8);
Console.WriteLine(testEnum.color);

var sample = CsTomlSerializer.Deserialize<Sample>(@"Key = 12345"u8);
Console.WriteLine($"Sample.Custom = {sample.Key.Value}");

var type = new TypeImmutable()
{
ImmutableArray = [new TypeTable() { Value = "[1] This is TypeTable in ImmutableArray" },
                              new TypeTable() { Value = "[2] This is TypeTable in ImmutableArray" },
                              new TypeTable() { Value = "[3] This is TypeTable in ImmutableArray" }],
ImmutableList = [new TypeTable() { Value = "[1] This is TypeTable in ImmutableList" },
                              new TypeTable() { Value = "[2] This is TypeTable in ImmutableList" },
                              new TypeTable() { Value = "[3] This is TypeTable in ImmutableList" }],
IImmutableList = [new TypeTable() { Value = "[1] This is TypeTable in IImmutableList" },
                              new TypeTable() { Value = "[2] This is TypeTable in IImmutableList" },
                              new TypeTable() { Value = "[3] This is TypeTable in IImmutableList" }],
};

Console.WriteLine($"Check TypeImmutable");
using var bytes = CsTomlSerializer.Serialize(type);
var typeImmutable2 = CsTomlSerializer.Deserialize<TypeImmutable>(bytes.ByteSpan);
Console.WriteLine($"{nameof(typeImmutable2.ImmutableArray)}.Length = {typeImmutable2.ImmutableArray.Length}");
Console.WriteLine($"{nameof(typeImmutable2.ImmutableArray)}[0] = {typeImmutable2.ImmutableArray[0].Value}");
Console.WriteLine($"{nameof(typeImmutable2.ImmutableArray)}[1] = {typeImmutable2.ImmutableArray[1].Value}");
Console.WriteLine($"{nameof(typeImmutable2.ImmutableArray)}[2] = {typeImmutable2.ImmutableArray[2].Value}");
Console.WriteLine($"{nameof(typeImmutable2.ImmutableList)}.Count = {typeImmutable2.ImmutableList.Count}");
Console.WriteLine($"{nameof(typeImmutable2.ImmutableList)}[0] = {typeImmutable2.ImmutableList[0].Value}");
Console.WriteLine($"{nameof(typeImmutable2.ImmutableList)}[1] = {typeImmutable2.ImmutableList[1].Value}");
Console.WriteLine($"{nameof(typeImmutable2.ImmutableList)}[2] = {typeImmutable2.ImmutableList[2].Value}");
Console.WriteLine($"{nameof(typeImmutable2.IImmutableList)}.Count = {typeImmutable2.IImmutableList.Count}");
Console.WriteLine($"{nameof(typeImmutable2.IImmutableList)}[0] = {typeImmutable2.IImmutableList[0].Value}");
Console.WriteLine($"{nameof(typeImmutable2.IImmutableList)}[1] = {typeImmutable2.IImmutableList[1].Value}");
Console.WriteLine($"{nameof(typeImmutable2.IImmutableList)}[2] = {typeImmutable2.IImmutableList[2].Value}");
Console.WriteLine("END!");

[TomlSerializedObject]
public partial class TestClass
{
    [TomlValueOnSerialized]
    public string str { get; set; }

    [TomlValueOnSerialized("int")]
    public long IntValue { get; set; }
    [TomlValueOnSerialized]
    public float flt { get; set; }
    [TomlValueOnSerialized]
    public bool boolean { get; set; }

    [TomlValueOnSerialized()]
    public Dictionary<string, object?> Dict { get; set; }

    [TomlValueOnSerialized()]
    public IReadOnlyDictionary<string, object?> Dict2 { get; set; }

    [TomlValueOnSerialized()]
    public Dictionary<long, string> Dict3 { get; set; }

    [TomlValueOnSerialized]
    public KeyValuePair<int, StringBuilder> Pair { get; set; }

    [TomlValueOnSerialized]
    public ValueTuple<int, int>? TwoValueTuple { get; set; } = default!;

    [TomlValueOnSerialized]
    public IImmutableSet<TestStruct?> TestStructArr { get; set; }

    [TomlValueOnSerialized]
    public Custom TestStruct2 { get; set; }

}

[TomlSerializedObject]
public partial struct TestStruct
{
    [TomlValueOnSerialized]
    public int Value { get; set; }

    [TomlValueOnSerialized]
    public string Str { get; set; }
}

public class TestStruct2Formatter : ITomlValueFormatter<TestStruct2>
{
    public TestStruct2 Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetInt64(out var value))
        {
            return new TestStruct2() { Value = value };
        }

        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TestStruct2 target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteInt64(target.Value);
    }
}

[TomlSerializedObject]
public partial class TestEnum
{
    [TomlValueOnSerialized]
    public Color color { get; set; }
}

public class CustomFormatter : ITomlValueFormatter<Custom>
{
    public Custom Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.ValueType == TomlValueType.Integer)
        {
            return new Custom(rootNode.GetInt64());
        }

        if (rootNode.ValueType == TomlValueType.String)
        {
            return new Custom(rootNode.GetString());
        }

        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Custom target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteInt64(target.Value);
    }
}