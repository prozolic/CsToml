[CsToml - TOML Parser/Serializer  for .NET](https://prozolic.github.io/CsToml)
===

[![MIT License](https://img.shields.io/github/license/prozolic/CsToml)](LICENSE)
[![CsToml](https://img.shields.io/nuget/v/CsToml?label=nuget%20CsToml)](https://www.nuget.org/packages/CsToml)
[![CsToml.Extensions](https://img.shields.io/nuget/v/CsToml.Extensions?label=nuget%20CsToml.Extensions
)](https://www.nuget.org/packages/CsToml.Extensions)
[![CsToml.Generator](https://img.shields.io/nuget/v/CsToml.Generator?label=nuget%20CsToml.Generator
)](https://www.nuget.org/packages/CsToml.Generator/)

CsToml is Fast and low memory allocation TOML Parser/Serializer for .NET.  
For more information about TOML, visit the official website at [https://toml.io/en/](https://toml.io/en/)

> [!NOTE]
> The official release version is v1.1.0 or higher. Less than v1.1.0 is deprecated due to incompatible APIs.

![80 values with table and array of tables](https://github.com/user-attachments/assets/2f38c653-28ce-43e5-a615-9fa61637525d)

> This benchmark parses a string (string) into a TOML object. I used [Tommy](https://github.com/dezhidki/Tommy), [Tomlet](https://github.com/SamboyCoding/Tomlet) and [Tomlyn](https://github.com/xoofx/Tomlyn) for comparison. `CsToml` includes additional `UTF8.GetBytes` calls. This benchmark code is [sandbox/Benchmark](https://github.com/prozolic/CsToml/blob/main/sandbox/Benchmark/ParseBenchmark.cs).

![Deserialize TestTomlSerializedObject (9 values with table and array of tables)](https://github.com/user-attachments/assets/10ff18d6-3209-43c9-87e2-37c91e987733)
![Serialize TestTomlSerializedObject (9 values with table and array of tables)](https://github.com/user-attachments/assets/374780e9-ec12-4f53-a390-cabe1085aa65)

> This benchmark convert custom class to string and string to custom class. I used [Tomlet](https://github.com/SamboyCoding/Tomlet) and [Tomlyn](https://github.com/xoofx/Tomlyn) for comparison. `CsToml` includes additional `UTF8.GetBytes` calls. This benchmark code is [sandbox/Benchmark Deserialization](https://github.com/prozolic/CsToml/blob/main/sandbox/Benchmark/ClassDeserializationBenchmark.cs), [sandbox/Benchmark Serialization](https://github.com/prozolic/CsToml/blob/main/sandbox/Benchmark/ClassSerializationBenchmark.cs).

CsToml has the following features.

- It complies with [TOML v1.0.0](https://toml.io/en/v1.0.0).
- .NET 8 or later supported.
- Parsing is performed using byte sequence instead of `string`.
- It is processed byte sequence directly by the API defined in `System.Buffers`(`IBufferWriter<byte>`,`ReadOnlySequence<byte>`), memory allocation is small and fast.
- Buffers are rented from the pool(`ArrayPool<T>`), reducing the allocation.
- CsToml deserializer has been tested using [the standard TOML v1.0.0 test cases](https://github.com/toml-lang/toml-test/tree/master/tests) and all have passed.
- The serialization interface and implementation is influenced by [MemoryPack](https://github.com/Cysharp/MemoryPack) and [VYaml](https://github.com/hadashiA/VYaml).

Table of Contents
---

* [Installation](#installation)
* [Serialize and deserialize TOML sequence (Parse TOML sequence)](#serialize-and-deserialize-toml-sequence-parse-toml-sequence)
* [Serialize and deserialize custom class/struct/record/record struct (CsToml.Generator)](#serialize-and-deserialize-custom-classstructrecordrecord-struct-cstomlgenerator)
* [Built-in support type](#built-in-support-type)
* [Deserialize API](#deserialize-api)
* [Serialize API](#serialize-api)
* [Serialize and deserialize TOML values only](#serialize-and-deserialize-toml-values-only)
* [TomlDocument class](#tomldocument-class)
* [Extensions (CsToml.Extensions)](#extensions-cstomlextensions)
* [UnitTest](#unittest)
* [License](#license)

Installation
---

This library is distributed via NuGet. We target .NET 8 and .NET 9.  

> PM> Install-Package [CsToml](https://www.nuget.org/packages/CsToml/)

When you install `CsToml.Generator`, it automatically creates code to make your custom classes serializable.(learn more in our [Serialize and deserialize custom classes (CsToml.Generator)](#serialize-and-deserialize-custom-classes-cstomlgenerator)). It is basically recommended to install it together with CsToml.  
However, this requires Roslyn 4.3.1 (Visual Studio 2022 version 17.3) or higher.

> PM> Install-Package [CsToml.Generator](https://www.nuget.org/packages/CsToml.Generator/)  

Additional features are available by installing optional documents.(learn more in our [Extensions (CsToml.Extensions)](#extensions-cstomlextensions))

> PM> Install-Package [CsToml.Extensions](https://www.nuget.org/packages/CsToml.Extensions/)  

Serialize and deserialize TOML sequence (Parse TOML sequence)
---

`CsTomlSerializer.Deserialize<T>` method supports `ReadOnlySpan<byte>`, `ReadOnlySequence<byte>` and `Stream`.
`CsTomlSerializer.Serialize<T>` method supports a return type of `ByteMemoryResult` as well as it can serialize to `IBufferWriter<byte>` or `Stream`.

```csharp
var tomlText = @"
key = ""value""
number = 123
"u8;

var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
string key = document.RootNode["key"u8].GetString();
long number = document.RootNode["number"u8].GetInt64();

using var result = CsTomlSerializer.Serialize(document);
var utf8Span = result.ByteSpan;
var utf8Memory = result.ByteMemory;
```

You can deserialize/serialize it by using `TomlDocument` class, while still preserving the TOML data structure.  

```csharp
var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
using var result = CsTomlSerializer.Serialize(document);
```

Serialize and deserialize custom `class`/`struct`/`record`/`record struct` (`CsToml.Generator`)
---

Define the `class`, `struct`, `record` and `record struct` to be serialized and assign the `[TomlSerializedObject]` Attribute and the partial keyword.  

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
See [Built-in support type](#built-in-support-type) for more information on available property types.

<details><summary>Generated Code(CsTomlClass_generated.g.cs)</summary>

```csharp
// <auto-generated> This .cs file is generated by CsToml.Generator. </auto-generated>
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
        var __Key__RootNode = rootNode["Key"u8];
        var __Key__ = options.Resolver.GetFormatter<string>()!.Deserialize(ref __Key__RootNode, options);
        var __Value__RootNode = rootNode["alias"u8];
        var __Value__ = options.Resolver.GetFormatter<string>()!.Deserialize(ref __Value__RootNode, options);
        var __Array__RootNode = rootNode["Array"u8];
        var __Array__ = options.Resolver.GetFormatter<int[]>()!.Deserialize(ref __Array__RootNode, options);
        var __Number__RootNode = rootNode["Number"u8];
        var __Number__ = options.Resolver.GetFormatter<int?>()!.Deserialize(ref __Number__RootNode, options);
        var __Table__RootNode = rootNode["Table"u8];
        var __Table__ = options.Resolver.GetFormatter<global::ConsoleApp.TableClass>()!.Deserialize(ref __Table__RootNode, options);

        var target = new CsTomlClass(){
            Key = __Key__,
            Value = __Value__,
            Array = __Array__,
            Number = __Number__,
            Table = __Table__,
        };

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
        else
        {
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
// <auto-generated> This .cs file is generated by CsToml.Generator. </auto-generated>
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
        var __Key__RootNode = rootNode["Key"u8];
        var __Key__ = options.Resolver.GetFormatter<string>()!.Deserialize(ref __Key__RootNode, options);
        var __Number__RootNode = rootNode["Number"u8];
        var __Number__ = options.Resolver.GetFormatter<int>()!.Deserialize(ref __Number__RootNode, options);

        var target = new TableClass(){
            Key = __Key__,
            Number = __Number__,
        };

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

### Properties

`[TomlValueOnSerialized]`serializes public instance properties.
Property names with `[TomlValueOnSerialized]` are used as keys in the TOML document.

```csharp
[TomlSerializedObject]
public partial class CsTomlClass
{
    [TomlValueOnSerialized]
    public string Key { get; set; }
}
```

This is serialized as follows:

```csharp
Key = "value"
```

The key name can also be changed with `[TomlValueOnSerialized(aliasName)]`.  

```csharp
[TomlSerializedObject]
public partial class CsTomlClass
{
    [TomlValueOnSerialized(aliasName: "alias")]
    public string Key { get; set; }
}
```

This is serialized as follows:

```csharp
alias = "value"
```

In `CsToml.Generator` v1.3.0 and later versions, read-only properties, or those that have no setter either private or public, can also be deserialized.


```csharp
[TomlSerializedObject]
internal partial class TypeTable(long intValue, string strValue)
{
    [TomlValueOnSerialized]
    public long IntValue { get; } = intValue;

    [TomlValueOnSerialized]
    public string StrValue { get; private set; } = strValue;
}
```

### Constructors

`CsToml.Generator` supports both parameterized and non-parameterized constructors. The choice of constructor is subject to the following rules.

* Non-public constructors are ignored.
* If there is no explicit constructor, use parameterless.
* If there is one parameterless/parameterized constructor, use it.
* If there is more than one constructor, the parameterized constructor with the most matching parameters is automatically selected.
* The condition for a parameterized constructor is that all parameter names must match the corresponding member names (case-insensitive).


```csharp
[TomlSerializedObject]
internal partial class Constructor(string str) // Parameterized constructors are available.
{
    [TomlValueOnSerialized]
    public string Str { get; set; } = str;
}

[TomlSerializedObject]
internal partial class Constructor2
{
    public Constructor2(string str, long intValue)
    {
        Str = str;
        IntValue = intValue;
    }

    // Use this parameterized constructor.
    public Constructor2(bool booleanValue, string str, double floatValue, long intValue)
    {
        Str = str;
        IntValue = intValue;
        FloatValue = floatValue;
        BooleanValue = booleanValue;
    }

    [TomlValueOnSerialized]
    public string Str { get; set; }

    [TomlValueOnSerialized]
    public long IntValue { get; set; }

    [TomlValueOnSerialized]
    public double FloatValue { get; set; }

    [TomlValueOnSerialized]
    public bool BooleanValue { get; set; }
}

[TomlSerializedObject]
internal partial class Constructor3
{
    // Use this parameterized constructor.
    public Constructor3(string str, long intValue)
    {
        Str = str;
        IntValue = intValue;
    }

    [TomlValueOnSerialized]
    public string Str { get; }

    [TomlValueOnSerialized]
    public long IntValue { get;}
}
```


### `CsTomlSerializerOptions.TableStyle`

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
* `OrderedDictionary<>`, `ReadOnlySet<>`
* `ImmutableArray<>`, `ImmutableList<>`, `ImmutableStack<>`, `ImmutableQueue<>`, `ImmutableHashSet<>`, `ImmutableSortedSet<>`, `ImmutableDictionary<>`, `ImmutableSortedDictionary<>`

Deserialize API
---

Deserialize has `ReadOnlySpan<byte>`, `ReadOnlySequence<byte>` and `Stream`.
The second argument is `CsTomlSerializerOptions`, which need not be specified explicitly in `CsTomlSerializer.Deserialize`.
It may be used to add optional features in the future.  

```csharp
T Deserialize<T>(ReadOnlySpan<byte> tomlText, CsTomlSerializerOptions? options = null)
T Deserialize<T>(ReadOnlySequence<byte> tomlSequence, CsTomlSerializerOptions? options = null)
T Deserialize<T>(Stream stream, CsTomlSerializerOptions? options = null)
DeserializeAsync<T>(Stream stream, CsTomlSerializerOptions? options = null, bool configureAwait = false, CancellationToken cancellationToken = default)
```

Asynchronous API is available `CsTomlSerializer.DeserializeAsync`.
this only supports `Stream`.

```csharp
var ms = new MemoryStream(tomlText); // FileStream is also OK.
var document = await CsTomlSerializer.DeserializeAsync<TomlDocument>(ms);
```

You can deserialize it by using `TomlDocument` class, while still preserving the TOML data structure.  

```csharp
var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
```

You can also deserialize into primitive object type or collection of keys and values, in addition to custom classes with the `[TomlSerializedObject]` Attribute.

```csharp
// dynamic is the same as IDictionary<object, object>
var dict = CsTomlSerializer.Deserialize<dynamic>(tomlText);
var dict2 = CsTomlSerializer.Deserialize<Dictionary<object, object>>(tomlText);
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

Serialize API
---

`Serialize` has three overloads, including synchronous and asynchronous APIs.

```csharp
ByteMemoryResult Serialize<T>(T target, CsTomlSerializerOptions? options = null)
void Serialize<TBufferWriter, T>(ref TBufferWriter bufferWriter, T target, CsTomlSerializerOptions? options = null)
void Serialize<T>(Stream stream, T value, CsTomlSerializerOptions? options = null)
async ValueTask SerializeAsync<T>(Stream stream, T value, CsTomlSerializerOptions? options = null, bool configureAwait = false, CancellationToken cancellationToken = default)
```

`IBufferWriter<byte>` serializes directly into the buffer.
If `Span<byte>` or `Memory<byte>` is required, API that returns a `ByteMemoryResult` can be used.

```csharp
var bufferWriter = new ArrayBufferWriter<byte>();
CsTomlSerializer.Serialize(ref bufferWriter, document);

// The first is obtained by ByteMemoryResult.
using var result = CsTomlSerializer.Serialize(document);
Console.WriteLine(Encoding.UTF8.GetString(result.ByteSpan));
```

Asynchronous API is available `CsTomlSerializer.SerializeAsync`.
this only supports `Stream`, the same as deserialization.

```csharp
var ms2 = new MemoryStream(65536);
await CsTomlSerializer.SerializeAsync(ms2, document);
Console.WriteLine(Encoding.UTF8.GetString(ms2.ToArray()));
```

Serialize and deserialize TOML values only
---

`CsTomlSerializer.DeserializeValueType<T>` deserialize TOML values to a specified type.
`CsTomlSerializer.SerializeValueType` serializes the value into TOML format.  
The object can be used with the type listed in [Built-in support type](#built-in-support-type).

```csharp
T DeserializeValueType<T>(ReadOnlySpan<byte> tomlText, CsTomlSerializerOptions? options = null)
T DeserializeValueType<T>(ReadOnlySequence<byte> tomlSequence, CsTomlSerializerOptions? options = null)
ByteMemoryResult SerializeValueType<T>(T target, CsTomlSerializerOptions? options = null)
void SerializeValueType<TBufferWriter, T>(ref TBufferWriter bufferWriter, T target, CsTomlSerializerOptions? options = null)
```

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

`TomlDocument` class
---

Use `TomlDocument` when parsing with the TOML structure preserved.
You can get the TOML value by specifying a key from the `RootNode` property of this class (`TomlDocumentNode` struct).  
Like `Dictionary` class, it is searched by the indexer.
Parameters can use `ReadOnlySpan<byte>`, `ReadOnlySpan<char>` and `int`.
The best performance is achieved with `ReadOnlySpan<byte>`.
`ReadOnlySpan<char>` is a little slower because the process of conversion from UTF16 to UTF8 is performed.

```csharp
public TomlDocumentNode this[ReadOnlySpan<char> key]
public TomlDocumentNode this[ReadOnlySpan<byte> key]
public TomlDocumentNode this[int index]
```

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

To retrieve primitive values from `omlDocumentNode` and `TomlValue`, call the following API.
If an API with the `Get` prefix fails, an `CsTomlException` is thrown and a value is returned if it was successful.
If an API with the `TryGet` prefix fails, false is returned and the value is set to the argument of the out parameter if it was successful.
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

`ToDictionary` method is available as an API to convert from `TomlDocument` to `IDictionary<TKey, TValue>`.

```csharp
var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);

// Same as document.RootNode.GetValue<Dictionary<object, object>>().
var dict = document.ToDictionary<object, object>();
```

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

Please note that we are using the TOML files located in the ['tests/' directory of the ‘toml-test repository (MIT License)’](https://github.com/toml-lang/toml-test/tree/master/tests) for some of our unit tests.

License
---

MIT License. Some code is implemented based on [dotnet/runtime](https://github.com/dotnet/runtime), Please check the original license.
