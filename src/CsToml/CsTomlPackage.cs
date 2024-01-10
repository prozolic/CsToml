﻿using CsToml.Values;
using CsToml.Error;
using CsToml.Utility;
using CsToml.Formatter;
using System.Collections.ObjectModel;

namespace CsToml;


public sealed class CsTomlPackageFactory : ICsTomlPackageFactory
{
    public static CsTomlPackage GetPackage() => new();
}

public class CsTomlPackage
{
    private CsTomlTable table = new ();
    private List<CsTomlException>? exceptions;

    internal CsTomlTableNode Node => table.RootNode;

    public long LineNumber { get; internal set; }

    public ReadOnlyCollection<CsTomlException>? Exceptions => exceptions?.AsReadOnly() ?? ReadOnlyCollection<CsTomlException>.Empty;

    public bool IsThrowCsTomlException { get; set; } = true;

    public CsTomlPackage()
    {
        table = new();
        LineNumber = 0;
    }

    public bool TryGetValue(ReadOnlySpan<byte> key, out CsTomlValue? value)
        => table.TryGetValue(key, out value);

    public bool TryGetValue(string key, out CsTomlValue? value)
    {
        using var writer = new ArrayPoolBufferWriter<byte>(128);
        var utf8Writer = new Utf8Writer(writer);
        StringFormatter.Serialize(ref utf8Writer, key);
        return table.TryGetValue(writer.WrittenSpan, out value);
    }

    internal bool TryAddKeyValue(CsTomlKey key, CsTomlValue value, CsTomlTableNode? node, IEnumerable<CsTomlString>? comments)
        => table.TryAddValue(key, value, node, comments);

    internal bool TryAddTableHeader(CsTomlKey key, out CsTomlTableNode? newNode, IEnumerable<CsTomlString>? comments)
        => table.TryAddTableHeader(key, out newNode, comments);

    internal bool TryAddTableArrayHeader(CsTomlKey key, out CsTomlTableNode? newNode, IEnumerable<CsTomlString>? comments)
        => table.TryAddTableArrayHeader(key, out newNode, comments);

    internal void AddException(CsTomlException exception)
    {
        exceptions ??= [];
        exceptions!.Add(exception);
    }

}

