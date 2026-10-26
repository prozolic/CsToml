#nullable enable

using BenchmarkDotNet.Attributes;
using CsToml;
using System.Text;

namespace Benchmark;

// Dotted-key descent (TomlTableNode.GetOrAddKeyNode: for `a.b.c = 1` it is called for `a` and `b`),
// driven through the public API only so the NuGetCsToml job (CsToml 1.8.4) can run the same code.
//   Hit:        one table, KeyCount lines "database.connection.<leaf> = <i>"; `database` and `connection`
//               are found on every line after the first, with a fresh key instance per line (byte compare).
//   HitSameRef: 16 tables "[table<j>]" with the same KeyCount lines; from the second table on, the parser
//               reuses the key instances of the previous sibling table, so lookups hit by reference.
//   Miss:       one table, KeyCount lines "<leaf>.value = <i>" with distinct first segments, so every
//               intermediate node is created once.
// Per-op time covers the whole parse (lexing, value parsing, leaf insertion), so the dictionary change is
// diluted. The NuGetCsToml job is the reference from before the GetOrAddKeyNode optimization.
public class DottedKeyParseBenchmark
{
    private static readonly string[] Words =
    [
        "name", "version", "dependencies", "owner", "database", "servers", "alpha", "ports",
        "enabled", "title", "description", "connection_max", "ip", "dc", "role", "temp_targets",
        "hosts", "products", "sku", "color", "authors", "edition", "features", "target",
        "profile", "package", "bin", "lib", "workspace", "members", "exclude", "metadata",
    ];

    private const int SiblingTableCount = 16;

#pragma warning disable CS8618
    private static byte[] hitText;
    private static byte[] hitSameRefText;
    private static byte[] missText;
#pragma warning restore CS8618

    [Params(8, 64)]
    public int KeyCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        var leaves = new string[KeyCount];
        for (int i = 0; i < KeyCount; i++)
        {
            leaves[i] = i < Words.Length ? Words[i] : Words[i % Words.Length] + i;
        }

        var builder = new StringBuilder();
        for (int i = 0; i < KeyCount; i++)
        {
            builder.AppendLine($"database.connection.{leaves[i]} = {i}");
        }
        hitText = Encoding.UTF8.GetBytes(builder.ToString());

        builder.Clear();
        for (int j = 1; j <= SiblingTableCount; j++)
        {
            builder.AppendLine($"[table{j}]");
            for (int i = 0; i < KeyCount; i++)
            {
                builder.AppendLine($"database.connection.{leaves[i]} = {i}");
            }
        }
        hitSameRefText = Encoding.UTF8.GetBytes(builder.ToString());

        builder.Clear();
        for (int i = 0; i < KeyCount; i++)
        {
            builder.AppendLine($"{leaves[i]}.value = {i}");
        }
        missText = Encoding.UTF8.GetBytes(builder.ToString());
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public TomlDocument CsToml_Parse_DottedKeyHit()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(hitText);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public TomlDocument CsToml_Parse_DottedKeyHitSameRef()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(hitSameRefText);
        return document;
    }

    [BenchmarkCategory("Benchmark"), Benchmark]
    public TomlDocument CsToml_Parse_DottedKeyMiss()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(missText);
        return document;
    }
}
