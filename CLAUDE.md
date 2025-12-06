# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

CsToml is a high-performance TOML parser and serializer library for .NET that prioritizes speed and minimal memory allocation. The project consists of multiple components:

- **CsToml** - Core TOML parser/serializer library
- **CsToml.Generator** - Roslyn source generator for automatic serialization code generation
- **CsToml.Extensions** - File I/O utilities and additional functionality
- **CsToml.Extensions.Configuration** - Microsoft.Extensions.Configuration provider for TOML files

## Common Development Commands

### Building
```bash
# Build entire solution
dotnet build

# Build in Release mode
dotnet build --configuration Release

# Restore dependencies
dotnet restore
```

### Testing
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/CsToml.Tests/
dotnet test tests/CsToml.Generator.Tests/
dotnet test tests/CsToml.Extensions.Configuration.Tests/

# Run tests with specific framework
dotnet test --framework net9.0
```

### Benchmarks
```bash
# Run performance benchmarks
cd sandbox/Benchmark
dotnet run --configuration Release

# Run specific benchmark filter
dotnet run --configuration Release -- [filter]
```

### Development Testing
```bash
# Console app for manual testing
cd sandbox/ConsoleApp
dotnet run

# Native AOT compatibility testing
cd sandbox/ConsoleNativeAOT
dotnet publish --configuration Release
```

## Architecture

### Core Library Structure
- **Values/** - TOML value type implementations (TomlString, TomlInteger, TomlArray, etc.)
- **Formatter/** - Type formatters for serialization/deserialization of .NET types
- **Utility/** - Low-level buffer management, UTF-8 processing, memory optimization
- **Error/** - Exception handling and error reporting

### Performance Design Principles
- Uses `ReadOnlySpan<byte>` and `ReadOnlySequence<byte>` instead of string processing
- Leverages `ArrayPool<T>` for buffer management to minimize allocations
- Direct `System.Buffers` API integration (`IBufferWriter<byte>`)
- Byte-first processing throughout the pipeline

### Source Generator (CsToml.Generator)
- Generates serialization code at compile time using Roslyn analyzers
- Supports `[TomlSerializedObject]` and `[TomlValueOnSerialized]` attributes
- Handles complex type hierarchies and constructor patterns
- AOT (Native AOT) compatible code generation
- **Generic Type Support**: Enhanced support for generic type parameters (`TypeParameter`, `NullableStructWithTypeParameter`)
- **File Naming**: Replaces `<`, `>`, `,` with underscores in generated filenames for filesystem compatibility

### Target Frameworks
- **Main libraries**: .NET 8.0, 9.0, 10.0
- **Source generator**: .NET Standard 2.0 (for Roslyn compatibility)
- **Language version**: C# 14.0

## Testing Strategy

### Test Structure
- **Unit tests** - Standard xUnit tests for functionality
- **TOML compliance tests** - Uses official TOML v1.0.0 test suite from toml-lang/toml-test
- **Performance tests** - BenchmarkDotNet comparison against Tommy, Tomlet, Tomlyn libraries
- **AOT compatibility tests** - Validates Native AOT scenarios

### Test Data Location
Official TOML test cases are located at `tests/CsToml.Tests/toml-test/` with both valid and invalid test scenarios.

## Development Workflow

### Making Changes to Core Library
1. Modify source in `src/CsToml/`
2. Run tests: `dotnet test tests/CsToml.Tests/`
3. Run benchmarks to verify performance: `cd sandbox/Benchmark && dotnet run -c Release`
4. Test with console app: `cd sandbox/ConsoleApp && dotnet run`

### Working with Source Generator
1. Modify generator code in `src/CsToml.Generator/`
2. Test generation: `dotnet build sandbox/ConsoleApp` (triggers source generation)
3. Run generator tests: `dotnet test tests/CsToml.Generator.Tests/`
4. Debug generator using `sandbox/ConsoleApp` project as target
5. **AOT Testing**: Use `sandbox/ConsoleNativeAOT` to test Native AOT compatibility
   - Build: `dotnet publish --configuration Release`
   - Run: `./bin/Release/net8.0/linux-x64/publish/ConsoleNativeAOT`

### Memory and Performance Considerations
- All buffer operations should use `ArrayPool<byte>.Shared` when possible
- Prefer `ReadOnlySpan<byte>` over `string` for TOML text processing
- Use `stackalloc` for small, fixed-size buffers
- Validate performance impact with benchmarks for any core changes

### Error Handling
- Use `CsTomlException` for parsing errors with detailed position information
- Wrap multiple errors in `CsTomlSerializeException.ParseExceptions`
- Include line numbers and character positions in error messages

## Key Files for Understanding

- `src/CsToml/CsTomlSerializer.cs` - Main public API
- `src/CsToml/CsTomlParser.cs` - Core parsing logic
- `src/CsToml/CsTomlReader.cs` - Core TOML reader
- `src/CsToml/TomlDocument.cs` - Document model for preserving TOML structure
- `src/CsToml/Formatter/*Formatter.cs` - Built-in support type serialization behavior
- `src/CsToml.Generator/Generator.cs` - Source generator implementation
- `src/CsToml.Generator/TypeMeta.cs` - Type analysis and metadata for code generation
- `src/CsToml.Generator/FormatterTypeMetaData.cs` - Formatter resolution and type mapping logic
- `src/CsToml.Generator/TomlSerializationKind.cs` - Serialization kind enumeration (includes TypeParameter support)
- `tests/CsToml.Tests/TomlTest.cs` - TOML compliance test runner
- `tests/CsToml.Generator.Tests/` - Source generator specific tests

## Source Generator Architecture

The source generator operates in several phases:

1. **Type Discovery**: Identifies types marked with `[TomlSerializedObject]`
2. **Type Analysis**: `TypeMeta` analyzes properties, constructors, and type relationships
3. **Serialization Kind Detection**: `FormatterTypeMetaData.GetTomlSerializationKind()` determines how each type should be serialized
4. **Code Generation**: Generates `ITomlSerializedObject<T>` implementations with proper formatter registration

### Type Parameter Handling
- `TypeParameter` and `NullableStructWithTypeParameter` serialization kinds handle generic constraints
- Generated code includes runtime type checking for generic type parameters
- AOT-compatible patterns avoid reflection where possible
