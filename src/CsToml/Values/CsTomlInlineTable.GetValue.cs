﻿

namespace CsToml.Values;

internal partial class CsTomlInlineTable
{
    public override bool CanGetValue(CsTomlValueFeature feature)
        => (CsTomlValueFeature.InlineTable & feature) == feature;

    public override CsTomlValue? Find(ReadOnlySpan<byte> keys, bool isDotKey = false)
        => inlineTable.Find(keys, isDotKey);

    public override CsTomlValue? Find(ReadOnlySpan<ByteArray> dottedKeys)
        => inlineTable.Find(dottedKeys);
}

