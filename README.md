# CsToml - TOML Parser  for .NET

CsToml is TOML parser for .NET.  
For more information about TOML, visit the official website at [https://toml.io/en/](https://toml.io/en/)

> [!NOTE]
> It is currently in preview. The library name and API may undergo breaking changes.

```csharp
using CsToml;
using System.Text;

var tomlText = @"
key = ""value""
number = 123
"u8;

var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);

if (package.TryGetValue("key"u8, out var value))
{
    var str = value!.GetString();
    Console.WriteLine($"key = {str}"); // key = value
}

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

## Deserialize a toml format string or file

Deserializing from UTF-8 strings and TOML file paths to a `CsTomlPackage` is possible.  

```csharp
var tomlText = @"
key = ""value""
number = 123
"u8;

var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);
```

The basic API involve `CsTomlPackage? package = CsTomlSerializer.DeserializeFromFile<CsTomlPackage>("test.toml")`, `CsTomlPackage? package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText)` and `CsTomlPackage? package = await CsTomlSerializer.DeserializeFromFileAsync<CsTomlPackage>("test.toml")`.
When parsing from TOML files, you can use `DeserializeFromFile`.
When parsing from UTF-8 string(`ReadOnlySpan<byte>` or `ReadOnlySequence<byte>`) in TOML format, you can use `Deserialize`.  

```csharp
public static TPackage? DeserializeFromFile<TPackage>(string? path, CsTomlSerializerOptions? options = null)
public static async ValueTask<TPackage?> DeserializeFromFileAsync<TPackage>(string? path, CsTomlSerializerOptions? options = null, CancellationToken cancellationToken = default, bool configureAwait = true)
public static TPackage? Deserialize<TPackage>(ReadOnlySpan<byte> tomlText, CsTomlSerializerOptions? options = null)
public static TPackage? Deserialize<TPackage>(in ReadOnlySequence<byte> tomlTextSequence, CsTomlSerializerOptions? options = null)
```

`DeserializeFromFile` and `Deserialize` accepts `CsTomlSerializerOptions? options` as parameters.
`IsThrowCsTomlException` determines whether an exception is thrown if a syntax error occurs during parsing.
If you set to false, no syntax error is thrown. Errors that occur during analysis can be found at `CsTomlPackage.Exceptions`.
The default is true, which means that an error is thrown during parsing.  

```csharp
// throw CsTomlException
var errorPackage = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);

// no CsTomlException is thrown
var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText, CsTomlSerializerOptions.NoThrow);
if (package.Exceptions.Count > 0)
{
    foreach (CsTomlException? e in package.Exceptions)
    {
        // check error
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
var result = CsTomlSerializer.Serialize(package);
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
public static ByteMemoryResult Serialize<TPackagePart>(ref TPackagePart target)
public static void Serialize<TBufferWriter, TPackagePart>(ref TBufferWriter bufferWriter, ref TPackagePart target)
public static ByteMemoryResult Serialize<TPackage>(TPackage? package)
public static void Serialize<TBufferWriter, TPackage>(ref TBufferWriter bufferWriter, TPackage? package)
```

## How to get the value

```csharp
var tomlText = @"
key = ""value""
number = 123
"u8;

var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);

var value = package!.Find("key"u8);
//var result = package!.TryGetValue("key"u8, out var value);

string str = value!.GetString();
Console.WriteLine($"value = \"{str}\"");
// value = "value"
```

### Searching for a toml value from a key

CsToml converts deserialized TOML format values to `CsTomlValue`.
`CsTomlValue` can be obtained by calling `TryGetValue` and `Find`, the APIs of `CsTomlPackage`, with the key.

```csharp
public bool TryGetValue(ReadOnlySpan<byte> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
public bool TryGetValue(ReadOnlySpan<ByteArray> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
public bool TryGetValue(ReadOnlySpan<byte> tableHeaderKey, ReadOnlySpan<byte> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
public bool TryGetValue(ReadOnlySpan<ByteArray> tableHeader, ReadOnlySpan<byte> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
public bool TryGetValue(ReadOnlySpan<char> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
public bool TryGetValue(ReadOnlySpan<char> tableHeader, ReadOnlySpan<char> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
public bool TryGetValue(ReadOnlySpan<byte> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<byte> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
public bool TryGetValue(ReadOnlySpan<byte> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<ByteArray> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
public bool TryGetValue(ReadOnlySpan<ByteArray> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<byte> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)
public bool TryGetValue(ReadOnlySpan<ByteArray> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<ByteArray> key, out CsTomlValue? value, CsTomlPackageOptions? options = default)

public CsTomlValue? Find(ReadOnlySpan<byte> keys, CsTomlPackageOptions? options = default)
public CsTomlValue? Find(ReadOnlySpan<char> keys, CsTomlPackageOptions? options = default)
public CsTomlValue? Find(ReadOnlySpan<ByteArray> keys, CsTomlPackageOptions? options = default)
public CsTomlValue? Find(ReadOnlySpan<byte> tableHeader, ReadOnlySpan<byte> key, CsTomlPackageOptions? options = default)
public CsTomlValue? Find(ReadOnlySpan<char> tableHeader, ReadOnlySpan<char> key, CsTomlPackageOptions? options = default)
public CsTomlValue? Find(ReadOnlySpan<ByteArray> tableHeader, ReadOnlySpan<byte> key, CsTomlPackageOptions? options = default)
public CsTomlValue? Find(ReadOnlySpan<byte> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<byte> key, CsTomlPackageOptions? options = default)
public CsTomlValue? Find(ReadOnlySpan<byte> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<ByteArray> key, CsTomlPackageOptions? options = default)
public CsTomlValue? Find(ReadOnlySpan<ByteArray> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<byte> key, CsTomlPackageOptions? options = default)
public CsTomlValue? Find(ReadOnlySpan<ByteArray> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<ByteArray> key, CsTomlPackageOptions? options = default)
public CsTomlValue? Find(ReadOnlySpan<char> arrayOfTableHeader, int arrayIndex, ReadOnlySpan<char> key, CsTomlPackageOptions? options = default)
```

There are three ways to search with the dot key.
The first is to search using `ReadOnlySpan<ByteArray>` as the key.
Since C#12, the ability to use collection expressions has made it convenient to create `ReadOnlySpan<ByteArray>` as well as to initialize arrays.  
The second is to use the optional function CsTomlPackageOptions.DottedKeys.
For example, to search for the dotted key `a.b`, execute `Find("a.b "u8, CsTomlPackageOptions.DottedKeys)`.
The internal process is to split `"a.b"` into `a` and `b` before searching, but this is superior in performance because it splits the string without creating unnecessary strings.
Note that the dot (.) as a delimiter, it may not be possible to search for some keys.
If you want to search exactly, you should use `ReadOnlySpan<ByteArray>`.  
The third is to use an indexer.
For example, to search for the dotted key a.b, execute `package!["a"u8]["b"u8].Value`.
It is possible to search faster than the above two.

```csharp
var tomlText = @"
key = ""value""
number = 123
dotted.keys = ""value""

[table]
key = ""value""

[[ArrayOfTables]]
key = ""value""

[[ArrayOfTables]]
number = 123
"u8;

var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);

{
    var value = package!.Find("key"u8);
    var rawValue = value?.GetString(); // "value"
    var result = package!.TryGetValue("key"u8, out var value2); // "value"
    var result2 = package!["key"u8].Value.GetString();  // "value" 
}

// Dotted keys
{
    var value = package!.Find(["dotted"u8,"keys"u8]); // "value"
    var result = package!.TryGetValue(["dotted"u8, "keys"u8], out var value2); // "value"

    var value3 = package!.Find("dotted.keys"u8, CsTomlPackageOptions.DottedKeys); // "value"
    var result2 = package!.TryGetValue("dotted.keys"u8, out var value4, CsTomlPackageOptions.DottedKeys); // "value"
    var result3 = package!["dotted"u8]["keys"u8].Value.GetString();  // "value" 
}

// table
{
    var value = package!.Find("table"u8, "key"u8); // "value"
    var result = package!.TryGetValue("table", "key", out var value2); // "value"
    var result2 = package!["table"u8]["key"u8].Value.GetString();  // "value" 
}

// ArrayOfTables
{
    var value = package!.Find("ArrayOfTables"u8, 0, "key"u8);  // "value"
    var result = package!.TryGetValue("ArrayOfTables"u8, 0, "key"u8, out var value2); // "value"
    var result2 = package!["arrayOfTables"u8][0]["key"u8].Value.GetInt64() // "value"
}
{
    var value = package!.Find("ArrayOfTables"u8, 1, "number"u8); // 123
    var result = package!.TryGetValue("ArrayOfTables"u8, 1, "number"u8, out var value2); // 123
    var result2 = package!["arrayOfTables"u8][1]["number"u8].Value.GetString() // 123
}

```

### Casting a toml value

The following API is used to obtain internal values from CsTomlValue.
`Get~` raises an exception, but `TryGet~` does not, and the return value can be used to determine if the acquisition was successful.

```csharp
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
```

## UnitTest

Please note that we are using the TOML files located in the [‘tests/’ directory of the ‘toml-test repository (MIT License)’](https://github.com/toml-lang/toml-test/tree/master/tests) for some of our unit tests.

## License

MIT License. Some code is implemented based on [dotnet/runtime](https://github.com/dotnet/runtime) and [Cysharp/NativeMemoryArray](https://github.com/Cysharp/NativeMemoryArray), Please check the original license.
