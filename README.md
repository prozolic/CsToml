[CsToml - TOML Parser/Serializer  for .NET](https://prozolic.github.io/CsToml)
===

CsToml is TOML Parser/Serializer for .NET.  
For more information about TOML, visit the official website at [https://toml.io/en/](https://toml.io/en/)

> [!NOTE]
> It is currently in preview. The library name and API may undergo breaking changes.  
> Currently he latest version is CsToml Ver.1.0.6, CsToml.Extensions Ver.1.0.3, and CsToml.Generator Ver.1.0.2.  
> The API specifications have changed significantly in the latest versions of each library.
> The next version will be released as the official version.

```csharp
using CsToml;
using System.Text;

var tomlText = @"
key = ""value""
number = 123
"u8;

var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);

if (document!.TryFind("key"u8, out var value))
{
    var str = value!.GetString(); // value
    Console.WriteLine($"key = {str}"); // key = value
}

Console.WriteLine($"key = {document!.RootNode["key"u8].GetString()}"); // key = value

using var serializedTomlText = CsTomlSerializer.Serialize(document);
Console.WriteLine(Encoding.UTF8.GetString(serializedTomlText.ByteSpan));
// key = "value"
// number = 123

```

Table of Contents
---

* [Feature](#feature)
* [Installation](#installation)
* [Deserialize a toml format string to TomlDocument](#deserialize-a-toml-format-string-to-tomldocument)
* [Serialize TomlDocument](#serialize-tomldocument)
* [Find values from TomlDocument](#find-values-from-tomldocument)
* [Casting a TOML value](#casting-a-toml-value)
* [Serialize and deserialize custom classes](#serialize-and-deserialize-custom-classes)
* [Extensions](#extensions)
* [Third Party Libraries](#third-party-libraries)
* [UnitTest](#unittest)
* [License](#license)

Feature
---

- Compatible with [TOML v1.0.0](https://toml.io/en/v1.0.0) specification.  
- Implemented in .NET 8 and C# 12.(supports .NET 8 or later. )  
- Supports I/O APIs (`IBufferWriter<byte>`, `ReadOnlySpan<byte>`, `ReadOnlySequence<byte>`)  
- By parsing directly in UTF-8 (byte array) instead of UTF-16 (strings), low allocation and high speed are achieved.  

Installation
---

This library is distributed via NuGet.

> PM> Install-Package [CsToml](https://www.nuget.org/packages/CsToml/)

Additional features are available by installing optional documents.(learn more in our [extensions section](#extensions))

> PM> Install-Package [CsToml.Extensions](https://www.nuget.org/packages/CsToml.Extensions/)  
> PM> Install-Package [CsToml.Generator](https://www.nuget.org/packages/CsToml.Generator/)  

Deserialize a toml format string to `TomlDocument`
---

It can deserialize (parse) from a UTF8 string (`byte[], ReadOnlySpan<byte>`) in TOML format.

```csharp
var tomlText = @"
key = ""value""
number = 123
"u8;

var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
```

Call `CsTomlSerializer.Deserialize<TomlDocument>(tomlText)` to deserialize a UTF-8 string (`ReadOnlySpan<byte>` or `ReadOnlySequence<byte>`) in TOML format.
The second argument is `CsTomlSerializerOptions`, which does not need to be specified explicitly at this time.
It may be used to add optional features in the future.  

```csharp
var tomlText = @"
key = ""value""
number = ""Error
"u8;

try
{
    // throw CsTomlException
    var error = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
}
catch(CsTomlSerializeException ctse)
{
    foreach (var cte in ctse.Exceptions)
    {
        // A syntax error (CsTomlException) occurred while parsing line 3 of the TOML file. Check InnerException for details.
        var e = cte.InnerException; // InnerException: 10 is a character that cannot be converted to a number.
    }
}
```

If a syntax error is found during deserialization, an `CsTomlSerializeException` is thrown after deserialization.
The contents of the thrown exception can be viewed at `CsTomlException.InnerException`.

Serialize `TomlDocument`
---

Call `CsTomlSerializer.Serialize` to Serialize `TomlDocument`.
You can return a ByteMemoryResult or get a utf8 byte array via IBufferWriter<byte>.

```csharp
var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);

// The first is obtained by ByteMemoryResult.
using var result = CsTomlSerializer.Serialize(document);
Console.WriteLine(Encoding.UTF8.GetString(result.ByteSpan));

// The second is obtained via IBufferWriter<byte>.
var bufferWriter = new ArrayBufferWriter<byte>();
CsTomlSerializer.Serialize(ref bufferWriter, document);
```

Find values from `TomlDocument`
---

```csharp
var tomlText = @"
key = ""value""
number = 123
"u8;

var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);

var value = document!.Find("key"u8)!.GetString();
Console.WriteLine($"key = {value}"); // key = value

if (document!.TryFind("key"u8, out var value2))
{
    var str = value2!.GetString(); // value
    Console.WriteLine($"key = {str}"); // key = value
}
```

You can get a `TomlValue` by calling `Find`, `TryFind`.
You can call the API defined in `TomlValue` to convert the value to the corresponding type.

```csharp
var tomlText = @"
array = [1, 2, 3]
inlineTable = { key = ""value"", number = 123 }

[table]
key = ""value""

[[arrayOfTables]]
key = ""value""

[[arrayOfTables]]
number = 123
"u8;

var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
// Array
{
    var tomlArray = document!.Find("array"u8);
    foreach (var tomlValue in tomlArray!.GetArray())
    {
        var value = tomlValue!.GetInt64(); // 1, 2, 3
    }
}
// InlineTable
{
    var tomlInlineTable = document!.Find("inlineTable"u8)!.Find("key"u8);
    var tomlInlineTable2 = document!.Find("inlineTable.key"u8, isDottedKeys:true);
   var value = tomlInlineTable!.GetString(); // "value"
}
// Table
{
    var tomlValue = document!.Find("table"u8, "key"u8);
    var value = tomlValue!.GetString(); // "value"
}
// ArrayOfTables
{
    var tomlValue = document!.Find("arrayOfTables"u8, 0, "key"u8);  // "value"
    var value = tomlValue!.GetString(); // "value"
}
```

The same search can be performed on array, tables, arrays of tables, and inline tables.
These APIs have several overload methods that can be used for different purposes, such as retrieving table values or table array values.


```csharp
var tomlText = @"
dotted.keys = ""value""

"u8;

var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
if (document!.TryFind(["dotted"u8, "keys"u8], out var tomlValue))
{
    var value = tomlValue!.GetString(); // "value"
}
if (document!.TryFind("dotted.keys"u8, out var tomlValue2, isDottedKeys: true))
{
    var value2 = tomlValue2!.GetString(); // "value"
}
var value3 = document!.RootNode["dotted"u8]["keys"u8].GetString(); // "value"
```

There are three ways to search with the dotted key.
The first is to search using `ReadOnlySpan<ByteArray>` as the key.
Collection expressions have been available since C#12, making it convenient to create `ReadOnlySpan<ByteArray>` as well as initialize arrays.

The second is to include a dot in the key string.
If the call is made with `isDottedKeys:true` as an argument, the keys are interpreted as dotted keys and searched.
For example, if the key is “A.B”, the table “A” containing the key “B” is searched.

The third is a search using the `TomlDocument.RootNode` property and indexer.
This gives `TomlDocumentNode` instead of `TomlValue`, but the value can be retrieved using the same API as `TomlValue`.
Also, if you have a complex configuration, it is easier to understand and implement using this method.

Casting a TOML value
---

Each TOML value type can be cast as follows.

| TOML Value Type        | Castable types from `TomlValue`                                                                          |
|:-----------------------|:-----------------------------------------------------------------------------------------------------------|
| `String`               | `string`, `long`, `double`, `bool`, `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `INumberBase<T>` |
| `Integer`              | `string`, `long`, `double`, `bool`,`INumberBase<T>`                                                        |
| `Floating`             | `string`, `long`, `double`, `bool`,`INumberBase<T>`                                                        |
| `Boolean`              | `string`, `long`, `double`, `bool`,`INumberBase<T>`                                                        |
| `Offset Date-Time`     | `string`, `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`                                             |
| `Local Date-Time`      | `string`, `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`                                             |
| `Local Date`           | `string`, `DateTime`, `DateTimeOffset`, `DateOnly`                                                         |
| `Local Time`           | `string`, `TimeOnly`                                                                                       |
| `Array`                | `ReadOnlyCollection<TomlValue>`,`T[]`,`IEnumerable<T>`,`IReadOnlyCollection<T>`,`IReadOnlyList<T>`                                                                          |

The following APIs are defined in `TomlValue`.
APIs with the `Get` prefix throw an error on failure and return a value on success.
APIs with the `TryGet` prefix return false on failure and set the value to the argument of the out parameter on success.
`CanGetValue` can be used to see which types can be converted.

```csharp
public partial class TomlValue :
    ISpanFormattable,
    IUtf8SpanFormattable
{
    public bool CanGetValue(TomlValueFeature feature)
    public ReadOnlyCollection<TomlValue> GetArray()
    public TomlValue GetArrayValue(int index)
    public string GetString()
    public long GetInt64()
    public double GetDouble()
    public bool GetBool()
    public DateTime GetDateTime()
    public DateTimeOffset GetDateTimeOffset()
    public DateOnly GetDateOnly()
    public TimeOnly GetTimeOnly()
    public object GetObject()
    public T GetNumber<T>() where T : struct, INumberBase<T>
    public T GetValue<T>()
    public bool TryGetArray(out IReadOnlyList<TomlValue> value)
    public bool TryGetArrayValue(int index, out TomlValue value)
    public bool TryGetString(out string value)
    public bool TryGetInt64(out long value)
    public bool TryGetDouble(out double value)
    public bool TryGetBool(out bool value)
    public bool TryGetDateTime(out DateTime value)
    public bool TryGetDateTimeOffset(out DateTimeOffset value)
    public bool TryGetDateOnly(out DateOnly value)
    public bool TryGetTimeOnly(out TimeOnly value)
    public bool TryGetObject(out object value)
    public bool TryGetNumber<T>(out T value) where T : struct, INumberBase<T>
    public bool TryGetValue<T>(out T value)
    public TomlValue? Find(ReadOnlySpan<byte> keys, bool isDottedKeys = false)
    public TomlValue? Find(ReadOnlySpan<char> keys, bool isDottedKeys = false)
    public TomlValue? Find(ReadOnlySpan<ByteArray> dottedKeys)
    public bool TryFind(ReadOnlySpan<byte> key, out TomlValue? value, bool isDottedKeys = false)
    public bool TryFind(ReadOnlySpan<char> key, out TomlValue? value, bool isDottedKeys = false)
    public bool TryFind(ReadOnlySpan<ByteArray> dottedKeys, out TomlValue? value)
}
```

Serialize and deserialize custom classes
---

`CsToml.Generator` provides the ability to serialize and deserialize your own classes.
Define the class to be serialized and assign the `TomlSerializedObject` and `TomlValueOnSerialized` attribute and the partial keyword.
`TomlValueOnSerialized` can only be given to read-write (they have both a get and a set accessor) properties.

```csharp
[TomlSerializedObject]
public partial class CsTomlClass
{
    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public string Key { get; set; }

    [TomlValueOnSerialized(TomlValueType.KeyValue)]
    public int Number { get; set; }

    [TomlValueOnSerialized(TomlValueType.Array)]
    public int[] Array { get; set; }

    [TomlValueOnSerialized(TomlValueType.Table)]
    public TableClass Table { get; set; } = new TableClass();

    public class TableClass
    {
        [TomlValueOnSerialized(TomlValueType.KeyValue)]
        public string Key { get; set; }

        [TomlValueOnSerialized(TomlValueType.KeyValue)]
        public int Number { get; set; }
    }
}
```

Adding the above attributes will generate code for serialization/deserialization by [Source Generators](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview).
Property names with `TomlValueOnSerialized` are used as keys in the TOML document.

<details><summary>Generated Code</summary>

```csharp
#nullable enable
#pragma warning disable CS0219 // The variable 'variable' is assigned but its value is never used
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8604 // Possible null reference argument for parameter.

using CsToml;
using System.Buffers;

namespace ConsoleApp;

partial class CsTomlClass : ITomlSerializedObject<CsTomlClass>
{
    static void ITomlSerializedObject<CsTomlClass>.Serialize<TBufferWriter, ITomlValueSerializer>(ref TBufferWriter writer, CsTomlClass? target, CsTomlSerializerOptions? options)
    {
        ITomlValueSerializer.SerializeKey(ref writer, "Key");
        ITomlValueSerializer.SerializeEqual(ref writer);
        if (string.IsNullOrEmpty(target.Key))
            ITomlValueSerializer.Serialize(ref writer, Span<char>.Empty);
        else
            ITomlValueSerializer.Serialize(ref writer, target.Key.AsSpan());
        ITomlValueSerializer.SerializeNewLine(ref writer);
        ITomlValueSerializer.SerializeKey(ref writer, "Number");
        ITomlValueSerializer.SerializeEqual(ref writer);
        ITomlValueSerializer.Serialize(ref writer, target.Number);
        ITomlValueSerializer.SerializeNewLine(ref writer);
        ITomlValueSerializer.SerializeKey(ref writer, "Array");
        ITomlValueSerializer.SerializeEqual(ref writer);
        ITomlValueSerializer.Serialize(ref writer, target.Array);
        ITomlValueSerializer.SerializeNewLine(ref writer);
        ITomlValueSerializer.SerializeNewLine(ref writer);
        ITomlValueSerializer.SerializeTableHeader(ref writer, "Table");
        ITomlValueSerializer.SerializeKey(ref writer, "Key");
        ITomlValueSerializer.SerializeEqual(ref writer);
        if (string.IsNullOrEmpty(target.Table.Key))
            ITomlValueSerializer.Serialize(ref writer, Span<char>.Empty);
        else
            ITomlValueSerializer.Serialize(ref writer, target.Table.Key.AsSpan());
        ITomlValueSerializer.SerializeNewLine(ref writer);
        ITomlValueSerializer.SerializeKey(ref writer, "Number");
        ITomlValueSerializer.SerializeEqual(ref writer);
        ITomlValueSerializer.Serialize(ref writer, target.Table.Number);
        ITomlValueSerializer.SerializeNewLine(ref writer);

    }

    static CsTomlClass ITomlSerializedObject<CsTomlClass>.Deserialize<ITomlValueSerializer>(ReadOnlySpan<byte> tomlText, CsTomlSerializerOptions? options)
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText, options);
        var target = new ConsoleApp.CsTomlClass();
        if (document.TryFind("Key"u8, out var _Key))
            target.Key = (String)_Key!.GetString();
        if (document.TryFind("Number"u8, out var _Number))
            target.Number = (Int32)_Number!.GetInt64();
        if (document.TryFind("Array"u8, out var _Array))
            target.Array = _Array!.GetValue<Int32[]>();
        if (document.TryFind(["Table"u8, "Key"u8], out var _Table_Key))
            target.Table.Key = (String)_Table_Key!.GetString();
        if (document.TryFind(["Table"u8, "Number"u8], out var _Table_Number))
            target.Table.Number = (Int32)_Table_Number!.GetInt64();
        return target;

    }

    static CsTomlClass ITomlSerializedObject<CsTomlClass>.Deserialize<ITomlValueSerializer>(in ReadOnlySequence<byte> tomlText, CsTomlSerializerOptions? options)
    {
        // TODO: implemented...
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText, options);
        var target = new ConsoleApp.CsTomlClass();
        if (document.TryFind("Key"u8, out var _Key))
            target.Key = (String)_Key!.GetString();
        if (document.TryFind("Number"u8, out var _Number))
            target.Number = (Int32)_Number!.GetInt64();
        if (document.TryFind("Array"u8, out var _Array))
            target.Array = _Array!.GetValue<Int32[]>();
        if (document.TryFind(["Table"u8, "Key"u8], out var _Table_Key))
            target.Table.Key = (String)_Table_Key!.GetString();
        if (document.TryFind(["Table"u8, "Number"u8], out var _Table_Number))
            target.Table.Number = (Int32)_Table_Number!.GetInt64();
        return target;

    }
}
```

</details>

As a result, it can be serialized and deserialized as follows.

```csharp
var tomlText = @"
Key = ""value""
Number = 123
Array = [1, 2, 3]

[Table]
Key = ""value""
Number = 123
"u8;

var value = CsTomlSerializer.Deserialize<CsTomlClass>(tomlText);
using var serializedText = CsTomlSerializer.Serialize(ref value);

// Key = "value"
// Number = 123
// Array = [1, 2, 3]
//
// [Table]
// Key = "value"
// Number = 123
var serializedTomlText = Encoding.UTF8.GetString(serializedText.ByteSpan);
```

These types can be serialized/deserialize by default
* .NET Built-in types(`bool`, `byte`, `long`, `double`, etc)
* `string`, `object`
* `T[]`, `IEnumerator<>`, `IReadOnlyCollection<>`, `IReadOnlyList<>`

Extensions
---

`CsToml.Extensions` provides an API for reading and writing TOML file.

```csharp
// deserialize from TOML File
var document = CsTomlFileSerializer.Deserialize<TomlDocument>("test.toml");
var document2 = await CsTomlFileSerializer.DeserializeAsync<TomlDocument>("test.toml");

// seserialize To TOML File
CsTomlFileSerializer.Serialize("test.toml", document);
await CsTomlFileSerializer.SerializeAsync("test.toml", document);
```

`CsTomlFileSerializer.Deserialize` and `CsTomlFileSerializer.DeserializeAsync` deserialize UTF8 strings in TOML files into `TomlDocument`.
`CsTomlFileSerializer.Serialize` and `CsTomlFileSerializer.SerializeAsync` serialize the UTF8 string of `TomlDocument` to the TOML file.

Third Party Libraries
---

`CsToml.Extensions` references [Cysharp/NativeMemoryArray](https://github.com/Cysharp/NativeMemoryArray) as a dependent library.  

UnitTest
---

Please note that we are using the TOML files located in the [‘tests/’ directory of the ‘toml-test repository (MIT License)’](https://github.com/toml-lang/toml-test/tree/master/tests) for some of our unit tests.

License
---

MIT License. Some code is implemented based on [dotnet/runtime](https://github.com/dotnet/runtime), Please check the original license.
