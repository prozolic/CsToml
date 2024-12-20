[CsToml - TOML Parser/Serializer  for .NET](https://prozolic.github.io/CsToml)
===

[![MIT License](https://img.shields.io/github/license/prozolic/CsToml)](LICENSE)
[![CsToml](https://img.shields.io/nuget/v/CsToml?label=nuget%20CsToml)](https://www.nuget.org/packages/CsToml)
[![CsToml.Extensions](https://img.shields.io/nuget/v/CsToml.Extensions?label=nuget%20CsToml.Extensions
)](https://www.nuget.org/packages/CsToml.Extensions)
[![CsToml.Generator](https://img.shields.io/nuget/v/CsToml.Generator?label=nuget%20CsToml.Generator
)](https://www.nuget.org/packages/CsToml.Generator/)

CsToml is TOML Parser/Serializer for .NET.  
For more information about TOML, visit the official website at [https://toml.io/en/](https://toml.io/en/)

> [!NOTE]
> The official release version is v1.1.0 or higher.  
> Less than v1.1.0 is deprecated due to incompatible APIs.

CsToml has the following features.

- It complies with [TOML v1.0.0](https://toml.io/en/v1.0.0).
- .NET 8 or later supported.
- CsToml is implemented in .NET 8 (C# 12).
- Parsing is performed using byte sequence instead of `string`.
- It is processed byte sequence directly by the API defined in `System.Buffers`(`IBufferWriter<byte>`,`ReadOnlySequence<byte>`), memory allocation is small and fast.
- Buffers are rented from the pool(`ArrayPool<T>`), reducing the allocation.
- CsToml deserializer has been tested using [the standard TOML v1.0.0 test cases](https://github.com/toml-lang/toml-test/tree/master/tests) and all have passed.
- The serialization interface and implementation is influenced by [MemoryPack](https://github.com/Cysharp/MemoryPack) and [VYaml](https://github.com/hadashiA/VYaml).

Table of Contents
---

* [Installation](#installation)
* [Serialize and deserialize TomlDocument](#serialize-and-deserialize-tomldocument)
* [Find values from TomlDocument](#find-values-from-tomldocument)
* [Serialize and deserialize custom classes (CsToml.Generator)](#serialize-and-deserialize-custom-classes-cstomlgenerator)
* [Serialize and deserialize TOML values only](#serialize-and-deserialize-toml-values-only)
* [Built-in support type](#built-in-support-type)
* [Extensions (CsToml.Extensions)](#extensions-cstomlextensions)
* [UnitTest](#unittest)
* [License](#license)

Installation
---

This library is distributed via NuGet. We target .NET 8 or later.  

> PM> Install-Package [CsToml](https://www.nuget.org/packages/CsToml/)

When you install `CsToml.Generator`, it automatically creates code to make your custom classes serializable.(learn more in our [Serialize and deserialize custom classes (CsToml.Generator)](#serialize-and-deserialize-custom-classes-cstomlgenerator)). It is basically recommended to install it together with CsToml.  
However, this requires Roslyn 4.3.1 (Visual Studio 2022 version 17.3) or higher.

> PM> Install-Package [CsToml.Generator](https://www.nuget.org/packages/CsToml.Generator/)  

Additional features are available by installing optional documents.(learn more in our [Extensions (CsToml.Extensions)](#extensions-cstomlextensions))

> PM> Install-Package [CsToml.Extensions](https://www.nuget.org/packages/CsToml.Extensions/)  

Serialize and deserialize `TomlDocument`
---

Call `CsTomlSerializer.Deserialize<TomlDocument>(tomlText)` to deserialize a UTF-8 string (`ReadOnlySpan<byte>` or `ReadOnlySequence<byte>`) in TOML format.
By using `TomlDocument` for the type parameter, serialization/deserialization can be performed while preserving the TOML data structure.
The second argument is `CsTomlSerializerOptions`, which need not be specified explicitly in `CsTomlSerializer.Deserialize`.
It may be used to add optional features in the future.  

```csharp
var tomlText = @"
key = ""value""
number = 123
"u8;

var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);

// The first is obtained by ByteMemoryResult.
using var result = CsTomlSerializer.Serialize(document);
Console.WriteLine(Encoding.UTF8.GetString(result.ByteSpan));
```

Call `CsTomlSerializer.Serialize` to serialize `TomlDocument`.
You can return a `ByteMemoryResult` or get a utf8 byte array via `IBufferWriter<byte>`.

```csharp
// The first is obtained by ByteMemoryResult.
using var result = CsTomlSerializer.Serialize(document);
Console.WriteLine(Encoding.UTF8.GetString(result.ByteSpan));

// The second is obtained via IBufferWriter<byte>.
var bufferWriter = new ArrayBufferWriter<byte>();
CsTomlSerializer.Serialize(ref bufferWriter, document);
```

Synchronous and asynchronous APIs for serialization/deserialization from Stream to TomlDocument are available.  
So, for example, if you specify `FileStream`, you can read and write files.

```csharp
var tomlText = @"
key = ""value""
number = 123
"u8.ToArray();

var ms = new MemoryStream(tomlText); // FileStream is also OK.
var document = await CsTomlSerializer.DeserializeAsync<TomlDocument>(ms);

var ms2 = new MemoryStream(65536);
await CsTomlSerializer.SerializeAsync(ms2, document);
Console.WriteLine(Encoding.UTF8.GetString(ms2.ToArray()));
```

If a syntax error is found during deserialization, an `CsTomlSerializeException` is thrown after deserialization.
The contents of the thrown exception can be viewed at `CsTomlException.InnerException`.

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

Find values from `TomlDocument`
---

It can be obtained via indexers (`[ReadOnlySpan<char>]`,`[ReadOnlySpan<byte>]`,`[int index]`) from `TomlDocument.RootNode` property.

```csharp
var tomlText = @"
key = 123
dotted.keys = ""value""
array = [1, ""2"", 3]
inlineTable = { key = ""value2"", number = 123 }
configurations = [1, {key = [ { key2 = [""VALUE""]}]}]

[table]
key = ""value3""

[[arrayOfTables]]
key = ""value4""

[[arrayOfTables]]
number = 123
"u8;

var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);

var key = document!.RootNode["key"u8].GetInt64();   // 123
var dottedKeys = document!.RootNode["dotted"u8]["keys"u8].GetString();  // "value"
var array = document!.RootNode["array"u8].GetArray();   // [1, 2, 3]
var item1 = array[0].GetInt64();    // 1
var item2 = array[1].GetString();   // "2"
var item3 = array[2].GetInt64();   // 3
// Same as "array[0].GetInt64()"
var item1_2 = document!.RootNode["array"u8].GetArrayValue(0).GetInt64();
var inlineTable = document!.RootNode["inlineTable"u8]["key"u8].GetString();  // "value2"
var configurations = document!.RootNode["configurations"u8][1]["key"u8][0]["key2"u8][0].GetString(); // "VALUE"

var table = document!.RootNode["table"u8]["key"u8].GetString();  // "value3"
var arrayOfTables = document!.RootNode["arrayOfTables"u8][0]["key"u8].GetString();  // "value4"
var arrayOfTables2 = document!.RootNode["arrayOfTables"u8][1]["number"u8].GetString();  // 123

var tuple = document!.RootNode["array"u8].GetValue<Tuple<long, string, long>>(); // Tuple<long, string, long>(1, "2", 3)
```

`TomlValue` and `TomlDocumentNode` have APIs for accessing and casting values.
APIs with the `Get` prefix throw an `CsTomlException` on failure and return a value on success.
APIs with the `TryGet` prefix return false on failure and set the value to the argument of the out parameter on success.
`CanGetValue` can be used to see which Toml value types can be converted.
`GetValue<T>` and `TryGetValue<T>` can be used to obtain a value converted from a Toml value type to a specified type.

```csharp
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
```

Serialize and deserialize custom classes (`CsToml.Generator`)
---

Define the class to be serialized and assign the `[TomlSerializedObject]` and `[TomlValueOnSerialized]` attribute and the partial keyword.
`[TomlValueOnSerialized]` can only be given to read-write (they have both a get and a set accessor) properties.

```csharp
[TomlSerializedObject]
public partial class CsTomlClass
{
    [TomlValueOnSerialized]
    public string Key { get; set; }

    [TomlValueOnSerialized]
    public int? Number { get; set; }

    [TomlValueOnSerialized]
    public int[] Array { get; set; }

    [TomlValueOnSerialized(aliasName: "alias")]
    public string Value { get; set; }

    [TomlValueOnSerialized]
    public TableClass Table { get; set; } = new TableClass();
}

[TomlSerializedObject]
public partial class TableClass
{
    [TomlValueOnSerialized]
    public string Key { get; set; }

    [TomlValueOnSerialized]
    public int Number { get; set; }
}
```

Adding the above attributes will generate code for serialization/deserialization by [Source Generators](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview).
Property names with `[TomlValueOnSerialized]` are used as keys in the TOML document.
The key name can also be changed with `[TomlValueOnSerialized(aliasName)]`.  
See [Built-in support type](#built-in-support-type) for more information on available property types.

<details><summary>Generated Code(CsTomlClass_generated.g.cs)</summary>

```csharp
#nullable enable
#pragma warning disable CS0219 // The variable 'variable' is assigned but its value is never used
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8604 // Possible null reference argument for parameter.
#pragma warning disable CS8619 // Possible null reference assignment fix

using CsToml;
using CsToml.Formatter;
using CsToml.Formatter.Resolver;

namespace ConsoleApp;

partial class CsTomlClass : ITomlSerializedObject<CsTomlClass>
{

    static CsTomlClass ITomlSerializedObject<CsTomlClass>.Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        var target = new CsTomlClass();
        var __Key__RootNode = rootNode["Key"u8];
        target.Key = options.Resolver.GetFormatter<string>()!.Deserialize(ref __Key__RootNode, options);
        var __Value__RootNode = rootNode["alias"u8];
        target.Value = options.Resolver.GetFormatter<string>()!.Deserialize(ref __Value__RootNode, options);
        var __Array__RootNode = rootNode["Array"u8];
        target.Array = options.Resolver.GetFormatter<int[]>()!.Deserialize(ref __Array__RootNode, options);
        var __Number__RootNode = rootNode["Number"u8];
        target.Number = options.Resolver.GetFormatter<int?>()!.Deserialize(ref __Number__RootNode, options);
        var __Table__RootNode = rootNode["Table"u8];
        target.Table = options.Resolver.GetFormatter<global::ConsoleApp.TableClass>()!.Deserialize(ref __Table__RootNode, options);
        return target;

    }

    static void ITomlSerializedObject<CsTomlClass>.Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, CsTomlClass target, CsTomlSerializerOptions options)
    {
        writer.WriteKey("Key"u8);
        writer.WriteEqual();
        options.Resolver.GetFormatter<String>()!.Serialize(ref writer, target.Key, options);
        writer.EndKeyValue();
        writer.WriteKey("alias"u8);
        writer.WriteEqual();
        options.Resolver.GetFormatter<String>()!.Serialize(ref writer, target.Value, options);
        writer.EndKeyValue();
        writer.WriteKey("Array"u8);
        writer.WriteEqual();
        options.Resolver.GetFormatter<int[]>()!.Serialize(ref writer, target.Array, options);
        writer.EndKeyValue();
        writer.WriteKey("Number"u8);
        writer.WriteEqual();
        options.Resolver.GetFormatter<int?>()!.Serialize(ref writer, target.Number, options);
        writer.EndKeyValue();
        if (options.SerializeOptions.TableStyle == TomlTableStyle.Header && (writer.State == TomlValueState.Default || writer.State == TomlValueState.Table)){
            writer.WriteTableHeader("Table"u8);
            writer.WriteNewLine();
            writer.BeginCurrentState(TomlValueState.Table);
            writer.PushKey("Table"u8);
            options.Resolver.GetFormatter<TableClass>()!.Serialize(ref writer, target.Table, options);
            writer.PopKey();
            writer.EndCurrentState();
        }
        else{
            writer.PushKey("Table"u8);
            options.Resolver.GetFormatter<TableClass>()!.Serialize(ref writer, target.Table, options);
            writer.PopKey();
        }

    }

    static void ITomlSerializedObjectRegister.Register()
    {
        TomlSerializedObjectFormatterResolver.Register(new TomlSerializedObjectFormatter<CsTomlClass>());
    }
}
```

</details>

<details><summary>Generated Code(TableClass_generated.g.cs)</summary>

```csharp
#nullable enable
#pragma warning disable CS0219 // The variable 'variable' is assigned but its value is never used
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8604 // Possible null reference argument for parameter.
#pragma warning disable CS8619 // Possible null reference assignment fix

using CsToml;
using CsToml.Formatter;
using CsToml.Formatter.Resolver;

namespace ConsoleApp;

partial class TableClass : ITomlSerializedObject<TableClass>
{

    static TableClass ITomlSerializedObject<TableClass>.Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        var target = new TableClass();
        var __Key__RootNode = rootNode["Key"u8];
        target.Key = options.Resolver.GetFormatter<string>()!.Deserialize(ref __Key__RootNode, options);
        var __Number__RootNode = rootNode["Number"u8];
        target.Number = options.Resolver.GetFormatter<int>()!.Deserialize(ref __Number__RootNode, options);
        return target;

    }

    static void ITomlSerializedObject<TableClass>.Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TableClass target, CsTomlSerializerOptions options)
    {
        writer.WriteKey("Key"u8);
        writer.WriteEqual();
        options.Resolver.GetFormatter<String>()!.Serialize(ref writer, target.Key, options);
        writer.EndKeyValue();
        writer.WriteKey("Number"u8);
        writer.WriteEqual();
        options.Resolver.GetFormatter<Int32>()!.Serialize(ref writer, target.Number, options);
        writer.EndKeyValue();

    }

    static void ITomlSerializedObjectRegister.Register()
    {
        TomlSerializedObjectFormatterResolver.Register(new TomlSerializedObjectFormatter<TableClass>());
    }
}
```

</details>

As a result, it can be serialized and deserialized as follows.
Custom class serialization does not preserve the layout of the original TOML text.

```csharp
var tomlText = @"
Key = ""value""
Number = 123
Array = [1, 2, 3]
alias = ""alias""

[Table]
Key = ""value""
Number = 123
"u8;

var value = CsTomlSerializer.Deserialize<CsTomlClass>(tomlText);
using var serializedText = CsTomlSerializer.Serialize(value);

// Key = "value"
// alias = "alias"
// Array = [ 1, 2, 3 ]
// Number = 123
// Table.Key = "value"
// Table.Number = 123
var serializedTomlText = Encoding.UTF8.GetString(serializedText.ByteSpan);
```

It can also serialize to TOML table format by setting `CsTomlSerializerOptions.TableStyle` to `TomlTableStyle.Header`.
You can create custom `CsTomlSerializerOptions` using `CsTomlSerializerOptions.Default` and a with expression.


```csharp
// You can create custom options by using a with expression.
var option = CsTomlSerializerOptions.Default with
{
    SerializeOptions = new SerializeOptions { TableStyle = TomlTableStyle.Header }
};

var value = new CsTomlClass() { 
    Key = "value", Number = 123, Array = [1,2,3] , Value = "alias",
    Table = new TableClass() { Key = "kEY", Number = 123 } 
};
using var serializedText = CsTomlSerializer.Serialize<CsTomlClass>(value, option);

// Key = "value"
// alias = "alias"
// Array = [ 1, 2, 3 ]
// Number = 123
// [Table]
// Key = "kEY"
// Number = 123
var serializedTomlText = Encoding.UTF8.GetString(serializedText.ByteSpan);
```

Serialize and deserialize TOML values only
---

`CsTomlSerializer.DeserializeValueType<T>` deserialize TOML values to a specified type.
`CsTomlSerializer.SerializeValueType` serializes the value into TOML format.  
The object can be used with the type listed in [Built-in support type](#built-in-support-type).

```csharp
var tomlIntValue = CsTomlSerializer.DeserializeValueType<long>("1234"u8);
var tomlStringValue = CsTomlSerializer.DeserializeValueType<string>("\"\\U00000061\\U00000062\\U00000063\""u8); // abc
var tomlDateTimeOffsetValue = CsTomlSerializer.DeserializeValueType<DateTimeOffset>("2024-10-20T15:16:00"u8);
var tomlArrayValue = CsTomlSerializer.DeserializeValueType<string[]>("[ \"red\", \"yellow\", \"green\" ]"u8);
var tomlinlineTableValue = CsTomlSerializer.DeserializeValueType<IDictionary<string, object>>("{ x = 1, y = 2, z = \"3\" }"u8);
var tomlTupleValue = CsTomlSerializer.DeserializeValueType<Tuple<string,string,string>>("[ \"red\", \"yellow\", \"green\" ]"u8);

using var serializedTomlValue1 = CsTomlSerializer.SerializeValueType(tomlIntValue);
// 1234
using var serializedTomlValue2 = CsTomlSerializer.SerializeValueType(tomlStringValue);
// "abc"
using var serializedTomlValue3 = CsTomlSerializer.SerializeValueType(tomlDateTimeValue);
// 2024-10-20T15:16:00
using var serializedTomlValue4 = CsTomlSerializer.SerializeValueType(tomlArrayValue);
// [ "red", "yellow", "green" ]
using var serializedTomlValue5 = CsTomlSerializer.SerializeValueType(tomlinlineTableValue);
// {x = 1, y = 2, z = "3"}
using var serializedTomlValue6 = CsTomlSerializer.SerializeValueType(tomlTupleValue);
// [ "red", "yellow", "green" ]
```

Built-in support type
---

These types can be serialized/deserialized by default as properties of custom classes.

* .NET Built-in types(`bool`, `long`, `double`, `string` etc)
* `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan`
* `Enum`, `Half`, `Int128`, `UInt128`, `BigInteger`, `BitArray`
* `Uri`, `Version`, `Guid`, `Type`, `Nullable`, `StringBuilder`
* `T[]`, `Memory<>`, `ReadOnlyMemory<>`
* `List<>`, `Stack<>`, `HashSet<>`, `SortedSet<>`, `Queue<>`, `PriorityQueue<,>`, `LinkedList<>`, `ReadOnlyCollection<>`, `BlockingCollection<>`, `SortedList<>`
* `ConcurrentQueue<>`, `ConcurrentStack<>`, `ConcurrentBag<>`, `ConcurrentDictionary<>`
* `IEnumerable<>`, `ICollection<>`, `IReadOnlyCollection<>`, `IList<>`, `IReadOnlyList<>`, `ISet<>`, `IReadOnlySet<>`
* `Dictionary<>`, `ReadOnlyDictionary<>`, `SortedDictionary<>`, `IDictionary<>`, `IReadOnlyDictionary<>`
* `ArrayList`
* `KeyValuePair<>`, `Tuple<,...>`, `ValueTuple<,...>`

Extensions (`CsToml.Extensions`)
---

`CsToml.Extensions` provides APIs to serialize and deserialize Toml files on disk.

```csharp
// deserialize from TOML File
var document = CsTomlFileSerializer.Deserialize<TomlDocument>("test.toml");
var document2 = await CsTomlFileSerializer.DeserializeAsync<TomlDocument>("test.toml");

// serialize To TOML File
CsTomlFileSerializer.Serialize("test.toml", document);
await CsTomlFileSerializer.SerializeAsync("test.toml", document);
```

`CsTomlFileSerializer.Deserialize` and `CsTomlFileSerializer.DeserializeAsync` deserialize UTF8 strings in TOML files into `TomlDocument`.
`CsTomlFileSerializer.Serialize` and `CsTomlFileSerializer.SerializeAsync` serialize the UTF8 string of `TomlDocument` to the TOML file.  

`CsToml.Extensions` uses [Cysharp/NativeMemoryArray](https://github.com/Cysharp/NativeMemoryArray) as a third party library.

UnitTest
---

Please note that we are using the TOML files located in the [‘tests/’ directory of the ‘toml-test repository (MIT License)’](https://github.com/toml-lang/toml-test/tree/master/tests) for some of our unit tests.

License
---

MIT License. Some code is implemented based on [dotnet/runtime](https://github.com/dotnet/runtime), Please check the original license.
