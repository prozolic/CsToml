using CsToml.Error;
using CsToml.Values;
using System.Buffers;
using System.Numerics;

namespace CsToml.Formatter;

internal sealed class ComplexFormatter : ITomlValueFormatter<Complex>
{
    public static readonly ComplexFormatter Instance = new();

    public Complex Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.CanGetValue(TomlValueFeature.Array) && rootNode.Value is TomlArray array)
        {
            var real = array[0].GetDouble();
            var imaginary = array[1].GetDouble();
            return new Complex(real, imaginary);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(Complex));
        return default;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Complex target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.BeginArray();
        writer.WriteDouble(target.Real);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        writer.WriteDouble(target.Imaginary);
        writer.WriteSpace();
        writer.EndArray();
    }

}

internal sealed class NullableComplexFormatter : ITomlValueFormatter<Complex?>
{
    public static readonly NullableComplexFormatter Instance = new();

    public Complex? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return null!;
        }

        if (rootNode.CanGetValue(TomlValueFeature.Array) && rootNode.Value is TomlArray array)
        {
            var real = array[0].GetDouble();
            var imaginary = array[1].GetDouble();
            return new Complex(real, imaginary);
        }

        return null;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Complex? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target.HasValue)
        {
            var t = target.GetValueOrDefault();
            writer.BeginArray();
            writer.WriteDouble(t.Real);
            writer.Write(TomlCodes.Symbol.COMMA);
            writer.WriteSpace();
            writer.WriteDouble(t.Imaginary);
            writer.WriteSpace();
            writer.EndArray();
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(Complex?));
        }

    }

}

#if NET11_0_OR_GREATER

internal sealed class ComplexFormatter<T> : ITomlValueFormatter<Complex<T>>
    where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public Complex<T> Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.CanGetValue(TomlValueFeature.Array) && rootNode.Value is TomlArray tomlArray)
        {
            if (tomlArray.Count != 2)
            {
                ExceptionHelper.ThrowDeserializationFailed(typeof(Complex<T>));
                return default!;
            }

            var formatter = options.Resolver.GetFormatter<T>()!;
            var realNode = rootNode[0];
            var real = formatter.Deserialize(ref realNode, options);
            var imaginaryNode = rootNode[1];
            var imaginary = formatter.Deserialize(ref imaginaryNode, options);

            return new Complex<T>(real, imaginary);
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(Complex<T>));
        return default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Complex<T> target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        var formatter = options.Resolver.GetFormatter<T>()!;

        writer.BeginArray();
        formatter.Serialize(ref writer, target.Real, options);
        writer.Write(TomlCodes.Symbol.COMMA);
        writer.WriteSpace();
        formatter.Serialize(ref writer, target.Imaginary, options);
        writer.WriteSpace();
        writer.EndArray();
    }
}

#endif