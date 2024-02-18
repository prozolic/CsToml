# CsToml - TOML parser for .NET

CsToml is TOML parser for .NET. This library supports .NET 8 or later.  
For more information about TOML, visit the official website at https://toml.io/ja/

> [!NOTE]
> It is currently in preview. The library name and API may undergo breaking changes.

## Feature

- Implemented in .NET 8 and C# 12.
- Compatible with [TOML v1.0.0](https://toml.io/en/v1.0.0) specification.
- Parsing in UTF-8 instead of UTF-16 (Strings) maintains low allocation and high performance.

## Example

```csharp
var tomlText = @"
key = ""value""
number = 123
"u8;

var package = new CsTomlPackage();
CsTomlSerializer.Deserialize(ref package, tomlText);

if (package.TryGetValue("key", out var value22))
{
    var str = value22!.GetString();
    Console.WriteLine($"key = {str}"); // key = value
}

var serializedTomlText = CsTomlSerializer.Serialize(ref package);
Console.WriteLine(Encoding.UTF8.GetString(serializedTomlText));
// key = "value"
// number = 123

```

## Installation

## Quick Start

## API

## UnitTest

Please note that we are using the TOML files located in the [‘tests/’ directory of the ‘toml-test repository (MIT License)’](https://github.com/toml-lang/toml-test/tree/master/tests) for some of our unit tests.

## License

MIT License.
