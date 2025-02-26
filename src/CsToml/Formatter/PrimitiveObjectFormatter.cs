using CsToml.Error;
using CsToml.Formatter.Resolver;
using CsToml.Values;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CsToml.Formatter;

public sealed class PrimitiveObjectFormatter : ITomlValueFormatter<object>
{
    public static readonly PrimitiveObjectFormatter Instance = new PrimitiveObjectFormatter();

    private static readonly Dictionary<Type, int> TypeToJumpCode = new()
    {
        { typeof(bool), 0 },
        { typeof(byte), 1 },
        { typeof(sbyte), 2 },
        { typeof(char), 3 },
        { typeof(short), 4 },
        { typeof(int), 5 },
        { typeof(long), 6 },
        { typeof(ushort), 7 },
        { typeof(uint), 8 },
        { typeof(ulong), 9 },
        { typeof(float), 10 },
        { typeof(double), 11 },
        { typeof(string), 12 },
        { typeof(DateTime), 13 },
        { typeof(DateTimeOffset), 14 },
        { typeof(DateOnly), 15 },
        { typeof(TimeOnly), 16 },
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetJumpCode(Type type, out int jumpCode)
        => TypeToJumpCode.TryGetValue(type, out jumpCode);

    public object Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.Value.HasValue)
        {
            var value = rootNode.Value;
            if (value is TomlBoolean boolValue)
            {
                return boolValue.GetBool();
            }
            else if (value is TomlInteger intValue)
            {
                return intValue.GetInt64();
            }
            else if (value is TomlFloat doubleValue)
            {
                return doubleValue.GetDouble();
            }
            else if (value is TomlString stringValue)
            {
                return stringValue.GetString();
            }
            else if (value is TomlOffsetDateTime dateTimeOffsetValue)
            {
                return dateTimeOffsetValue.GetDateTimeOffset();
            }
            else if (value is TomlLocalDateTime dateTimeValue)
            {
                return dateTimeValue.GetDateTime();
            }
            else if (value is TomlLocalDate dateOnlyValue)
            {
                return dateOnlyValue.GetDateOnly();
            }
            else if (value is TomlLocalTime timeOnlyValue)
            {
                return timeOnlyValue.GetTimeOnly();
            }
            else if (value is TomlArray arrayValue)
            {
                ITomlValueFormatter<object> formatter = options.Resolver.GetFormatter<object>()!;

                var array = new object[arrayValue.Count];
                var arraySpan = array.AsSpan();
                for (int i = 0; i < arraySpan.Length; i++)
                {
                    var arrayValueNode = rootNode[i];
                    arraySpan[i] = formatter.Deserialize(ref arrayValueNode, options);
                }
                return array;
            }
            else if (value is TomlTable tableValue)
            {
                return tableValue.GetDictionary();
            }
            else if (value is TomlInlineTable inlineTableValue)
            {
                return inlineTableValue.GetDictionary();
            }
            else if (value is TomlDottedKey dottedKey)
            {
                return dottedKey.GetString();
            }
            else
            {
                ExceptionHelper.ThrowNotRegisteredInResolver(value.GetType());
                return default;
            }
        }
        else
        {
            return options.Resolver.GetFormatter<IDictionary<object, object>>()!.Deserialize(ref rootNode, options);
        }
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, object target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (TypeToJumpCode.TryGetValue(target.GetType(), out var jumpCode))
        {
            switch(jumpCode)
            {
                case 0:
                    writer.WriteBoolean((bool)target);
                    break;
                case 1:
                    writer.WriteInt64((byte)target);
                    break;
                case 2:
                    writer.WriteInt64((sbyte)target);
                    break;
                case 3:
                    writer.WriteInt64((char)target);
                    break;
                case 4:
                    writer.WriteInt64((short)target);
                    break;
                case 5:
                    writer.WriteInt64((int)target);
                    break;
                case 6:
                    writer.WriteInt64((long)target);
                    break;
                case 7:
                    writer.WriteInt64((ushort)target);
                    break;
                case 8:
                    writer.WriteInt64((uint)target);
                    break;
                case 9:
                    writer.WriteInt64(checked((long)target));
                    break;
                case 10:
                    writer.WriteDouble((float)target);
                    break;
                case 11:
                    writer.WriteDouble((double)target);
                    break;
                case 12:
                    writer.WriteString((string)target);
                    break;
                case 13:
                    writer.WriteDateTime((DateTime)target);
                    break;
                case 14:
                    writer.WriteDateTimeOffset((DateTimeOffset)target);
                    break;
                case 15:
                    writer.WriteDateOnly((DateOnly)target);
                    break;
                case 16:
                    writer.WriteTimeOnly((TimeOnly)target);
                    break;
            }
            return;
        }

        if (target is IDictionary dictionary)
        {
            var tempDict = new Dictionary<object, object?>(dictionary.Count);
            foreach (DictionaryEntry entry in dictionary)
            {
                tempDict[entry.Key] = entry.Value;
            }
            options.Resolver.GetFormatter<IDictionary<object, object?>>()!.Serialize(ref writer, tempDict!, options);
            return;
        }

        if (target is ICollection collection)
        {
            var formatter = options.Resolver.GetFormatter<object>();
            writer.BeginArray();
            var en = collection.GetEnumerator();
            using var _ = en as IDisposable;

            if (!en.MoveNext())
            {
                writer.EndArray();
                return;
            }
            formatter!.Serialize(ref writer, en.Current, options);
            if (!en.MoveNext())
            {
                writer.EndArray();
                return;
            }
            do
            {
                writer.Write(TomlCodes.Symbol.COMMA);
                writer.WriteSpace();
                formatter!.Serialize(ref writer, en.Current, options);

            } while (en.MoveNext());
            writer.EndArray();
            return;
        }

        ExceptionHelper.ThrowSerializationFailedAsKey(target.GetType());
    }

}

