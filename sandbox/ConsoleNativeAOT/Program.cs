using CsToml;
using CsToml.Formatter;
using CsToml.Formatter.Resolver;
using System.Buffers;
using System.Text;
using ConsoleNativeAOT;

Console.WriteLine("Hello, World!");

GeneratedFormatterResolver.Register(new TestStruct2Formatter()); // Custom type formatter registration.

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
var res = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
var rootNode = res.RootNode;
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
    TestStructArr = new List<TestStruct>()
    {
         new TestStruct() { Value = 999, Str = "Test" },
         new TestStruct() { Value = 999, Str = "Test" },
    },
    TestStruct2 = new TestStruct2() { Value = 12345 }
};

using var text = CsTomlSerializer.Serialize(testClass);
Console.WriteLine(text.ToString());
Console.WriteLine("CsTomlSerializer.Serialize<TestClass> END");

var deserializedTestClass = CsTomlSerializer.Deserialize<TestClass>(text.ByteSpan);

var testEnum = CsTomlSerializer.Deserialize<TestEnum>(@"color = ""Red"""u8);
Console.WriteLine(testEnum.color);

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
    public ValueTuple<int, int> TwoValueTuple { get; set; } = default!;

    [TomlValueOnSerialized]
    public List<TestStruct> TestStructArr { get; set; }

    [TomlValueOnSerialized]
    public TestStruct2 TestStruct2 { get; set; }

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


