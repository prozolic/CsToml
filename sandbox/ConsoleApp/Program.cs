
using ConsoleApp;
using CsToml;
using CsToml.Error;
using CsToml.Extensions;
using System.Buffers;
using System.Text;


Console.WriteLine("Hello, World!");

var testDocument = await CsTomlFileSerializer.DeserializeAsync<TomlDocument>("./../../../Toml/test_withoutBOM.toml");
var testDocument_lf = await CsTomlFileSerializer.DeserializeAsync<TomlDocument>("./../../../Toml/test_withoutBOM_LF.toml");
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

CsTomlFileSerializer.Serialize("serialzedTest.toml", testDocument);
var testDocument2 = CsTomlFileSerializer.Deserialize<TomlDocument>("serialzedTest.toml");

{
    var testpartDeserialized = CsTomlFileSerializer.Deserialize<TestTomlSerializedObject2>("serialzedTest.toml");
    using var testpartSerialized = CsTomlSerializer.Serialize(testpartDeserialized);

    var testdef = new TestTomlSerializedObject2();
    var testpartDeserialized2 = CsTomlSerializer.Deserialize<TestTomlSerializedObject2>(testpartSerialized.ByteSpan);
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

    if (document.RootNode["array"u8]["of"u8]["tables"][2]["name"]["last"].TryGetString(out var str))
    {

    }
}
{
    var value = document.RootNode["integers"u8].GetArrayValue(0);
    var value2 = document!.RootNode["nested_mixed_array"u8][0][1].GetInt64();
    var _ = document!.RootNode["integers"u8].TryGetArray(out var value12);
    var valu3 = document!.RootNode["Name\\tJos\\u00E9"].GetTomlValue();

    var valu4 = document!.RootNode["products"u8][0]["name"u8];
    var valu5 = document!.RootNode["fruit"u8]["apple"u8]["texture"u8]["smooth"u8].GetBool();
    var valu6 = document!.RootNode["products"u8][0]["name"u8].GetString();
    var valu7 = document!.RootNode["products"u8][2]["name"u8].GetString();
    var valu8 = document!.RootNode["array"u8]["of"u8]["tables"u8][2]["name"u8]["array"u8].GetArray();
}

var tomlText = @"
d = 1985-06-18 17:04:07+12:61
flt7 = 6.626e-34
str = ""string""
int = 99
flt = 1.0
bool = true
odt = 1979-05-27T07:32:00Z
ldt = 1979-05-27T07:32:00
ld1 = 1979-05-27
array = [ 1, 2, 3]
test = {key2=1979-05-27T07:32:00Z}

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

using var result = CsTomlSerializer.Serialize(document);
Console.WriteLine(Encoding.UTF8.GetString(result.ByteSpan));

var buffer = new ArrayBufferWriter<byte>();
CsTomlSerializer.Serialize(ref buffer, document);

var part = new TestTomlSerializedObject();
using var partBytes = CsTomlSerializer.Serialize(part);
var partDocument = CsTomlSerializer.Deserialize<TomlDocument>(partBytes.ByteSpan);

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

var testTomlSerializedObject = CsTomlSerializer.Deserialize<TestTomlSerializedObject>(tomlText2);

var testTomlSerializedObject2 = new TestTomlSerializedObject();
testTomlSerializedObject2.intArr[0] = 9999;
using var testTomlSerializedObject2SerializedText = CsTomlSerializer.Serialize(testTomlSerializedObject2);
var testTomlSerializedObject2_2 = CsTomlSerializer.Deserialize<TestTomlSerializedObject>(testTomlSerializedObject2SerializedText.ByteSpan);

var tomlTextEnum = @"
Value = ""1""
"u8.ToArray();

var enumObject = CsTomlSerializer.Deserialize<EnumObject>(tomlTextEnum);
var enumObjectText = CsTomlSerializer.Serialize<EnumObject>(enumObject);

void Test()
{
    var tomlText = @"
Value = 128
Str = ""this is Str""
Doubl = 123.455
odt1 = 1979-05-27T07:32:00Z
array = [1,1,3,4]
array2 = [[1,2,3,4], [4,5,6,7]]
array3 = [100,200,300,400]
test128 = ""17.234""
guid = ""2D5139DB-CBA6-4641-BB50-04038A47C3ED""
uri = ""https://learn.microsoft.com/""
uri2 = ""https://learn.microsoft.com/2""
tuple = [1979-05-27T07:32:00Z, 123, 456, ""test"",""TEST"", true, 0.11]
KeyValuePair = [1979-05-27T07:32:00Z, ""test""]

[Dict]
key.a.b.v = ""value""
abc = [1, {key = [1, [1, {test = 123, TEST2 = [67, ""test"", 1.01 ]} ] , 5, 1979-05-27T07:32:00Z], key2 = {key3 = 0.1, key4 = 123}}, 23]
ab = {key = 23}

[Dict2]
a.a = 123
Key.test = ""value""

[[Dict3.b]]
a = 34534

[[Dict3.b]]
a = 4564

"u8.ToArray();
    var testClass = CsTomlSerializer.Deserialize<TestClass>(tomlText);

    var buffer = new ArrayBufferWriter<byte>();
    CsTomlSerializer.Serialize(ref buffer, testClass, CsTomlSerializerOptions.Default);

    var test = Encoding.UTF8.GetString(buffer.WrittenSpan);

    var testClass2 = CsTomlSerializer.Deserialize<TestClass>(buffer.WrittenSpan);
    var testDocument = CsTomlSerializer.Deserialize<TomlDocument>(buffer.WrittenSpan);

    var _ = testDocument.RootNode["Dict2"u8]["a"u8]["a"u8].GetInt64();
    var __ = testDocument.RootNode["Dict3"u8]["b"u8][1]["a"u8].GetInt64();
    var ___ = testDocument.RootNode["guid"].GetValue<Guid>();

    var dict = testDocument.RootNode["Dict"u8]["abc"u8].GetArray();

}
Test();

void Sample()
{
    var tomlText = @"
Key = ""value""
Number = 123
Array = [1, 2, 3]
alias = ""alias""

[Table]
Key = ""value""
Number = 123
"u8;

    var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
    var value = CsTomlSerializer.Deserialize<CsTomlClass>(tomlText);
    using var serializedText = CsTomlSerializer.Serialize(value);

    // Key = "value"
    // Number = 123
    // Array = [1, 2, 3]
    // alias = ""alias""
    //
    // [Table]
    // Key = "value"
    // Number = 123
    var serializedTomlText = Encoding.UTF8.GetString(serializedText.ByteSpan);
}

Sample();

Console.WriteLine("END");

