using CsToml;
using System.Text;

Console.WriteLine("Hello, World!");

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
        new TestStruct() { Value = 9999, Str = "Test2" },
    },
};

using var text = CsTomlSerializer.Serialize(testClass);
Console.WriteLine(text.ToString());
Console.WriteLine("CsTomlSerializer.Serialize<TestClass> END");

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
    public List<TestStruct?> TestStructArr { get; set; }
}

[TomlSerializedObject]
public partial struct TestStruct
{
    [TomlValueOnSerialized]
    public int Value { get; set; }

    [TomlValueOnSerialized]
    public string Str { get; set; }
}