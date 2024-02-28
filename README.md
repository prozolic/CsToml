# CsToml - TOML parser for .NET

CsToml is TOML parser for .NET. This library supports .NET 8 or later.  
For more information about TOML, visit the official website at https://toml.io/en/

> [!NOTE]
> It is currently in preview. The library name and API may undergo breaking changes.

## Feature

- Implemented in .NET 8 and C# 12.
- Supports I/O APIs (`IBufferWriter<byte>`, `ReadOnlySpan<byte>`, `ReadOnlySequence<byte>`)
- Compatible with [TOML v1.0.0](https://toml.io/en/v1.0.0) specification.
- By parsing directly in UTF-8 (byte array) instead of UTF-16 (strings), low allocation and high speed are achieved.

## Example

```csharp
var tomlText = @"
key = ""value""
number = 123
"u8;

var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);

if (package.TryGetValue("key", out var value22))
{
    var str = value22!.GetString();
    Console.WriteLine($"key = {str}"); // key = value
}

var serializedTomlText = CsTomlSerializer.Serialize(package);
Console.WriteLine(Encoding.UTF8.GetString(serializedTomlText));
// key = "value"
// number = 123

```

## Installation

preparing.

## Deserialize a toml format string or file

```csharp
var tomlText = @"
key = ""value""
number = 123
"u8;

var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);
```

| Name(Argument) | Return |  
| --- | --- |  
| **ReadAndDeserialize**(`string?` path, `CsTomlSerializerOptions?` options = null) | `TPackage : CsTomlPackage, ICsTomlPackageCreator<TPackage>` |  
| **ReadAndDeserializeAsync**(`string?` path, `CsTomlSerializerOptions?` options = null, `CancellationToken` cancellationToken = default) | `ValueTask<TPackage?> : CsTomlPackage, ICsTomlPackageCreator<TPackage>` |  
| **Deserialize**(`ReadOnlySpan<byte>` tomlText, `CsTomlSerializerOptions?` options = null) | `TPackage : CsTomlPackage, ICsTomlPackageCreator<TPackage>` |  
| **Deserialize**(`in ReadOnlySequence<byte>` tomlTextSequence, `CsTomlSerializerOptions?` options = null) | `TPackage : CsTomlPackage, ICsTomlPackageCreator<TPackage>` |  

## Serialize to toml format string or file

```csharp
var tomlText = @"
key = ""value""
number = 123
"u8;

var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);
var toml = CsTomlSerializer.Serialize(package);
// key = "value"
// number = 123
```

| Name(Argument) | Return |  
| --- | --- |  
| **Serialize**(`TPackage?` package) `where TPackage : CsTomlPackage` | `byte[]` |  
| **Serialize**(`in IBufferWriter<byte>` bufferWriter, `TPackage?` package) `where TPackage : CsTomlPackage` | `void` |  

## API

It provides various APIs.

### CsTomlPackage

| Name(Argument) | Return |  
| --- | --- |  
| **TryGetValue**(`ReadOnlySpan<byte>` key, out `CsTomlValue?` value) | `bool` |  
| **TryGetValue**(`string` key, out `CsTomlValue?` value) | `bool` |  
| **Find(ByteArray[] keys)**(`ByteArray[]` keys) | `CsTomlValue` |  
| **Find(ByteArray[] keys)**(`ByteArray[]` tableHeaderKeys, `int` arrayIndex, `ByteArray[]` keys) | `CsTomlValue` |  

The `ByteArray` class is used as a substitute because the Span cannot be used with the params keyword.  


### CsTomlValue

| Name(Argument) | Return |  
| --- | --- |  
| **GetString**() | `string` |  
| **GetInt64**() | `long` |  
| **GetDouble**() | `double` |  
| **GetBool**() | `bool` |  
| **GetDateTime**() | `DateTime` |  
| **GetDateTimeOffset**() | `DateTimeOffset` |  
| **GetDateOnly**() | `DateOnly` |  
| **GetTimeOnly**() | `TimeOnly` |  
| **GetNumber**`<T>`() `where T : INumberBase<T>` | `string` |  
| **TryGetString**(`out string` value) | `bool` |  
| **TryGetInt64**(`out long` value) | `bool` |  
| **TryGetDouble**(`out double` value) | `bool` |  
| **TryGetBool**(`out bool` value) | `bool` |  
| **TryGetDateTime**(`out DateTime` value) | `bool` |  
| **TryGetDateTimeOffset**(`out DateTimeOffset` value) | `bool` |  
| **TryGetDateOnly**(`out DateOnly` value) | `bool` |  
| **TryGetTimeOnly**(`out TimeOnly` value) | `bool` |  
| **TryGetNumber**`<T>`(`out T` value) `where T : INumberBase<T>` | `bool` |  

## UnitTest

Please note that we are using the TOML files located in the [‘tests/’ directory of the ‘toml-test repository (MIT License)’](https://github.com/toml-lang/toml-test/tree/master/tests) for some of our unit tests.

## License

MIT License. Some code is implemented based on [dotnet/runtime](https://github.com/dotnet/runtime) and [Cysharp/NativeMemoryArray](https://github.com/Cysharp/NativeMemoryArray), Please check the original license.
