#pragma warning disable CS8618

using System.Runtime.CompilerServices;

namespace CsToml.Generator.Tests;

[TomlUnion<int>]
[Union]
public struct StringOrInt
{
    private readonly object? _value;

    public StringOrInt(string value) => _value = value;
    public StringOrInt(int value) => _value = value;

    public object? Value => _value;
}

[TomlUnion<long>]
[Union]
public struct TriState
{
    private readonly byte _tag;
    private readonly long _longValue;
    private readonly string? _stringValue;

    public TriState(long value)
    {
        _tag = 1;
        _longValue = value;
        _stringValue = null;
    }

    public TriState(string value)
    {
        _tag = 2;
        _longValue = 0;
        _stringValue = value;
    }

    public bool HasValue => _tag != 0;

    public object? Value => _tag switch { 1 => _longValue, 2 => _stringValue, _ => null };

    public bool TryGetValue(out long value)
    {
        value = _longValue;
        return _tag == 1;
    }

    public bool TryGetValue(out string value)
    {
        value = _stringValue!;
        return _tag == 2;
    }
}

[TomlUnion<long>]
[Union]
public partial struct ProvidedUnion : ProvidedUnion.IUnionMembers
{
    private readonly object? _value;

    private ProvidedUnion(object? value) => _value = value;

    public interface IUnionMembers
    {
        static ProvidedUnion Create(long value) => new(value);
        static ProvidedUnion Create(string value) => new(value);
        object? Value { get; }
    }

    object? IUnionMembers.Value => _value;
}

[TomlUnion<string>]
[Union]
public sealed class ClassUnion
{
    private readonly object? _value;

    public ClassUnion(long value) => _value = value;
    public ClassUnion(string value) => _value = value;

    public object? Value => _value;
}

[TomlUnion<StringOrInt>]
[Union]
public struct OuterUnion
{
    private readonly object? _value;

    public OuterUnion(StringOrInt value) => _value = value;
    public OuterUnion(bool value) => _value = value;

    public object? Value => _value;
}

[Union]
public struct ListOrText<T>
{
    private readonly object? _value;

    public ListOrText(List<T> value) => _value = value;
    public ListOrText(string value) => _value = value;

    public object? Value => _value;
}

[TomlSerializedObject]
public partial class PinCaseA
{
    [TomlValueOnSerialized]
    public int Number { get; set; }
}

[TomlUnion<PinCaseA>]
[Union]
public struct PinnedUnion
{
    private readonly object? _value;

    public PinnedUnion(PinCaseA value) => _value = value;
    public PinnedUnion(long value) => _value = value;

    public object? Value => _value;
}

[Union]
public struct UnpinnedUnion
{
    private readonly object? _value;

    public UnpinnedUnion(long value) => _value = value;
    public UnpinnedUnion(string value) => _value = value;

    public object? Value => _value;
}

[TomlUnion<long>]
[Union]
public struct StandaloneUnion
{
    private readonly object? _value;

    public StandaloneUnion(long value) => _value = value;
    public StandaloneUnion(string value) => _value = value;

    public object? Value => _value;
}

[TomlUnion<long>]
[Union]
public struct FirstWinsUnion
{
    private readonly object? _value;

    public FirstWinsUnion(long value) => _value = value;
    public FirstWinsUnion(string value) => _value = value;

    public object? Value => _value;
}

[TomlSerializedObject]
public partial class UnionHolder
{
    [TomlValueOnSerialized]
    public string? Name { get; set; }

    [TomlValueOnSerialized]
    public StringOrInt Value { get; set; }
}

[TomlSerializedObject]
public partial class TriStateHolder
{
    [TomlValueOnSerialized]
    public TriState State { get; set; }
}

[TomlSerializedObject]
public partial class ProvidedUnionHolder
{
    [TomlValueOnSerialized]
    public ProvidedUnion Value { get; set; }
}

[TomlSerializedObject]
public partial class ClassUnionHolder
{
    [TomlValueOnSerialized(NullHandling = TomlNullHandling.Ignore)]
    public ClassUnion? Value { get; set; }
}

[TomlSerializedObject]
public partial class OuterUnionHolder
{
    [TomlValueOnSerialized]
    public OuterUnion Value { get; set; }
}

[TomlSerializedObject]
public partial class GenericUnionHolder
{
    [TomlValueOnSerialized]
    [TomlUnion<List<long>>]
    public ListOrText<long> Value { get; set; }
}

[TomlSerializedObject]
public partial class PinnedUnionHolder
{
    [TomlValueOnSerialized]
    public PinnedUnion Value { get; set; }
}

[TomlSerializedObject]
public partial class MemberPinnedHolder
{
    [TomlValueOnSerialized]
    [TomlUnion<long>]
    public UnpinnedUnion Number { get; set; }

    [TomlValueOnSerialized]
    [TomlUnion<string>]
    public UnpinnedUnion Text { get; set; }

    [TomlValueOnSerialized]
    [TomlUnion<string>]
    public StringOrInt AsText { get; set; }
}

[TomlSerializedObject]
public partial class UnionCollectionsHolder
{
    [TomlValueOnSerialized]
    public StringOrInt[]? Items { get; set; }

    [TomlValueOnSerialized]
    public List<StringOrInt>? ListItems { get; set; }

    [TomlValueOnSerialized]
    public Dictionary<string, StringOrInt>? Map { get; set; }

    [TomlValueOnSerialized(NullHandling = TomlNullHandling.Ignore)]
    public StringOrInt? MaybeValue { get; set; }
}
