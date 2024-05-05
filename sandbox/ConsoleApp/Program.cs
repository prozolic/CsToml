﻿
using ConsoleApp;
using CsToml;
using CsToml.Error;
using System.Buffers;
using System.Text;

Console.WriteLine("Hello, World!");


var package = CsTomlSerializer.DeserializeFromFile<CsTomlPackage>("./../../../Toml/test.toml");

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
    var value11 = package!.Find("nested_mixed_array"u8)?.GetArrayValue(0)?.GetArrayValue(1);
    var _ = package!.Find("integers"u8)?.TryGetArray(out var value12);
    var value13 = package!.Find("Name\\tJos\\u00E9");

    var value14 = package!["products"u8][0]["name"u8];
}
{
    var value = package!.Find("table-1"u8, "key1"u8);
    var value2 = package!.TryGetValue("table-1", "key1", out var ___);
}
{
    var value = package!.Find("fruit.apple.texture"u8, "smooth"u8, CsTomlPackageOptions.DottedKeys);
    var value2 = package!.Find(["fruit"u8, "apple"u8, "texture"u8], "smooth"u8, CsTomlPackageOptions.DottedKeys);
    var value3 = package!.TryGetValue("fruit.apple.texture"u8, "smooth"u8, out var ____, CsTomlPackageOptions.DottedKeys);
    var value4 = package!.TryGetValue(["fruit"u8, "apple"u8, "texture"u8], "smooth"u8, out var _____, CsTomlPackageOptions.DottedKeys);
}
{
    var value = package!.Find("products"u8, 0, "name"u8);
    var value2 = package!.Find("products", 0, "name");
    var value3 = package!.Find("products"u8, 1, "name"u8);
}

var tomlText = @"
key = ""value""
number = 123
"u8;

var testpackage = CsTomlSerializer.Deserialize<TestPackage>(tomlText, CsTomlSerializerOptions.NoThrow);
var testCsTomlpackage = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText, CsTomlSerializerOptions.NoThrow);

if (package.Exceptions.Count > 0)
{
    foreach (CsTomlException? e in package.Exceptions)
    {
        // check error
    }
}

if (testCsTomlpackage!.TryGetValue("key", out var value22))
{
    var str = value22!.GetString();
    Console.WriteLine($"key = {str}"); // key = value
}

var serializedTomlText = CsTomlSerializer.Serialize(testCsTomlpackage);
Console.WriteLine(Encoding.UTF8.GetString(serializedTomlText.ByteSpan));

var result = CsTomlSerializer.Serialize(package);
Console.WriteLine(Encoding.UTF8.GetString(result.ByteSpan));

var buffer = new ArrayBufferWriter<byte>();
CsTomlSerializer.Serialize(ref buffer, package);

var part = new TestPackagePart();
using var partBytes = CsTomlSerializer.Serialize(ref part);
var partPackage = CsTomlSerializer.Deserialize<CsTomlPackage>(partBytes.ByteSpan);

Console.WriteLine("END");

