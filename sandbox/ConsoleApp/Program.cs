
using ConsoleApp;
using CsToml;
using CsToml.Error;
using CsToml.Extensions;
using CsToml.Values;
using System.Buffers;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;

Console.WriteLine("Hello, World!");

using (var stream = new FileStream("./../../../Toml/test.toml", FileMode.Open))
{
    var streamPackage = await CsTomlStreamingSerializer.DeserializeAsync<CsTomlPackage>(stream);
}

using (var stream = new FileStream("./../../../Toml/test_withoutBOM.toml", FileMode.Open))
{
    var streamPackage = await CsTomlStreamingSerializer.DeserializeAsync<CsTomlPackage>(PipeReader.Create(stream));
}

var packageAsync = await CsTomlFileSerializer.DeserializeAsync<CsTomlPackage>("./../../../Toml/test.toml");
var package = CsTomlFileSerializer.Deserialize<TestPackage>("./../../../Toml/test.toml");

using var serializedPackageTomlText = CsTomlSerializer.Serialize(package);
var packageText = Encoding.UTF8.GetString(serializedPackageTomlText.ByteSpan);
var package2 = CsTomlSerializer.Deserialize<CsTomlPackage>(serializedPackageTomlText.ByteSpan);

{
    var valuekeyValue = package!.RootNode["bare_key"u8].GetString();
    var valuekeyValue2 = package!
        .RootNode["fruit"u8]["apple"u8]["color"u8]
        .GetString();
    var valueKeyValue3 = package!.RootNode["array"u8]["of"u8]["tables"u8][1]["arr"u8].GetArrayValue(1).GetString();
    var valueKeyValue4 = package!.RootNode["array"]["of"]["tables"][2]["name"u8]["last"u8].GetString();
    var valueKeyValue5 = package!.RootNode["contributors"u8][1]["name"u8].GetString();
    var valueKeyValue6 = package!
        .RootNode["array"u8]["of"u8]["tables"u8][2]["name"u8]["array"u8][1]["z"u8]
        .GetArrayValue(0).GetInt64();

    if (package!.TryFind("array.of.tables", out var _arrayOfTableValue, isDottedKeys:true))
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
    var value2 = package!.Find("int1"u8);
    var value3 = package!.Find("int6"u8);
    var value4 = package!.Find("hex1"u8);
    var value5 = package!.Find("flt1"u8);
    var value6 = package!.Find("bool1"u8);
    var value7 = package!.Find("odt1"u8);
    var value8 = package!.Find("ldt1"u8);
    var value9 = package!.Find("lt1"u8);
    var value10 = package!.Find("integers"u8)?.GetArrayValue(0);

    value10 = package.RootNode["integers"u8].GetArrayValue(0);
    var value11 = package!.Find("nested_mixed_array"u8)?.GetArrayValue(0)?.GetArrayValue(1);
    var _ = package!.Find("integers"u8)?.TryGetArray(out var value12);
    var value13 = package!.Find("Name\\tJos\\u00E9");

    var value14 = package!.RootNode["products"u8][0]["name"u8];
    //var value15 = package!.RootNode["products"u8][3]["name"u8].GetString();
}
{
    var value = package!.Find("table-1"u8, "key1"u8);
    var value2 = package!.TryFind("table-1", "key1", out var ___);
}
{
    var value = package!.Find("fruit.apple.texture"u8, "smooth"u8, isTableHeaderAsDottedKeys:true, isDottedKeys: true);
    var value2 = package!.Find(["fruit"u8, "apple"u8, "texture"u8], "name"u8);
    var value3 = package!.TryFind("fruit.apple.texture"u8, "smooth"u8, out var ____, isDottedKeys:true);
    var value4 = package!.TryFind(["fruit"u8, "apple"u8, "texture"u8], "smooth"u8, out var _____);
}
{
    var value = package!.Find("products"u8, 0, "name"u8);
    var value2 = package!.Find("products", 0, "name");
    var value3 = package!.Find("products"u8, 1, "name"u8);
    var value4 = package!.Find("array.of.tables"u8, 2, "name.array"u8, true, true);
    var arr = value4!.GetArrayValue(1);
    var a = arr.Find("z"u8)!.GetArrayValue(0).GetInt64();
    var value5 = package!.Find("array.of.tables"u8, 2, "name.last2"u8, true, true);
    var value6 = value5!.Find("key"u8)!.GetString();
    var value7 = value5!.Find("key"u8)!.GetObject();
    var value8 = value5!.Find("key"u8)!.GetValue<string>();
    var value9 = package!.Find("integers"u8)!;
    var arr2 = value9.GetValue<IEnumerable<string>>();
    var value10 = package!.Find("int1"u8);
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
    var testCsTomlpackage = CsTomlSerializer.Deserialize<TestPackage>(tomlText);
}
catch(CsTomlSerializeException ctse)
{
    foreach (var cte in ctse.Exceptions)
    {
        // A syntax error (CsTomlException) occurred while parsing line 3 of the TOML file. Check InnerException for details.
        var e = cte.InnerException;
    }
}


//if (testCsTomlpackage!.TryGetValue("key", out var value22))
//{
//    var str = value22!.GetString();
//    Console.WriteLine($"key = {str}"); // key = value
//}

//using var serializedTomlText = CsTomlSerializer.Serialize(testCsTomlpackage);
//Console.WriteLine(Encoding.UTF8.GetString(serializedTomlText.ByteSpan));

using var result = CsTomlSerializer.Serialize(package);
Console.WriteLine(Encoding.UTF8.GetString(result.ByteSpan));

var buffer = new ArrayBufferWriter<byte>();
CsTomlSerializer.Serialize(ref buffer, package);

var part = new TestPackagePart();
using var partBytes = CsTomlSerializer.Serialize(ref part);
var partPackage = CsTomlSerializer.Deserialize<CsTomlPackage>(partBytes.ByteSpan);

if (partPackage.TryFind(["TableValue"u8, "あいうえお"u8], out var vvvvv))
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
IntArray = [9,8,7,6,5,4,3,2,1,0]

"u8.ToArray();

var partfasdfasdf2 = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText2);
using var partBytes2 = CsTomlSerializer.Serialize(partfasdfasdf2);

var part2 = CsTomlSerializer.DeserializeToPackagePart<TestPackagePart>(tomlText2);


Console.WriteLine("END");

