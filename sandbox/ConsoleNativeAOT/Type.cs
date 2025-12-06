#pragma warning disable CS8618

using CsToml;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace ConsoleNativeAOT;

public enum Color
{
    Red,
    Green,
    Blue
}

public partial struct TestStruct2
{
    public long Value { get; set; }

}

[TomlSerializedObject]
public partial class Sample
{
    [TomlValueOnSerialized]
    public Custom Key { get; set; }

}


public struct Custom
{
    public long Value { get; set; }

    public Custom(long value)
    {
        Value = value;
    }
    public Custom(string str)
    {
        if (long.TryParse(str.AsSpan(), out var value))
        {
            Value = value;
        }
        else
        {
            Value = 0;
        }
    }
}

[TomlSerializedObject]
public partial class TestData
{
    [TomlValueOnSerialized] public string E { get; set; }
    [TomlValueOnSerialized] public int EE { get; set; }
    [TomlValueOnSerialized] public long EEE { get; set; }
    [TomlValueOnSerialized] public float EEEE { get; set; }
    [TomlValueOnSerialized] public double EEEEE { get; set; }
}

[TomlSerializedObject]
internal partial class TypeImmutable
{
    [TomlValueOnSerialized]
    public ImmutableArray<TypeTable> ImmutableArray { get; set; }

    [TomlValueOnSerialized]
    public ImmutableList<TypeTable> ImmutableList { get; set; }

    [TomlValueOnSerialized]
    public IImmutableList<TypeTable> IImmutableList { get; set; }
}

[TomlSerializedObject]
internal partial class TypeTable
{
    [TomlValueOnSerialized]
    public string Value { get; set; }
}


[TomlSerializedObject(NamingConvention = TomlNamingConvention.KebabCase)]
internal partial class AliasName
{
    [TomlValueOnSerialized(AliasName = "alias")]
    public string? Key { get; set; }

    [TomlValueOnSerialized(AliasName = "あいうえおa")]
    public string? Hiragana { get; set; }
}

[TomlSerializedObject]
internal partial class GenericType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>
{
    [TomlValueOnSerialized]
    public T Value { get; set; }

    [TomlValueOnSerialized]
    public T? NullableValue { get; set; }
}

[TomlSerializedObject]
internal partial class Simple
{
    [TomlValueOnSerialized]
    public string Value { get; set; }
}

[TomlSerializedObject]
public partial class A
{
    [TomlValueOnSerialized]
    public B? B { get; set; } 

    [TomlValueOnSerialized]
    public int? Value { get; set; }
}

[TomlSerializedObject]
public partial class B
{
    [TomlValueOnSerialized]
    public string Name { get; set; }
}
