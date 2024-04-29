using CsToml.Formatter;
using CsToml.Utility;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlKey: {DotKeys}")]
internal sealed class CsTomlKey : CsTomlValue
{
    private readonly List<CsTomlString> dotKeys = new(2);

    public IReadOnlyList<CsTomlString> DotKeys => dotKeys;

    public ReadOnlySpan<CsTomlString> DotKeysSpan => CollectionsMarshal.AsSpan(dotKeys);

    public override bool HasValue => true;

    internal CsTomlKey() : base(){}

    internal string GetJoinName()
    {
        var bufferWriter = new ArrayPoolBufferWriter<byte>();
        using var _ = bufferWriter;
        var writer = new Utf8Writer<ArrayPoolBufferWriter<byte>>(ref bufferWriter);
        var dotKeysSpan = CollectionsMarshal.AsSpan(dotKeys);
        for (int i = 0; i < dotKeysSpan.Length; i++)
        {
            dotKeysSpan[i].ToTomlString(ref writer);
            if (i < dotKeysSpan.Length - 1)
                writer.Write(CsTomlSyntax.Symbol.PERIOD);
        }

        var tempReader = new Utf8Reader(bufferWriter.WrittenSpan);
        ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out string value);
        return value;
    }

    public void Add(CsTomlString key)
        => dotKeys.Add(key);
}
