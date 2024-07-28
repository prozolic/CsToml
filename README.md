# [CsToml - TOML Serializer  for .NET](https://prozolic.github.io/CsToml)

CsToml is TOML Serializer for .NET.  
For more information about TOML, visit the official website at [https://toml.io/en/](https://toml.io/en/)

> [!NOTE]
> It is currently in preview. The library name and API may undergo breaking changes.  
> Currently he latest version is CsToml Ver.1.0.5, CsToml.Extensions Ver.1.0.2, and CsToml.Generator Ver.1.0.1.  
> The API specification has changed significantly in Ver.1.0.5. Some of them are no longer compatible with previous versions.  
> CsToml Ver.1.0.4 and earlier versions have been deprecated.  
> CsToml.Extensions Ver.1.0.1 and earlier versions have been deprecated.  

```csharp
using CsToml;
using System.Text;

var tomlText = @"
key = ""value""
number = 123
"u8;

var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);

if (package!.TryFind("key"u8, out var value))
{
    var str = value!.GetString(); // value
    Console.WriteLine($"key = {str}"); // key = value
}

Console.WriteLine($"key = {package!.RootNode["key"u8].GetString()}"); // key = value

using var serializedTomlText = CsTomlSerializer.Serialize(package);
Console.WriteLine(Encoding.UTF8.GetString(serializedTomlText.ByteSpan));
// key = "value"
// number = 123

```

## Feature

- Compatible with [TOML v1.0.0](https://toml.io/en/v1.0.0) specification.  
- Implemented in .NET 8 and C# 12.(supports .NET 8 or later. )  
- Supports I/O APIs (`IBufferWriter<byte>`, `ReadOnlySpan<byte>`, `ReadOnlySequence<byte>`)  
- By parsing directly in UTF-8 (byte array) instead of UTF-16 (strings), low allocation and high speed are achieved.  

## Installation

This library is distributed via NuGet.

> PM> Install-Package [CsToml](https://www.nuget.org/packages/CsToml/)

Additional features are available by installing optional packages.(learn more in our [extensions section](#extensions))

> PM> Install-Package [CsToml.Extensions](https://www.nuget.org/packages/CsToml.Extensions/)  
> PM> Install-Package [CsToml.Generator](https://www.nuget.org/packages/CsToml.Generator/)  

## Deserialize a toml format string

Deserialize from UTF-8 string to `CsTomlPackage`.

```csharp
var tomlText = @"
key = ""value""
number = 123
"u8;

var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);
```

Call `CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText)` to deserialize a UTF-8 string (`ReadOnlySpan<byte>` or `ReadOnlySequence<byte>`) in TOML format.
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
    var errorPackage = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);
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

## Find a value from key

`CsTomlPackage` provides APIs to search for values.

```csharp
var tomlText = @"
key = ""value""
number = 123
"u8;

var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);

var value = package!.Find("key"u8)!.GetString();
Console.WriteLine($"key = {value}"); // key = value

if (package!.TryFind("key"u8, out var value2))
{
    var str = value2!.GetString(); // value
    Console.WriteLine($"key = {str}"); // key = value
}
```

You can get a `CsTomlValue` by calling `Find`, `TryFind`.
You can call the API defined in `CsTomlValue` to convert the value to the corresponding type.

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

var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);
// Array
{
    var tomlArray = package!.Find("array"u8);
    foreach (var tomlValue in tomlArray!.GetArray())
    {
        var value = tomlValue!.GetInt64(); // 1, 2, 3
    }
}
// InlineTable
{
    var tomlInlineTable = package!.Find("inlineTable"u8)!.Find("key"u8);
    var tomlInlineTable2 = package!.Find("inlineTable.key"u8, isDottedKeys:true);
   var value = tomlInlineTable!.GetString(); // "value"
}
// Table
{
    var tomlValue = package!.Find("table"u8, "key"u8);
    var value = tomlValue!.GetString(); // "value"
}
// ArrayOfTables
{
    var tomlValue = package!.Find("arrayOfTables"u8, 0, "key"u8);  // "value"
    var value = tomlValue!.GetString(); // "value"
}
```

The same search can be performed on array, tables, arrays of tables, and inline tables.

```csharp
var tomlText = @"
dotted.keys = ""value""

"u8;

var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);
if (package!.TryFind(["dotted"u8, "keys"u8], out var tomlValue))
{
    var value = tomlValue!.GetString(); // "value"
}
if (package!.TryFind("dotted.keys"u8, out var tomlValue2, isDottedKeys: true))
{
    var value2 = tomlValue2!.GetString(); // "value"
}
var value3 = package!.RootNode["dotted"u8]["keys"u8].GetString(); // "value"
```

There are three ways to search with the dotted key.
The first is to search using `ReadOnlySpan<ByteArray>` as the key.
Collection expressions have been available since C#12, making it convenient to create `ReadOnlySpan<ByteArray>` as well as initialize arrays.

The second is to include a dot in the key string.
If the call is made with `isDottedKeys:true` as an argument, the keys are interpreted as dotted keys and searched.
For example, if the key is “A.B”, the table “A” containing the key “B” is searched.

The third is a search using the `CsTomlPackage.RootNode` property and indexer.
This gives `CsTomlPackageNode` instead of `CsTomlValue`, but the value can be retrieved using the same API as `CsTomlValue`.

### Casting a TOML value

Each TOML value type can be cast as follows.

| TOML Value Type        | Castable types from `CsTomlValue`                                                                          |
|:-----------------------|:-----------------------------------------------------------------------------------------------------------|
| `String`               | `string`, `long`, `double`, `bool`, `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `INumberBase<T>` |
| `Integer`              | `string`, `long`, `double`, `bool`,`INumberBase<T>`                                                        |
| `Floating`             | `string`, `long`, `double`, `bool`,`INumberBase<T>`                                                        |
| `Boolean`              | `string`, `long`, `double`, `bool`,`INumberBase<T>`                                                        |
| `Offset Date-Time`     | `string`, `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`                                             |
| `Local Date-Time`      | `string`, `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`                                             |
| `Local Date`           | `string`, `DateTime`, `DateTimeOffset`, `DateOnly`                                                         |
| `Local Time`           | `string`, `TimeOnly`                                                                                       |
| `Array`                | `ReadOnlyCollection<CsTomlValue>`                                                                          |

The following APIs are defined in `CsTomlValue`.

```csharp
public partial class CsTomlValue
{
    public virtual bool CanGetValue(CsTomlValueFeature feature)
    public ReadOnlyCollection<CsTomlValue> GetArray()
    public CsTomlValue GetArrayValue(int index)
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
    public bool TryGetArray(out IReadOnlyList<CsTomlValue> value)
    public bool TryGetArrayValue(int index, out CsTomlValue value)
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
    public CsTomlValue? Find(ReadOnlySpan<byte> keys, bool isDottedKeys = false)
    public CsTomlValue? Find(ReadOnlySpan<char> keys, bool isDottedKeys = false)
    public CsTomlValue? Find(ReadOnlySpan<ByteArray> dottedKeys)
    public bool TryFind(ReadOnlySpan<byte> key, out CsTomlValue? value, bool isDottedKeys = false)
    public bool TryFind(ReadOnlySpan<char> key, out CsTomlValue? value, bool isDottedKeys = false)
    public bool TryFind(ReadOnlySpan<ByteArray> dottedKeys, out CsTomlValue? value)
}
```

APIs with the `Get` prefix throw an error on failure and return a value on success.
APIs with the `TryGet` prefix return false on failure and set the value to the argument of the out parameter on success.
`CanGetValue` can be used to see which types can be converted.

## Serialize to toml format string

Serialize a class to which `CsTomlPackage`

```csharp
var tomlText = @"
key = ""value""
number = 123
"u8;

var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);
using var result = CsTomlSerializer.Serialize(package);
Console.WriteLine(Encoding.UTF8.GetString(result.ByteSpan));
// key = "value"
// number = 123

var buffer = new ArrayBufferWriter<byte>();
CsTomlSerializer.Serialize(ref buffer, package);
Console.WriteLine(Encoding.UTF8.GetString(buffer.WrittenSpan));
// key = "value"
// number = 123
```

To serialize a `CsTomlPackage`, call `CsTomlSerializer.Serialize(package)`.
The serialized UTF8 string is returned as `ByteMemoryResult` or `IBufferWriter<byte>`.
`ByteMemoryResult` is only valid until Dispose is called.

## Serialize and deserialize objects

`CsToml.Generator` provides the ability to serialize and deserialize your own classes.

```csharp
var tomlText = @"
Key = ""value""
Number = 123
Array = [1, 2, 3]

[Table]
Key = ""value""
Number = 123
"u8;

var value = CsTomlSerializer.DeserializeToPackagePart<CsTomlClass>(tomlText);
using var serializedText = CsTomlSerializer.SerializeFromPackagePart(ref value);

// Key = "value"
// Number = 123
// Array = [1, 2, 3]
//
// [Table]
// Key = "value"
// Number = 123
var serializedTomlText = Encoding.UTF8.GetString(serializedText.ByteSpan);
```

By default, you must add the `[CsTomlPackagePart]` attribute for serializable types and the `[CsTomlValueOnSerialized]` attribute for properties.
Adding the above attributes will generate code for serialization/deserialization by Source Generators.

```csharp
[CsTomlPackagePart]
public partial class CsTomlClass
{
    [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
    public string Key { get; set; }

    [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
    public int Number { get; set; }

    [CsTomlValueOnSerialized(CsTomlValueType.Array)]
    public int[] Array { get; set; }

    [CsTomlValueOnSerialized(CsTomlValueType.Table)]
    public TableClass Table { get; set; } = new TableClass();

    public class TableClass
    {
        [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
        public string Key { get; set; }

        [CsTomlValueOnSerialized(CsTomlValueType.KeyValue)]
        public int Number { get; set; }
    }
}
```

<details><summary>Generated Code</summary>

```csharp
using CsToml;

namespace ConsoleApp;

partial class CsTomlClass : ICsTomlPackagePart<CsTomlClass>
{
    static void ICsTomlPackagePart<CsTomlClass>.Serialize<TBufferWriter, ICsTomlValueSerializer>(ref TBufferWriter writer, ref CsTomlClass target, CsTomlSerializerOptions? options)
    {
        ICsTomlValueSerializer.SerializeKey(ref writer, "Key");
        ICsTomlValueSerializer.SerializeEqual(ref writer);
        if (string.IsNullOrEmpty(target.Key))
            ICsTomlValueSerializer.Serialize(ref writer, Span<char>.Empty);
        else
            ICsTomlValueSerializer.Serialize(ref writer, target.Key.AsSpan());
        ICsTomlValueSerializer.SerializeNewLine(ref writer);
        ICsTomlValueSerializer.SerializeKey(ref writer, "Number");
        ICsTomlValueSerializer.SerializeEqual(ref writer);
        ICsTomlValueSerializer.Serialize(ref writer, target.Number);
        ICsTomlValueSerializer.SerializeNewLine(ref writer);
        ICsTomlValueSerializer.SerializeKey(ref writer, "Array");
        ICsTomlValueSerializer.SerializeEqual(ref writer);
        ICsTomlValueSerializer.Serialize(ref writer, target.Array);
        ICsTomlValueSerializer.SerializeNewLine(ref writer);
        ICsTomlValueSerializer.SerializeNewLine(ref writer);
        ICsTomlValueSerializer.SerializeTableHeader(ref writer, "Table");
        ICsTomlValueSerializer.SerializeKey(ref writer, "Key");
        ICsTomlValueSerializer.SerializeEqual(ref writer);
        if (string.IsNullOrEmpty(target.Table.Key))
            ICsTomlValueSerializer.Serialize(ref writer, Span<char>.Empty);
        else
            ICsTomlValueSerializer.Serialize(ref writer, target.Table.Key.AsSpan());
        ICsTomlValueSerializer.SerializeNewLine(ref writer);
        ICsTomlValueSerializer.SerializeKey(ref writer, "Number");
        ICsTomlValueSerializer.SerializeEqual(ref writer);
        ICsTomlValueSerializer.Serialize(ref writer, target.Table.Number);
        ICsTomlValueSerializer.SerializeNewLine(ref writer);

    }

    static CsTomlClass ICsTomlPackagePart<CsTomlClass>.Deserialize<ICsTomlValueSerializer>(ReadOnlySpan<byte> tomlText, CsTomlSerializerOptions? options)
    {
        var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText, options);
        var target = new ConsoleApp.CsTomlClass();
        if (package.TryFind("Key"u8, out var _Key))
            target.Key = (String)_Key!.GetString();
        if (package.TryFind("Number"u8, out var _Number))
            target.Number = (Int32)_Number!.GetInt64();
        if (package.TryFind("Array"u8, out var _Array))
            target.Array = _Array!.GetValue<Int32[]>();
        if (package.TryFind(["Table"u8, "Key"u8], out var _Table_Key))
            target.Table.Key = (String)_Table_Key!.GetString();
        if (package.TryFind(["Table"u8, "Number"u8], out var _Table_Number))
            target.Table.Number = (Int32)_Table_Number!.GetInt64();
        return target;

    }
}
```

</details>

## Extensions

`CsToml.Extensions` provides an API for reading and writing TOML file.

```csharp
// deserialize from TOML File
var package = CsTomlFileSerializer.Deserialize<CsTomlPackage>("test.toml");
var package2 = await CsTomlFileSerializer.DeserializeAsync<CsTomlPackage>("test.toml");

// seserialize To TOML File
CsTomlFileSerializer.Serialize("test.toml", package);
await CsTomlFileSerializer.SerializeAsync("test.toml", package);
```

`CsTomlFileSerializer.Deserialize` and `CsTomlFileSerializer.DeserializeAsync` deserialize UTF8 strings in TOML files into `CsTomlPackage`.
`CsTomlFileSerializer.Serialize` and `CsTomlFileSerializer.SerializeAsync` serialize the UTF8 string of `CsTomlPackage` to the TOML file.

### Third Party Libraries

`CsToml.Extensions` references [Cysharp/NativeMemoryArray](https://github.com/Cysharp/NativeMemoryArray) as a dependent library.  

## UnitTest

Please note that we are using the TOML files located in the [‘tests/’ directory of the ‘toml-test repository (MIT License)’](https://github.com/toml-lang/toml-test/tree/master/tests) for some of our unit tests.

## License

MIT License. Some code is implemented based on [dotnet/runtime](https://github.com/dotnet/runtime), Please check the original license.
