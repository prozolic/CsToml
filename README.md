# CsToml - TOML Serializer  for .NET

CsToml is TOML Serializer for .NET.  
For more information about TOML, visit the official website at [https://toml.io/en/](https://toml.io/en/)

> [!NOTE]
> It is currently in preview. The library name and API may undergo breaking changes.  
> Currently the latest version is CsToml Ver.1.0.4.
> The API specification has changed significantly in Ver.1.0.4. Some of them are no longer compatible with previous versions.

```csharp
using CsToml;
using System.Text;

var tomlText = @"
key = ""value""
number = 123
"u8;

var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);

if (package!.TryGetValue("key"u8, out var value))
{
    var str = value!.GetString(); // value
    Console.WriteLine($"key = {str}"); // key = value
}

Console.WriteLine($"key = {package!.RootNode["key"u8].GetString()}"); // key = value

var serializedTomlText = CsTomlSerializer.Serialize(package);
Console.WriteLine(Encoding.UTF8.GetString(serializedTomlText));
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

## Deserialize a toml format string or file

Deserializing from UTF-8 strings and TOML file paths to a `CsTomlPackage` is possible.  

```csharp
var tomlText = @"
key = ""value""
number = 123
"u8;

var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);
```

The basic API involve  `CsTomlPackage? package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText)`.
When parsing from UTF-8 string(`ReadOnlySpan<byte>` or `ReadOnlySequence<byte>`) in TOML format, you can use `Deserialize`.
`Deserialize` can be specified with `CsTomlSerializerOptions` as a parameter, but it is not necessary to specify it explicitly at this time. It may be used to add optional features in the future.

```csharp
public partial class CsTomlSerializer
{
    public static TPackage? Deserialize<TPackage>(ReadOnlySpan<byte> tomlText, CsTomlSerializerOptions? options = null)
    public static TPackage? Deserialize<TPackage>(in ReadOnlySequence<byte> tomlTextSequence,   CsTomlSerializerOptions? options = null)
}
```

If a syntax error is found during `Deserialize` execution, an exception is thrown after parsing.
To supplement the exception, specify `CsTomlSerializeException` in the `catch` block.
The contents of the thrown exception can be viewed from `CsTomlException.InnerException`.

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

## Serialize to toml format string

It is possible to serialize from `CsTomlPackage` to UTF-8 strings.

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

The basic API involve `using ByteMemoryResult result = CsTomlSerializer.Serialize(package)`, `CsTomlSerializer.Serialize(ref buffer, package);`.
The `ByteMemoryResult` returned from `Serialize(package)` is only valid until Dispose is called.
It is also possible to get it directly via `IBufferWriter<byte>`.

```csharp
public partial class CsTomlSerializer
{
    public static ByteMemoryResult Serialize<TPackagePart>(ref TPackagePart target)
    public static void Serialize<TBufferWriter, TPackagePart>(ref TBufferWriter bufferWriter, ref TPackagePart  target)
    public static ByteMemoryResult Serialize<TPackage>(TPackage? package)
    public static void Serialize<TBufferWriter, TPackage>(ref TBufferWriter bufferWriter, TPackage? package)
}
```

## How to get the value

### Searching for a toml value from a key

CsToml converts deserialized TOML format values to `CsTomlValue`.
`CsTomlValue` can be obtained by calling `TryGetValue` and `Find`, the APIs of `CsTomlPackage`, with the key.

```csharp
public partial class CsTomlPackage
{
    public bool TryGetValue(ReadOnlySpan<byte> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
    public bool TryGetValue(ReadOnlySpan<ByteArray> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
    public bool TryGetValue(ReadOnlySpan<byte> tableHeaderKey, ReadOnlySpan<byte> key, out CsTomlValue? value,  CsTomlPackageOptions? options = default)
    public bool TryGetValue(ReadOnlySpan<ByteArray> tableHeader, ReadOnlySpan<byte> key, out CsTomlValue? value,    CsTomlPackageOptions? options = default)
    public bool TryGetValue(ReadOnlySpan<char> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
    public bool TryGetValue(ReadOnlySpan<char> tableHeader, ReadOnlySpan<char> key, out CsTomlValue? value, CsTomlPackageOptions?   options = default)
    public bool TryGetValue(ReadOnlySpan<byte> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<byte> key, out CsTomlValue? value,  CsTomlPackageOptions? options = default)
    public bool TryGetValue(ReadOnlySpan<byte> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<ByteArray> key, out CsTomlValue?    value, CsTomlPackageOptions? options = default)
    public bool TryGetValue(ReadOnlySpan<ByteArray> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<byte> key, out CsTomlValue?    value, CsTomlPackageOptions? options = default)
    public bool TryGetValue(ReadOnlySpan<ByteArray> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<ByteArray> key, out    CsTomlValue? value, CsTomlPackageOptions? options = default)

    public CsTomlValue? Find(ReadOnlySpan<byte> keys, CsTomlPackageOptions? options = default)
    public CsTomlValue? Find(ReadOnlySpan<char> keys, CsTomlPackageOptions? options = default)
    public CsTomlValue? Find(ReadOnlySpan<ByteArray> keys, CsTomlPackageOptions? options = default)
    public CsTomlValue? Find(ReadOnlySpan<byte> tableHeader, ReadOnlySpan<byte> key, CsTomlPackageOptions? options = default)
    public CsTomlValue? Find(ReadOnlySpan<char> tableHeader, ReadOnlySpan<char> key, CsTomlPackageOptions? options = default)
    public CsTomlValue? Find(ReadOnlySpan<ByteArray> tableHeader, ReadOnlySpan<byte> key, CsTomlPackageOptions? options = default)
    public CsTomlValue? Find(ReadOnlySpan<byte> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<byte> key, CsTomlPackageOptions?   options = default)
    public CsTomlValue? Find(ReadOnlySpan<byte> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<ByteArray> key,    CsTomlPackageOptions? options = default)
    public CsTomlValue? Find(ReadOnlySpan<ByteArray> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<byte> key,    CsTomlPackageOptions? options = default)
    public CsTomlValue? Find(ReadOnlySpan<ByteArray> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<ByteArray> key,   CsTomlPackageOptions? options = default)
    public CsTomlValue? Find(ReadOnlySpan<char> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<char> key, CsTomlPackageOptions? options = default)
}
```

There are three ways to search with the dot key.
The first is to search using `ReadOnlySpan<ByteArray>` as the key.
Since C#12, the ability to use collection expressions has made it convenient to create `ReadOnlySpan<ByteArray>` as well as to initialize arrays.

```csharp
var tomlText = @"
key = ""value""
number = 123
dotted.keys = ""value""
array = [1, 2, 3]

[table]
key = ""value""

[[ArrayOfTables]]
key = ""value""

[[ArrayOfTables]]
number = 123
"u8;

var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);
{
    var tomlValue = package!.Find("key"u8);
    var value = tomlValue!.GetString(); // "value"

    var tomlValue2 = package!.Find("number"u8);
    var value2 = tomlValue2!.GetInt64(); // "123"
}
// Dotted keys
{
    var tomlValue = package!.Find(["dotted"u8, "keys"u8]); // "value"
    var value = tomlValue?.GetString(); // "value"

    var tomlValue2 = package!.Find("dotted.keys"u8, CsTomlPackageOptions.DottedKeys); 
    var value2 = tomlValue2!.GetString(); // "value"
}
// array
{
    var tomlArray = package!.Find("array"u8);
    foreach(var tomlValue in tomlArray!.GetArray())
    {
        var value = tomlValue!.GetInt64();
    }
}
// table
{
    var tomlValue = package!.Find("table"u8, "key"u8); 
    var value = tomlValue!.GetString(); // "value"
}
// ArrayOfTables
{
    var tomlValue = package!.Find("ArrayOfTables"u8, 0, "key"u8);  // "value"
    var value = tomlValue!.GetString(); // "value"
}
```

The second is to use the optional function CsTomlPackageOptions.DottedKeys.
For example, to search for the dotted key `a.b`, execute `Find("a.b "u8, CsTomlPackageOptions.DottedKeys)`.
The internal process is to split `"a.b"` into `a` and `b` before searching, but this is superior in performance because it splits the string without creating unnecessary strings.
Note that the dot (.) as a delimiter, it may not be possible to search for some keys.
If you want to search exactly, you should use `ReadOnlySpan<ByteArray>`.

```csharp
var tomlText = @"
key = ""value""
number = 123
dotted.keys = ""value""
array = [1, 2, 3]

[table]
key = ""value""

[[ArrayOfTables]]
key = ""value""

[[ArrayOfTables]]
number = 123
"u8;

var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);
{
    if(package!.TryGetValue("key"u8, out var tomlValue))
    {
        var value = tomlValue!.GetString(); // "value"
    }
    if (package!.TryGetValue("number"u8, out var tomlValue2))
    {
        var value2 = tomlValue2!.GetInt64(); // "123"
    }
}
// Dotted keys
{
    if (package!.TryGetValue(["dotted"u8, "keys"u8], out var tomlValue))
    {
        var value = tomlValue!.GetString(); // "value"
    }
    if (package!.TryGetValue("dotted.keys"u8, out var tomlValue2, CsTomlPackageOptions.DottedKeys))
    {
        var value2 = tomlValue2!.GetString(); // "value"
    }
}
// array
{
    if (package!.TryGetValue("array"u8, out var tomlArray))
    {
        foreach (var tomlValue in tomlArray!.GetArray())
        {
            var value = tomlValue!.GetInt64();
        }
    }
}
// table
{
    if (package!.TryGetValue("table"u8, "key"u8, out var tomlValue))
    {
        var value = tomlValue!.GetString(); // "value"
    }
}
// ArrayOfTables
{
    if (package!.TryGetValue("ArrayOfTables"u8, 0, "key"u8, out var tomlValue))
    {
        var value = tomlValue!.GetString(); // "value"
    }
}
```

The third is a search using the `CsTomlPackage.RootNode` property and indexer.
The advantage is that it is faster than `Find` and can search multiple combinations of values, such as inline tables and arrays.
This will give you a `CsTomlPackageNode` instead of a `CsTomlValue`, but you can use the same API as the `CsTomlValue` to get the value.

```csharp
var tomlText = @"
key = ""value""
number = 123
dotted.keys = ""value""
array = [1, 2, 3]

[table]
key = ""value""

[[ArrayOfTables]]
key = ""value""

[[ArrayOfTables]]
number = 123
"u8;

var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);
{
    var value = package!.RootNode["key"u8].GetString(); // "value"
    var value2 = package!.RootNode["number"u8].GetInt64(); // 123
}
// Dotted keys
{
    var value = package!.RootNode["dotted"u8]["keys"].GetString(); // "value"
}
// array
{
    var tomlArray = package!.RootNode["array"].GetArray();
    foreach (var tomlValue in tomlArray!)
    {
        var value = tomlValue!.GetInt64();
    }
}
// table
{
    var value = package!.RootNode["table"u8]["key"u8].GetString(); // "value"
}
// ArrayOfTables
{
    var value = package!.RootNode["ArrayOfTables"u8][0]["key"u8].GetString(); // "value"
}
```

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

The following API is used to obtain internal values from `CsTomlValue`.
`Get~` raises an exception, but `TryGet~` does not, and the return value can be used to determine if the acquisition was successful.
Whether it can be obtained or not can be determined by using `CanGetValue`.

```csharp
public partial class CsTomlValue
{
    public virtual bool CanGetValue(CsTomlValueFeature feature)

    public ReadOnlyCollection<CsTomlValue> GetArray()
    public CsTomlValue GetArrayValue(int index)
    public virtual string GetString()
    public virtual long GetInt64()
    public virtual double GetDouble()
    public virtual bool GetBool()
    public virtual DateTime GetDateTime()
    public virtual DateTimeOffset GetDateTimeOffset()
    public virtual DateOnly GetDateOnly()
    public virtual TimeOnly GetTimeOnly()
    public T GetNumber<T>() where T : INumberBase<T>

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
    public bool TryGetNumber<T>(out T value)
}
```

## Extensions

An extension option for `CsToml` is `CsToml.Extensions`.  
This option mainly provides an API for parsing toml files, as well as Stream and PipeReader based APIs.
The basic API for parsing from toml file path involve `CsTomlFileSerializer.Deserialize` and `CsTomlFileSerializer.DeserializeAsync`.  
The basic Stream-based API involve `CsTomlStreamingSerializer.Deserialize`.

```csharp
var package = CsTomlFileSerializer.Deserialize<CsTomlPackage>("test.toml");
var packageAsync = await CsTomlFileSerializer.DeserializeAsync<CsTomlPackage>("test.toml");

using (var stream = new FileStream("test.toml", FileMode.Open))
{
    var streamPackage = await CsTomlStreamingSerializer.DeserializeAsync<CsTomlPackage>(stream);
}

using (var stream = new FileStream("test.toml", FileMode.Open))
{
    var pipeReaderPackage = await CsTomlStreamingSerializer.DeserializeAsync<CsTomlPackage>(PipeReader.Create(stream));
}
```

```csharp
public partial class CsTomlFileSerializer
{
    public static TPackage? Deserialize<TPackage>(string? path, CsTomlSerializerOptions? options = null)
    public static async ValueTask<TPackage?> DeserializeAsync<TPackage>(string? path, CsTomlSerializerOptions? options = null, CancellationToken cancellationToken = default, bool configureAwait = true)
}

public partial class CsTomlStreamingSerializer
{
    public static ValueTask<TPackage> DeserializeAsync<TPackage>(Stream stream, CsTomlSerializerOptions? options = null, bool configureAwait = false, CancellationToken cancellationToken = default)
    public static ValueTask<TPackage> DeserializeAsync<TPackage>(PipeReader reader, CsTomlSerializerOptions? options = null, bool configureAwait = false, CancellationToken cancellationToken = default)
}
```

## Third Party Libraries

`CsToml.Extensions` references [Cysharp/NativeMemoryArray](https://github.com/Cysharp/NativeMemoryArray) as a dependent library.  

## UnitTest

Please note that we are using the TOML files located in the [‘tests/’ directory of the ‘toml-test repository (MIT License)’](https://github.com/toml-lang/toml-test/tree/master/tests) for some of our unit tests.

## License

MIT License. Some code is implemented based on [dotnet/runtime](https://github.com/dotnet/runtime), Please check the original license.
