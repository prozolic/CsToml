
using ConsoleApp;
using CsToml;
using CsToml.Error;
using CsToml.Extensions;
using System.Buffers;
using System.Text;


Console.WriteLine("Hello, World!");

var testDocument = await CsTomlFileSerializer.DeserializeAsync<TomlDocument>("./../../../Toml/test_withoutBOM.toml");
if (!testDocument.RootNode["s"u8].GetString().Equals(""))
{
}
if (!testDocument.RootNode["ss"u8].GetString().Equals("'"))
{
}
if (!testDocument.RootNode["sss"u8].GetString().Equals("''"))
{
}
if (!testDocument.RootNode["s2"u8].GetString().Equals("' "))
{
}
if (!testDocument.RootNode["s3"u8].GetString().Equals("'' "))
{
}
if (!testDocument.RootNode["s4"u8].GetString().Equals(" '"))
{
}
if (!testDocument.RootNode["s5"u8].GetString().Equals(" ''"))
{
}
if (!testDocument.RootNode["s6"u8].GetString().Equals(" ' ' '' ' ' '' ' '' ' ''   '     a ' "))
{
}
var findValue = testDocument.Find("fruit.apple"u8, isDottedKeys:true);

await CsTomlFileSerializer.SerializeAsync("serialzedTest.toml", testDocument);
var testDocument2 = CsTomlFileSerializer.Deserialize<TomlDocument>("serialzedTest.toml");

{
    var testpartDeserialized = CsTomlFileSerializer.Deserialize<TestTomlSerializedObject>("serialzedTest.toml");
}

var documentAsync = await CsTomlFileSerializer.DeserializeAsync<TomlDocument>("./../../../Toml/test.toml");
var document = CsTomlFileSerializer.Deserialize<TomlDocument>("./../../../Toml/test.toml");

using var serializedDocumentTomlText = CsTomlSerializer.Serialize(document);
var documentText = Encoding.UTF8.GetString(serializedDocumentTomlText.ByteSpan);
var document2 = CsTomlSerializer.Deserialize<TomlDocument>(serializedDocumentTomlText.ByteSpan);

{
    var valuekeyValue = document!.RootNode["bare_key"u8].GetString();
    var valuekeyValue2 = document!
        .RootNode["fruit"u8]["apple"u8]["color"u8]
        .GetString();
    var valueKeyValue3 = document!.RootNode["array"u8]["of"u8]["tables"u8][1]["arr"u8].GetArrayValue(1).GetString();
    var valueKeyValue4 = document!.RootNode["array"]["of"]["tables"][2]["name"u8]["last"u8].GetString();
    var valueKeyValue5 = document!.RootNode["contributors"u8][1]["name"u8].GetString();
    var valueKeyValue6 = document!
        .RootNode["array"u8]["of"u8]["tables"u8][2]["name"u8]["array"u8][1]["z"u8]
        .GetArrayValue(0).GetInt64();

    if (document!.TryFind("array.of.tables", out var _arrayOfTableValue, isDottedKeys:true))
    {
        if (_arrayOfTableValue!.TryGetArrayValue(0, out var _arrayOfTableValue_index0))
        {

        }
        if (_arrayOfTableValue!.TryGetArrayValue(1, out var _arrayOfTableValue_index1))
        {

        }
        if (_arrayOfTableValue!.TryGetArrayValue(2, out var _arrayOfTableValue_index2))
        {
            if (_arrayOfTableValue_index2.TryFind("name.last", out var _arrayOfTableValue_index2_, isDottedKeys:true))
            {
                var str = _arrayOfTableValue_index2_!.GetString();
            }
        }

    }
}
{
    var value2 = document!.Find("int1"u8);
    var value3 = document!.Find("int6"u8);
    var value4 = document!.Find("hex1"u8);
    var value5 = document!.Find("flt1"u8);
    var value6 = document!.Find("bool1"u8);
    var value7 = document!.Find("odt1"u8);
    var value8 = document!.Find("ldt1"u8);
    var value9 = document!.Find("lt1"u8);
    var value10 = document!.Find("integers"u8)?.GetArrayValue(0);

    value10 = document.RootNode["integers"u8].GetArrayValue(0);
    var value11 = document!.Find("nested_mixed_array"u8)?.GetArrayValue(0)?.GetArrayValue(1);
    var _ = document!.Find("integers"u8)?.TryGetArray(out var value12);
    var value13 = document!.Find("Name\\tJos\\u00E9");

    var value14 = document!.RootNode["products"u8][0]["name"u8];
    //var value15 = document!.RootNode["products"u8][3]["name"u8].GetString();
}
{
    var value = document!.Find("table-1"u8, "key1"u8);
    var value2 = document!.TryFind("table-1", "key1", out var ___);
}
{
    var value = document!.Find("fruit.apple.texture"u8, "smooth"u8, isTableHeaderAsDottedKeys:true, isDottedKeys: true);
    var value2 = document!.Find(["fruit"u8, "apple"u8, "texture"u8], "name"u8);
    var value3 = document!.TryFind("fruit.apple.texture"u8, "smooth"u8, out var ____, isDottedKeys:true);
    var value4 = document!.TryFind(["fruit"u8, "apple"u8, "texture"u8], "smooth"u8, out var _____);
}
{
    var value = document!.Find("products"u8, 0, "name"u8);
    var value2 = document!.Find("products", 0, "name");
    var value3 = document!.Find("products"u8, 1, "name"u8);
    var value4 = document!.Find("array.of.tables"u8, 2, "name.array"u8, true, true);
    var arr = value4!.GetArrayValue(1);
    var a = arr.Find("z"u8)!.GetArrayValue(0).GetInt64();
    var value5 = document!.Find("array.of.tables"u8, 2, "name.last2"u8, true, true);
    var value6 = value5!.Find("key"u8)!.GetString();
    var value7 = value5!.Find("key"u8)!.GetObject();
    var value8 = value5!.Find("key"u8)!.GetValue<string>();
    var value9 = document!.Find("integers"u8)!;
    var arr2 = value9.GetValue<IEnumerable<string>>();
    var value10 = document!.Find("int1"u8);
    var intValue = value10!.GetValue<int>();
}

var tomlText = @"
str = ""string""
int = 99
flt = 1.0
bool = true
odt = 1979-05-27T07:32:00Z
ldt = 1979-05-27T07:32:00
ld1 = 1979-05-27
array = [ 1, 2, 3 ]

[[ArrayOfTables]]
value = 1

[[ArrayOfTables]]
value2 = 2

[[ArrayOfTables]]
value3 = 3

"u8.ToArray();


try
{
    var testCsTomldocument = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
}
catch(CsTomlSerializeException ctse)
{
    foreach (var cte in ctse.Exceptions)
    {
        // A syntax error (CsTomlException) occurred while parsing line 3 of the TOML file. Check InnerException for details.
        var e = cte.InnerException;
    }
}


//if (testCsTomldocument!.TryGetValue("key", out var value22))
//{
//    var str = value22!.GetString();
//    Console.WriteLine($"key = {str}"); // key = value
//}

//using var serializedTomlText = CsTomlSerializer.Serialize(testCsTomldocument);
//Console.WriteLine(Encoding.UTF8.GetString(serializedTomlText.ByteSpan));

using var result = CsTomlSerializer.Serialize(document);
Console.WriteLine(Encoding.UTF8.GetString(result.ByteSpan));

var buffer = new ArrayBufferWriter<byte>();
CsTomlSerializer.Serialize(ref buffer, document);

var part = new TestTomlSerializedObject();
using var partBytes = CsTomlSerializer.Serialize(part);
var partDocument = CsTomlSerializer.Deserialize<TomlDocument>(partBytes.ByteSpan);

if (partDocument.TryFind(["TableValue"u8, "あいうえお"u8], out var vvvvv))
{

}

var tomlText2 = @"

value = [8, 8, 8]

LongValue = 999
boolValue = true
DateTimeValue = 1979-05-27T07:32:00Z

TableValue2 = {IntValue = 123456, IntArray = [ 1, 2, 3, 4, 5 ], UnknownValue = ""test"" }

[TableValue]
LongValue = -1
IntArray = [9,8,7,6,5,4,3,2,1,1]

"u8.ToArray();

var partfasdfasdf2 = CsTomlSerializer.Deserialize<TomlDocument>(tomlText2);
using var partBytes2 = CsTomlSerializer.Serialize(partfasdfasdf2);

var part2 = CsTomlSerializer.Deserialize<TestTomlSerializedObject>(tomlText2);


Console.WriteLine("END");

