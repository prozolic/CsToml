# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

CsToml is a TOML parser/serializer for .NET built for throughput and near-zero allocation. Four packages, versioned together via `VersionPrefix` in `src/NuGet.props`:

- **CsToml** — reader/parser, document model, writer, built-in formatters
- **CsToml.Generator** — Roslyn incremental source generator (`netstandard2.0`, for Roslyn compat)
- **CsToml.Extensions** — file I/O wrappers (`CsTomlFileSerializer`)
- **CsToml.Extensions.Configuration** — `Microsoft.Extensions.Configuration` provider

Solution is `CsToml.slnx` (slnx format). Libraries multi-target `net8.0;net9.0;net10.0` — set once in `src/Directory.Build.props`, along with `LangVersion 14.0` and `Nullable enable`. The core lib adds `AllowUnsafeBlocks` and `IsAotCompatible`.

## Commands

### Build

```bash
dotnet build
dotnet build -c Release
```

There is no `global.json`, so the newest installed SDK wins (currently a .NET 11 preview → `NETSDK1057` preview warnings are expected noise). There is no separate lint or format step: `TreatWarningsAsErrors`, `AnalysisLevel` and central package management are deliberately not adopted, and style is enforced only by `.editorconfig` during build.

### Test

Test projects run **xUnit v3 on Microsoft.Testing.Platform** (`UseMicrosoftTestingPlatformRunner`), not VSTest. `dotnet test --filter` does **not** work here — pass runner args after `--`:

```bash
dotnet test                                                                    # every project x 3 TFMs
dotnet test tests/CsToml.Tests/CsToml.Tests.csproj --framework net10.0         # one project, one TFM

# a single class / method
dotnet test tests/CsToml.Tests/CsToml.Tests.csproj --framework net10.0 -- --filter-class "CsToml.Tests.PushBareKeyTest"
dotnet test tests/CsToml.Tests/CsToml.Tests.csproj --framework net10.0 -- --filter-method "*.SerializeWithHeaderOption*"
dotnet test tests/CsToml.Tests/CsToml.Tests.csproj --framework net10.0 -- --list-tests
```

`--filter-class` / `--filter-method` / `--filter-namespace` / `--filter-trait` are "simple filters" and cannot be mixed with `--filter-query`. Wildcards are allowed at either end.

### Benchmarks

```bash
cd sandbox/Benchmark
dotnet run -c Release -- --filter "*ParseBenchmark*"
dotnet run -c Release --framework net10.0 -- --filter "*ClassSerializationBenchmark*"
```

`Program.cs` is `#if DEBUG`-gated: a Debug build bypasses `BenchmarkSwitcher` entirely and just runs `DefaultParseBenchmark` once — **always pass `-c Release`**. `BenchmarkConfig` registers a MediumRun job per TFM (net8/9/10) with `MemoryDiagnoser`; `--framework` narrows that to one.

### Manual / AOT checks

```bash
cd sandbox/ConsoleApp && dotnet run            # scratch target; also exercises the generator on build
cd sandbox/ConsoleNativeAOT && dotnet publish -c Release   # PublishAot, net10.0, no RID pinned
```

## Architecture

### Parse pipeline — byte-first, no `string` intermediate

`CsTomlSerializer.Deserialize` → `Utf8SequenceReader` (wraps the caller's `ReadOnlySpan<byte>` / `ReadOnlySequence<byte>` **without copying**) → `CsTomlReader` (`src/CsToml/CsTomlReader.cs`, ~3k lines; all lexing lives here — keys, strings, numbers, date/times, arrays) → `CsTomlParser` (a small `ref struct` state machine: `Comment` / `KeyValue` / `TableHeader` / `ArrayOfTablesHeader`) → `TomlDocument.Parse` drives that loop and builds the `TomlTableNode` tree.

Two things to know before editing the reader:

- The whole chain is `ref struct`s threaded by `ref`. Adding a field or changing a signature ripples through every layer.
- The reader must survive input split across multiple `ReadOnlySequence<byte>` segments. Nearly all tests feed a single span, so segment-boundary paths are easy to leave silently untested. `BasicStringSlowPathTest.cs` and `ArrayReadMultiSegmentTest.cs` show the pattern: build a real multi-segment sequence from the internal `ByteSequenceSegment` (reachable from tests via `InternalsVisibleTo`).

### Errors accumulate; they don't fail fast

`TomlDocument.Parse` catches `CsTomlException` per logical line, records a `CsTomlParseException` (carrying the line number), calls `reader.SkipOneLine()`, and continues. At the end it throws a single `CsTomlSerializeException` whose `ParseExceptions` holds all of them. New throw sites go through `ExceptionHelper` (`[DoesNotReturn]` + `NoInlining`), never a bare `throw` in a hot method.

### Formatter resolution

`ITomlValueFormatter<T>` is the one (de)serialization abstraction. `TomlValueFormatterResolver.Instance` resolves through a static generic `Cache<T>` — a per-`T` static constructor, so lookups after the first are a field read, not a dictionary probe. Resolution order:

1. `PrimitiveObjectFormatterResolver` — only when `T == object`
2. `BuiltinFormatterResolver` — the ~95 formatters in `src/CsToml/Formatter/`. Concrete types are registered directly; open generics (`List<>`, `ImmutableDictionary<,>`, `ValueTuple<...>`, …) go through a `Type → Type` table plus `MakeGenericType`, with enums and arrays handled by dedicated paths.
3. `TomlSerializedObjectFormatterResolver` — types registered by generated code

An unresolved `T` throws `ThrowNotRegisteredInResolver<T>`. Adding built-in support for a new .NET type means a new `XxxFormatter.cs` **plus** its entry in `BuiltinFormatterResolver`.

### Two API layers

- `Deserialize<T>` / `Serialize<T>` — a whole TOML document.
- `DeserializeValueType<T>` / `SerializeValueType<T>` — a bare TOML *value*, no document/table structure. The write side is the same writer with a flag flipped in its constructor.

`Deserialize<TomlDocument>` short-circuits: `TomlDocument` implements `ITomlValueFormatter<TomlDocument>` and simply returns itself, so no formatter lookup happens.

### Writer

`Utf8TomlDocumentWriter<TBufferWriter>` is a `ref struct` generic over `where TBufferWriter : IBufferWriter<byte>`. It tracks the current key path and value state in `TempList<T>`s backed by caller-stack `InlineArray4`, so nesting stays allocation-free until it overflows. Any method taking a caller `stackalloc` span must declare it `scoped ReadOnlySpan<byte>` — formatters are ordinary classes, and without `scoped` the compiler refuses to let stack memory cross into the ref struct.

### Spec versions

`CsTomlSerializerOptions.Spec` defaults to `TomlSpec.Version100`. `Version110` is just a bundle of opt-in flags (`AllowNewlinesInInlineTables`, `AllowTrailingCommaInInlineTables`, `SupportsEscapeSequenceE`/`X`, `AllowSecondsOmissionInTime`). `AllowUnicodeInBareKeys` is an unofficial extension, off in both. Reader code branches on the individual flags — never on a version number.

### Configuration provider

`CsToml.Extensions.Configuration` does not stream: `TomlStreamConfigurationParser` deserializes the whole thing to a `TomlDocument`, then walks it and flattens every leaf into the `Dictionary<string, string?>` that `Microsoft.Extensions.Configuration` expects, joining path segments with `ConfigurationPath.KeyDelimiter` and indexing array elements. An empty table becomes a null value; a duplicate flattened key throws `FormatException`.

### Source generator

`Generator.cs` is an `IIncrementalGenerator`: attributes are emitted from `RegisterPostInitializationOutput`, types are picked up by `ForAttributeWithMetadataName("CsToml.TomlSerializedObjectAttribute")`, `TypeMeta` analyzes members/constructors, `FormatterTypeMetaData.GetTomlSerializationKind()` picks a `TomlSerializationKind`, and the emitter writes an `ITomlSerializedObject<T>` implementation (static-abstract `Serialize` / `Deserialize` / `Register`).

The user-facing contract it implements (documented at length in README.md):

- `[TomlSerializedObject(NamingConvention = ...)]` on a `partial` type; `[TomlValueOnSerialized]` on each public instance property, optionally with `AliasName` (overrides the key) and `NullHandling` (`Error` throws on a null nullable property, `Ignore` skips it).
- Read-only and `private set` properties deserialize through a constructor. Constructor selection: non-public ctors are ignored; with several candidates the parameterized one with the most parameters whose names match member names (case-insensitive) wins.
- Violations surface as `CsTomlErrorNNN` diagnostics — abstract type, nested type, non-public setter, duplicate key/alias, and so on. All of them live in `DiagnosticDescriptors.cs`; add new ones there rather than throwing from the emitter.

Non-obvious parts:

- Because the generator targets `netstandard2.0` it cannot reference the runtime library, so shared shapes are **duplicated by hand**: `TomlNamingConvention` exists as `public` in `src/CsToml/` and as `internal` in `src/CsToml.Generator/`, and the attributes are string templates inside `Generator.cs`. Changing one side without the other compiles fine and breaks at runtime.
- Bare keys are emitted into a **single shared `CsTomlKeyCache` class**. Cache field names must derive from the key string, not the property name — otherwise two types with the same property name but different `[TomlValueOnSerialized(aliasName:)]` overwrite each other's key and produce wrong output. `Generator/KeyCacheTest.cs` guards this.
- Incrementality depends on every pipeline value being equatable. Use `Internal/EquatableArray.cs` instead of `ImmutableArray` for anything flowing through the pipeline, and keep both sides of `Collect()` equatable, or caching silently degrades to re-running on every keystroke.
- `hintName` sanitization replaces `<`, `>`, `,` with `_` so generic types produce legal filenames.

### Generator tests are real round-trips

`tests/CsToml.Generator.Tests` references the generator as an `Analyzer`, so types under `Declarations/` are genuinely generated at compile time. A test means: declare a `[TomlSerializedObject] partial` type in `Declarations/`, then assert exact UTF-8 output or a round-trip in `Generator/`. There are no snapshot files.

### Compliance tests

`tests/CsToml.Tests/TomlTest.cs` is data-driven over `toml-test/` (vendored from toml-lang/toml-test): each `valid/*.toml` is compared against its `.json` via `ToJsonObject()`, each `invalid/*.toml` must throw. `files-toml-1.0.0` / `files-toml-1.1.0` list which files run under which spec; `ExcludedFilesForV110` is the only opt-out. Most of the test count comes from here — dropping a `.toml`/`.json` pair into `toml-test/` adds cases automatically.

## Conventions

`.editorconfig`: 4-space indent for `.cs`, 2-space for csproj/props/json/yml/toml; `var` preferred except for built-in types; `IDE0044` and `IDE0051` disabled.

## CI

`.github/workflows/build.yml` runs `dotnet restore` → `build --no-restore` → `test --no-build` on ubuntu-latest with SDKs 8/9/10. All three TFMs must build and pass on Linux, so avoid Windows-only assumptions in tests (path separators, file casing).
