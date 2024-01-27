﻿using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Runtime.InteropServices;

namespace CsToml;
public partial class CsTomlSerializer
{
    public static byte[] Serialize(ref CsTomlPackage package)
    {
        var writer = new ArrayBufferWriter<byte>();
        Serialize(writer, ref package);
        return writer.WrittenSpan.ToArray();
    }

    public static void Serialize(IBufferWriter<byte> bufferWriter, ref CsTomlPackage package)
    {
        var utf8Writer = new Utf8Writer(bufferWriter);
        package.TrySerialize(ref utf8Writer);
    }

}