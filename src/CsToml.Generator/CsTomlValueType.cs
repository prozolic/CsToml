﻿
namespace CsToml.Generator;

internal enum CsTomlValueType
{
    None = -1,
    KeyValue = 0,
    Array = 1,
    InlineTable = 2,
    Table = 3,
    ArrayOfTables = 4,
}
