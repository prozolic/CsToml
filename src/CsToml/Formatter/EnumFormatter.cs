using CsToml.Error;
using System.Buffers;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;

namespace CsToml.Formatter;

public sealed class EnumFormatter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum> : ITomlValueFormatter<TEnum>
    where TEnum : Enum
{
    private static readonly FrozenDictionary<TEnum, string>? SerializedEnumTable;
    private static readonly FrozenDictionary<string, TEnum>? DeserializedEnumTable;
    private static readonly FrozenDictionary<TEnum, string>? SerializedEnumMemberTable;
    private static readonly FrozenDictionary<string, TEnum>? DeserializedEnumMemberTable;

    static EnumFormatter()
    {
        var enumFields = typeof(TEnum).GetFields();
        var serializedEnumTable = new Dictionary<TEnum, string>(enumFields.Length);
        var deserializedEnumTable = new Dictionary<string, TEnum>(enumFields.Length);

        Dictionary<TEnum, string>? serializedEnumMemberTable = null;
        Dictionary<string, TEnum>? deserializedEnumMemberTable = null;

        foreach (var e in enumFields.AsSpan())
        {
            if (e.FieldType == typeof(TEnum))
            {
                var enumValue = (TEnum)e.GetValue(null)!;
                var enumValueString = e.Name;

                serializedEnumTable.Add(enumValue, e.Name);
                deserializedEnumTable.Add(e.Name, enumValue);

                if (e.GetCustomAttributes().OfType<EnumMemberAttribute>().FirstOrDefault() is { Value: { } enumMember })
                {
                    (serializedEnumMemberTable ??= new()).Add(enumValue, enumMember);
                    (deserializedEnumMemberTable ??= new()).Add(enumMember, enumValue);
                }
            }
        }

        SerializedEnumTable = serializedEnumTable.ToFrozenDictionary();
        DeserializedEnumTable = deserializedEnumTable.ToFrozenDictionary();
        SerializedEnumMemberTable = serializedEnumMemberTable?.ToFrozenDictionary();
        DeserializedEnumMemberTable = deserializedEnumMemberTable?.ToFrozenDictionary();
    }

    public TEnum Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetString(out var value))
        {
            if (DeserializedEnumMemberTable?.TryGetValue(value, out var enumMember) ?? false)
            {
                return enumMember;
            }
            if (DeserializedEnumTable?.TryGetValue(value, out var enumValue) ?? false)
            {
                return enumValue;
            }
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(TEnum));
        return default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TEnum target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (SerializedEnumMemberTable?.TryGetValue(target, out var enumMemberString) ?? false)
        {
            writer.WriteString(enumMemberString);
        }
        else if (SerializedEnumTable?.TryGetValue(target, out var enumValueString) ?? false)
        {
            writer.WriteString(enumValueString);
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(TEnum));
        }

    }
}
