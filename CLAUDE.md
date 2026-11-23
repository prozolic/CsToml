# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

CsToml is a TOML parser/serializer for .NET built for throughput and near-zero allocation. Four packages, versioned together via `VersionPrefix` in `src/NuGet.props`:

- **CsToml** — reader/parser, document model, writer, built-in formatters
- **CsToml.Generator** — Roslyn incremental source generator (`netstandard2.0`, for Roslyn compat)
- **CsToml.Extensions** — file I/O wrappers (`CsTomlFileSerializer`)
- **CsToml.Extensions.Configuration** — `Microsoft.Extensions.Configuration` provider

Solution is `CsToml.slnx` (slnx format). Libraries multi-target `net10.0;net11.0` — set once in `src/Directory.Build.props`, along with `LangVersion 14.0` and `Nullable enable`. The core lib adds `AllowUnsafeBlocks` and `IsAotCompatible`.

## Commands

### Build

```bash
dotnet build
dotnet build -c Release
```

The root `global.json` only selects the test runner and pins no SDK, so the newest installed SDK wins (currently a .NET 11 preview → `NETSDK1057` preview warnings are expected noise). There is no separate lint or format step: `TreatWarningsAsErrors`, `AnalysisLevel` and central package management are deliberately not adopted, and style is enforced only by `.editorconfig` during build.

### Test

Test projects use **TUnit on Microsoft.Testing.Platform**. `global.json` puts `dotnet test` in MTP mode, so a project is passed with `--project`, and filtering uses `--treenode-filter "/<assembly>/<namespace>/<class>/<test>"` (VSTest's `--filter` does not exist):

```bash
dotnet test                                                                    # every project x 2 TFMs
dotnet test --project tests/CsToml.Tests --framework net10.0                   # one project, one TFM
dotnet test --project tests/CsToml.Tests --framework net10.0 --treenode-filter "/*/*/PushBareKeyTest/*"
dotnet test --project tests/CsToml.Tests --framework net10.0 --list-tests
```

- Each project references `TUnit.Engine` + `TUnit.Assertions`, not the `TUnit` meta-package (which adds CodeCoverage, Telemetry and TRX extensions).
- TUnit runs all tests in parallel, including tests of the same class, so tests must not share mutable static state.
- Assertions are Shouldly; for exceptions use `Assert.ThrowsExactly<T>` (exact type) or `Should.Throw<T>`.

### NativeAOT test run

The same tests also run as a NativeAOT executable (CI job `aot-test`). This relies on TUnit's default source-generation mode; never enable its reflection mode.

```bash
dotnet publish tests/CsToml.Tests/CsToml.Tests.csproj -c Release -f net10.0 -r win-x64
tests/CsToml.Tests/bin/Release/net10.0/win-x64/publish/CsToml.Tests.exe
```

- `PublishAot` is enabled only by `dotnet publish` (not by `-r` alone), so `dotnet test` stays a normal JIT run and still covers `BuiltinFormatterResolver`'s `MakeGenericType` fallback.
- Test code must be AOT-safe: no `dynamic` (use `ObjectGraph.Get<T>` in Generator tests), and locate files from `AppContext.BaseDirectory`, not the working directory.
- `[SkipOnNativeAot("reason")]` skips a test only in the AOT executable. Use it only for types resolvable solely through `MakeGenericType`, for library gaps (reasons starting with `Known gap:`; remove the skip once fixed), or for NativeAOT compiler limitations of an older TFM (guard the attribute with `#if`). Split such cases into their own test.

### Benchmarks

```bash
cd sandbox/Benchmark
dotnet run -c Release --framework net10.0 -- --filter "*ParseBenchmark*"
```

`Program.cs` is `#if DEBUG`-gated: a Debug build bypasses `BenchmarkSwitcher` entirely and just runs `DefaultParseBenchmark` once — **always pass `-c Release`**. `BenchmarkConfig` registers net10.0 and net11.0 MediumRun jobs plus a NuGet-package job with `MemoryDiagnoser`. It uses BenchmarkDotNet 0.16.0-preview.2, the first version with a net11.0 toolchain (`CsProjCoreToolchain` lives in `BenchmarkDotNet.Toolchains.NetCoreApp` there); `--framework` is required because the project multi-targets.

### Manual / AOT checks

```bash
cd sandbox/ConsoleApp && dotnet run            # scratch target; also exercises the generator on build
cd sandbox/ConsoleNativeAOT && dotnet publish -c Release   # PublishAot, net11.0, no RID pinned
```

## Architecture

### Parse pipeline — byte-first, no `string` intermediate

`CsTomlSerializer.Deserialize` → `Utf8SequenceReader` (wraps the caller's `ReadOnlySpan<byte>` / `ReadOnlySequence<byte>` **without copying**) → `CsTomlReader` (`src/CsToml/CsTomlReader.cs`, ~3k lines; all lexing lives here — keys, strings, numbers, date/times, arrays) → `CsTomlParser` (a small `ref struct` state machine: `Comment` / `KeyValue` / `TableHeader` / `ArrayOfTablesHeader`) → `TomlDocument.Parse` drives that loop and builds the `TomlTableNode` tree.

Three things to know before editing the reader:

- The whole chain is `ref struct`s threaded by `ref`. Adding a field or changing a signature ripples through every layer.
- The reader must survive input split across multiple `ReadOnlySequence<byte>` segments. Nearly all tests feed a single span, so segment-boundary paths are easy to leave silently untested. `BasicStringSlowPathTest.cs` and `ArrayReadMultiSegmentTest.cs` show the pattern: build a real multi-segment sequence from the internal `ByteSequenceSegment` (reachable from tests via `InternalsVisibleTo`).
- Arrays and inline tables are read recursively, and a stack overflow cannot be caught. `ReadValue(depth)` threads the nesting depth as a parameter, and `ReadArray` / `ReadInlineTable` start with `EnsureCanNest(depth)`, which enforces `CsTomlSerializerOptions.MaxDepth` (default 64). Any new recursive read must do the same; `NestingDepthTest.cs` covers it.

### Errors accumulate; they don't fail fast

`TomlDocument.Parse` catches `CsTomlException` per logical line, records a `CsTomlParseException` (carrying the line number), calls `reader.SkipOneLine()`, and continues. At the end it throws a single `CsTomlSerializeException` whose `ParseExceptions` holds all of them. New throw sites go through `ExceptionHelper` (`[DoesNotReturn]` + `NoInlining`), never a bare `throw` in a hot method.

### Formatter resolution

`ITomlValueFormatter<T>` is the one (de)serialization abstraction. `TomlValueFormatterResolver.Instance.GetFormatter<T>()` reads one per-`T` static slot (`Cache<T>.Formatter`, a class with no static constructor, so there is no init check) and drops into `GetFormatterSlow` only when it is null. The lookup itself runs once per `T` in the `Resolved<T>` static constructor and is published with `Interlocked.CompareExchange`; the slot doubles as the "registered" flag, so `Register<T>` is first-wins and never replaces a built-in formatter. Resolution order:

1. `BuiltinFormatterResolver` — the ~95 formatters in `src/CsToml/Formatter/`. Concrete types (including `object` → `PrimitiveObjectFormatter`) are registered in its static constructor; open generics (`List<>`, `ImmutableDictionary<,>`, `ValueTuple<...>`, …) go through a `Type → Type` table plus `MakeGenericType`, with enums and arrays handled by dedicated paths.
2. `TomlSerializedObjectFormatterResolver.TryRegister<T>` — invokes the generated `ITomlSerializedObjectRegister.Register()` by reflection. That code publishes into `Cache<T>` from inside the running `Resolved<T>` constructor (same-thread re-entrancy: `Register<T, TFormatter>()` sees the empty slot and fills it), which is why the lookup and the slot are two classes.

Generated `Register()` bodies start with `BeginRegistration<T>()` — false once `T` is registered, or while the current thread is already inside `T.Register()`, which is what terminates type cycles — then register every member formatter (`Register<T, TFormatter>()` constructs `TFormatter` only while the slot is empty; `Register<T>()` enters nested `[TomlSerializedObject]` types) and publish `T` itself **last**. The order matters: `Cache<T>` has no lock, so a thread that sees a published `T` must already find the formatters `T` needs (NativeAOT and union members have no reflection fallback); a thread racing the first lookup waits on the `Resolved<T>` constructor instead. `IsRegistered<T>` may run the lookup but never throws. An unresolved `T` throws `ThrowNotRegisteredInResolver<T>`. Adding built-in support for a new .NET type means a new `XxxFormatter.cs` **plus** its entry in `BuiltinFormatterResolver`.

### Two API layers

- `Deserialize<T>` / `Serialize<T>` — a whole TOML document.
- `DeserializeValueType<T>` / `SerializeValueType<T>` — a bare TOML *value*, no document/table structure. The write side is the same writer with a flag flipped in its constructor.

`Deserialize<TomlDocument>` short-circuits: `CsTomlSerializer` checks `typeof(T) == typeof(TomlDocument)` and returns the parsed document directly, and `Serialize<TomlDocument>` calls `ToTomlString` directly, so no formatter lookup happens and `TomlDocument` has no formatter of its own.

### Writer

`Utf8TomlDocumentWriter` is a non-generic `ref struct`, so formatters' `Serialize(ref Utf8TomlDocumentWriter writer, T target, CsTomlSerializerOptions options)` is an ordinary interface method. (With a writer type parameter on the method it was a generic virtual method: resolved at run time on every call, and compiled once per writer type for every formatter under NativeAOT.) The writer owns the buffer state itself — `Span<byte> buffer` and `int position` — and every write touches only those two with a single capacity compare. Where the buffer comes from is the job of a *buffer source*, consulted only in the non-inlined `Grow`, in `Flush` and in `Dispose`; never route individual writes through it. The buffer sources are internal structs in `src/CsToml/Utility/` that hand spans to the writer and do not write anything themselves. Both are held by value with at most one active, and they are reached through plain fields and a branch (function pointers were ruled out for safety):

- `ArrayPoolBufferSource` — owns the pooled array and grows it via `ArrayPool`; used for the `ByteMemoryResult` path and internal key formatting. The entry point must call `Dispose()`.
- `WriterBufferSource` — wraps a caller's `IBufferWriter<byte>` for `Serialize<T>(IBufferWriter<byte>, ...)`, `SerializeValueType<T>(IBufferWriter<byte>, ...)` and the `Stream` overloads. `Next` commits what was written with `Advance` and asks for the next span; `Commit` does only the final `Advance`. `Advance` therefore happens only when the current span is used up and once at the end, because `IBufferWriter<byte>` forbids writing into a span after `Advance`, and `GetSpan` is never called after the final `Advance`. The entry point must call the document writer's `Flush()` in a `finally`.

A third internal constructor takes a fixed `Span<byte>` (neither source active, so running out of room throws); only tests use it. The `IBufferWriter<byte>` overloads take the interface by value, so a struct writer is boxed at the call site and the caller's copy is not advanced.

`Utf8TomlDocumentWriter.WrittenSize` counts from the start of the current serialization (bytes already advanced on the `IBufferWriter<byte>` plus `position`), because `TomlTable` uses it to decide whether a blank line precedes a table header.

`Utf8TomlDocumentWriter` tracks the current key path and value state in `TempList<T>`s backed by caller-stack `InlineArray4`, so nesting stays allocation-free until it overflows. Any method taking a caller `stackalloc` span must declare it `scoped ReadOnlySpan<byte>` — formatters are ordinary classes, and without `scoped` the compiler refuses to let stack memory cross into the ref struct.

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

`.github/workflows/build.yml` runs `dotnet restore` → `build --no-restore` → `test --no-build` on ubuntu-latest with SDKs 10/11. Every TFM must build and pass on Linux, so avoid Windows-only assumptions in tests (path separators, file casing). A second job, `aot-test`, publishes the test projects for linux-x64 with NativeAOT and runs the executables, as a matrix over net10.0 (all three) and net11.0 (all but `CsToml.Generator.Tests`).
