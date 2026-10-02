#pragma warning disable CS8618

using System.Runtime.CompilerServices;

namespace CsToml.Generator.Tests
{
    public partial class GenericOuter<T>
    {
        [Union]
        public struct Inner
        {
            private readonly object? _value;

            public Inner(T value) => _value = value;
            public Inner(string value) => _value = value;

            public object? Value => _value;
        }
    }

    [TomlSerializedObject]
    public partial class NestedInGenericUnionHolder
    {
        [TomlValueOnSerialized]
        [TomlUnion<long>]
        public GenericOuter<long>.Inner Number { get; set; }

        [TomlValueOnSerialized]
        [TomlUnion<string>]
        public GenericOuter<long>.Inner Text { get; set; }
    }

    // "CsToml.Generator.Tests.Coll_Case" and "CsToml.Generator.Tests.Coll.Case" both sanitize to
    // "CsToml_Generator_Tests_Coll_Case".
    [TomlSerializedObject]
    public partial class Coll_Case
    {
        [TomlValueOnSerialized]
        public long Id { get; set; }
    }

    [Union]
    public struct CollisionUnion
    {
        private readonly object? _value;

        public CollisionUnion(Coll_Case value) => _value = value;
        public CollisionUnion(Coll.Case value) => _value = value;

        public object? Value => _value;
    }

    [TomlSerializedObject]
    public partial class CollisionUnionHolder
    {
        [TomlValueOnSerialized]
        [TomlUnion<Coll_Case>]
        public CollisionUnion First { get; set; }

        [TomlValueOnSerialized]
        [TomlUnion<Coll.Case>]
        public CollisionUnion Second { get; set; }
    }

    [TomlUnion<long>]
    [Union]
    public struct DeclaredOnlyProviderUnion
    {
        private readonly object? _value;

        public DeclaredOnlyProviderUnion(long value) => _value = value;
        public DeclaredOnlyProviderUnion(string value) => _value = value;

        public object? Value => _value;

        public interface IUnionMembers
        {
            static DeclaredOnlyProviderUnion Create(long value) => throw new InvalidOperationException("provider path must not be used");
            static DeclaredOnlyProviderUnion Create(string value) => throw new InvalidOperationException("provider path must not be used");
            object? Value { get; }
        }

        public bool TryGetValue(out long value)
        {
            if (_value is long l) { value = l; return true; }
            value = 0;
            return false;
        }

        public bool TryGetValue(out string value)
        {
            if (_value is string s) { value = s; return true; }
            value = "";
            return false;
        }
    }

    [TomlSerializedObject]
    public partial class DeclaredOnlyProviderUnionHolder
    {
        [TomlValueOnSerialized]
        public DeclaredOnlyProviderUnion Value { get; set; }
    }

    [Union]
    public struct ObjectBox<T>
    {
        private readonly object? _value;

        public ObjectBox(T value) => _value = value;
        public ObjectBox(string value) => _value = value;

        public object? Value => _value;
    }

    [TomlSerializedObject]
    public partial class ObjectBoxHolder
    {
        [TomlValueOnSerialized]
        [TomlUnion<string>]
        public ObjectBox<object> Text { get; set; }
    }
}

namespace CsToml.Generator.Tests.Coll
{
    [TomlSerializedObject]
    public partial class Case
    {
        [TomlValueOnSerialized]
        public string Name { get; set; }
    }
}
