using CsToml.Error;
using System.Buffers;

namespace CsToml.Formatter;

public sealed class ValueTupleFormatter<T1> : ITomlValueFormatter<ValueTuple<T1>>
{
    public ValueTuple<T1> Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        var item = options.Resolver.GetFormatter<T1>()!.Deserialize(ref rootNode, options);
        if (item != null)
        {
            return new ValueTuple<T1>(item);
        }
        else
        {
            ExceptionHelper.ThrowDeserializationFailed(typeof(ValueTuple<T1>));
            return default!;
        }
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ValueTuple<T1> target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        options.Resolver.GetFormatter<T1>()!.Serialize(ref writer, target.Item1, options);
    }
}

public sealed class ValueTupleFormatter<T1, T2> : ITomlValueFormatter<ValueTuple<T1, T2>>
{
    public ValueTuple<T1, T2> Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetArray(out var value))
        {
            if (value.Count != 2)
            {
                ExceptionHelper.ThrowInvalidTupleCount();
                return default!;
            }

            var t1Node = rootNode[0];
            var t1 = options.Resolver.GetFormatter<T1>()!.Deserialize(ref t1Node, options);
            var t2Node = rootNode[1];
            var t2 = options.Resolver.GetFormatter<T2>()!.Deserialize(ref t2Node, options);

            return new ValueTuple<T1, T2>(t1, t2);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(ValueTuple<T1, T2>));
        return default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ValueTuple<T1, T2> target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.BeginArray();
        options.Resolver.GetFormatter<T1>()!.Serialize(ref writer, target.Item1, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T2>()!.Serialize(ref writer, target.Item2, options);
        writer.WriteSpace();
        writer.EndArray();
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3> : ITomlValueFormatter<ValueTuple<T1, T2, T3>>
{
    public ValueTuple<T1, T2, T3> Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetArray(out var value))
        {
            if (value.Count != 3)
            {
                ExceptionHelper.ThrowInvalidTupleCount();
                return default!;
            }

            var t1Node = rootNode[0];
            var t1 = options.Resolver.GetFormatter<T1>()!.Deserialize(ref t1Node, options);
            var t2Node = rootNode[1];
            var t2 = options.Resolver.GetFormatter<T2>()!.Deserialize(ref t2Node, options);
            var t3Node = rootNode[2];
            var t3 = options.Resolver.GetFormatter<T3>()!.Deserialize(ref t3Node, options);


            return new ValueTuple<T1, T2, T3>(t1, t2, t3);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(ValueTuple<T1, T2, T3>));
        return default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ValueTuple<T1, T2, T3> target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.BeginArray();
        options.Resolver.GetFormatter<T1>()!.Serialize(ref writer, target.Item1, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T2>()!.Serialize(ref writer, target.Item2, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T3>()!.Serialize(ref writer, target.Item3, options);
        writer.WriteSpace();
        writer.EndArray();
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4> : ITomlValueFormatter<ValueTuple<T1, T2, T3, T4>>
{
    public ValueTuple<T1, T2, T3, T4> Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetArray(out var value))
        {
            if (value.Count != 4)
            {
                ExceptionHelper.ThrowInvalidTupleCount();
                return default!;
            }

            var t1Node = rootNode[0];
            var t1 = options.Resolver.GetFormatter<T1>()!.Deserialize(ref t1Node, options);
            var t2Node = rootNode[1];
            var t2 = options.Resolver.GetFormatter<T2>()!.Deserialize(ref t2Node, options);
            var t3Node = rootNode[2];
            var t3 = options.Resolver.GetFormatter<T3>()!.Deserialize(ref t3Node, options);
            var t4Node = rootNode[3];
            var t4 = options.Resolver.GetFormatter<T4>()!.Deserialize(ref t4Node, options);


            return new ValueTuple<T1, T2, T3, T4>(t1, t2, t3, t4);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(ValueTuple<T1, T2, T3, T4>));
        return default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ValueTuple<T1, T2, T3, T4> target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.BeginArray();
        options.Resolver.GetFormatter<T1>()!.Serialize(ref writer, target.Item1, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T2>()!.Serialize(ref writer, target.Item2, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T3>()!.Serialize(ref writer, target.Item3, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T4>()!.Serialize(ref writer, target.Item4, options);
        writer.WriteSpace();
        writer.EndArray();
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5> : ITomlValueFormatter<ValueTuple<T1, T2, T3, T4, T5>>
{
    public ValueTuple<T1, T2, T3, T4, T5> Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetArray(out var value))
        {
            if (value.Count != 5)
            {
                ExceptionHelper.ThrowInvalidTupleCount();
                return default!;
            }

            var t1Node = rootNode[0];
            var t1 = options.Resolver.GetFormatter<T1>()!.Deserialize(ref t1Node, options);
            var t2Node = rootNode[1];
            var t2 = options.Resolver.GetFormatter<T2>()!.Deserialize(ref t2Node, options);
            var t3Node = rootNode[2];
            var t3 = options.Resolver.GetFormatter<T3>()!.Deserialize(ref t3Node, options);
            var t4Node = rootNode[3];
            var t4 = options.Resolver.GetFormatter<T4>()!.Deserialize(ref t4Node, options);
            var t5Node = rootNode[4];
            var t5 = options.Resolver.GetFormatter<T5>()!.Deserialize(ref t5Node, options);

            return new ValueTuple<T1, T2, T3, T4, T5>(t1, t2, t3, t4, t5);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(ValueTuple<T1, T2, T3, T4, T5>));
        return default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ValueTuple<T1, T2, T3, T4, T5> target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.BeginArray();
        options.Resolver.GetFormatter<T1>()!.Serialize(ref writer, target.Item1, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T2>()!.Serialize(ref writer, target.Item2, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T3>()!.Serialize(ref writer, target.Item3, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T4>()!.Serialize(ref writer, target.Item4, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T5>()!.Serialize(ref writer, target.Item5, options);
        writer.WriteSpace();
        writer.EndArray();
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5, T6> : ITomlValueFormatter<ValueTuple<T1, T2, T3, T4, T5, T6>>
{
    public ValueTuple<T1, T2, T3, T4, T5, T6> Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetArray(out var value))
        {
            if (value.Count != 6)
            {
                ExceptionHelper.ThrowInvalidTupleCount();
                return default!;
            }

            var t1Node = rootNode[0];
            var t1 = options.Resolver.GetFormatter<T1>()!.Deserialize(ref t1Node, options);
            var t2Node = rootNode[1];
            var t2 = options.Resolver.GetFormatter<T2>()!.Deserialize(ref t2Node, options);
            var t3Node = rootNode[2];
            var t3 = options.Resolver.GetFormatter<T3>()!.Deserialize(ref t3Node, options);
            var t4Node = rootNode[3];
            var t4 = options.Resolver.GetFormatter<T4>()!.Deserialize(ref t4Node, options);
            var t5Node = rootNode[4];
            var t5 = options.Resolver.GetFormatter<T5>()!.Deserialize(ref t5Node, options);
            var t6Node = rootNode[5];
            var t6 = options.Resolver.GetFormatter<T6>()!.Deserialize(ref t6Node, options);

            return new ValueTuple<T1, T2, T3, T4, T5, T6>(t1, t2, t3, t4, t5, t6);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(ValueTuple<T1, T2, T3, T4, T5, T6>));
        return default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ValueTuple<T1, T2, T3, T4, T5, T6> target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.BeginArray();
        options.Resolver.GetFormatter<T1>()!.Serialize(ref writer, target.Item1, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T2>()!.Serialize(ref writer, target.Item2, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T3>()!.Serialize(ref writer, target.Item3, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T4>()!.Serialize(ref writer, target.Item4, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T5>()!.Serialize(ref writer, target.Item5, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T6>()!.Serialize(ref writer, target.Item6, options);
        writer.WriteSpace();
        writer.EndArray();
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5, T6, T7> : ITomlValueFormatter<ValueTuple<T1, T2, T3, T4, T5, T6, T7>>
{
    public ValueTuple<T1, T2, T3, T4, T5, T6, T7> Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetArray(out var value))
        {
            if (value.Count != 7)
            {
                ExceptionHelper.ThrowInvalidTupleCount();
                return default!;
            }

            var t1Node = rootNode[0];
            var t1 = options.Resolver.GetFormatter<T1>()!.Deserialize(ref t1Node, options);
            var t2Node = rootNode[1];
            var t2 = options.Resolver.GetFormatter<T2>()!.Deserialize(ref t2Node, options);
            var t3Node = rootNode[2];
            var t3 = options.Resolver.GetFormatter<T3>()!.Deserialize(ref t3Node, options);
            var t4Node = rootNode[3];
            var t4 = options.Resolver.GetFormatter<T4>()!.Deserialize(ref t4Node, options);
            var t5Node = rootNode[4];
            var t5 = options.Resolver.GetFormatter<T5>()!.Deserialize(ref t5Node, options);
            var t6Node = rootNode[5];
            var t6 = options.Resolver.GetFormatter<T6>()!.Deserialize(ref t6Node, options);
            var t7Node = rootNode[6];
            var t7 = options.Resolver.GetFormatter<T7>()!.Deserialize(ref t7Node, options);

            return new ValueTuple<T1, T2, T3, T4, T5, T6, T7>(t1, t2, t3, t4, t5, t6, t7);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(ValueTuple<T1, T2, T3, T4, T5, T6, T7>));
        return default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ValueTuple<T1, T2, T3, T4, T5, T6, T7> target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.BeginArray();
        options.Resolver.GetFormatter<T1>()!.Serialize(ref writer, target.Item1, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T2>()!.Serialize(ref writer, target.Item2, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T3>()!.Serialize(ref writer, target.Item3, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T4>()!.Serialize(ref writer, target.Item4, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T5>()!.Serialize(ref writer, target.Item5, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T6>()!.Serialize(ref writer, target.Item6, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T7>()!.Serialize(ref writer, target.Item7, options);
        writer.WriteSpace();
        writer.EndArray();
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5, T6, T7, TRest> : ITomlValueFormatter<ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>>
    where TRest : struct
{
    public ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetArray(out var value))
        {
            if (value.Count != 8)
            {
                ExceptionHelper.ThrowInvalidTupleCount();
                return default!;
            }

            var t1Node = rootNode[0];
            var t1 = options.Resolver.GetFormatter<T1>()!.Deserialize(ref t1Node, options);
            var t2Node = rootNode[1];
            var t2 = options.Resolver.GetFormatter<T2>()!.Deserialize(ref t2Node, options);
            var t3Node = rootNode[2];
            var t3 = options.Resolver.GetFormatter<T3>()!.Deserialize(ref t3Node, options);
            var t4Node = rootNode[3];
            var t4 = options.Resolver.GetFormatter<T4>()!.Deserialize(ref t4Node, options);
            var t5Node = rootNode[4];
            var t5 = options.Resolver.GetFormatter<T5>()!.Deserialize(ref t5Node, options);
            var t6Node = rootNode[5];
            var t6 = options.Resolver.GetFormatter<T6>()!.Deserialize(ref t6Node, options);
            var t7Node = rootNode[6];
            var t7 = options.Resolver.GetFormatter<T7>()!.Deserialize(ref t7Node, options);
            var tRestNode = rootNode[7];
            var tRest = options.Resolver.GetFormatter<TRest>()!.Deserialize(ref tRestNode, options);

            return new ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>(t1, t2, t3, t4, t5, t6, t7, tRest);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>));
        return default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.BeginArray();
        options.Resolver.GetFormatter<T1>()!.Serialize(ref writer, target.Item1, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T2>()!.Serialize(ref writer, target.Item2, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T3>()!.Serialize(ref writer, target.Item3, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T4>()!.Serialize(ref writer, target.Item4, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T5>()!.Serialize(ref writer, target.Item5, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T6>()!.Serialize(ref writer, target.Item6, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<T7>()!.Serialize(ref writer, target.Item7, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        options.Resolver.GetFormatter<TRest>()!.Serialize(ref writer, target.Rest, options);
        writer.WriteSpace();
        writer.EndArray();
    }
}
