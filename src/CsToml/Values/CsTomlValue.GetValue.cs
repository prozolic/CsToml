using CsToml.Error;
using CsToml.Formatter;
using CsToml.Utility;
using System.Buffers;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text.Unicode;

namespace CsToml.Values;

public partial class CsTomlValue
{
    public virtual bool CanGetValue(CsTomlValueFeature feature)
        => false;

    public virtual ReadOnlyCollection<CsTomlValue> GetArray()
        => ExceptionHelper.NotReturnThrow<ReadOnlyCollection<CsTomlValue>>(ExceptionHelper.ThrowInvalidCasting);

    public virtual CsTomlValue GetArrayValue(int index)
        => ExceptionHelper.NotReturnThrow<CsTomlValue>(ExceptionHelper.ThrowInvalidCasting);

    public virtual string GetString()
        => ExceptionHelper.NotReturnThrow<string>(ExceptionHelper.ThrowInvalidCasting);

    public virtual long GetInt64()
        => ExceptionHelper.NotReturnThrow<long>(ExceptionHelper.ThrowInvalidCasting);

    public virtual double GetDouble()
        => ExceptionHelper.NotReturnThrow<double>(ExceptionHelper.ThrowInvalidCasting);

    public virtual bool GetBool()
        => ExceptionHelper.NotReturnThrow<bool>(ExceptionHelper.ThrowInvalidCasting);

    public virtual DateTime GetDateTime()
        => ExceptionHelper.NotReturnThrow<DateTime>(ExceptionHelper.ThrowInvalidCasting);

    public virtual DateTimeOffset GetDateTimeOffset()
        => ExceptionHelper.NotReturnThrow<DateTimeOffset>(ExceptionHelper.ThrowInvalidCasting);

    public virtual DateOnly GetDateOnly()
        => ExceptionHelper.NotReturnThrow<DateOnly>(ExceptionHelper.ThrowInvalidCasting);

    public virtual TimeOnly GetTimeOnly()
        => ExceptionHelper.NotReturnThrow<TimeOnly>(ExceptionHelper.ThrowInvalidCasting);

    public virtual object GetObject()
        => ExceptionHelper.NotReturnThrow<TimeOnly>(ExceptionHelper.ThrowNoValue);

    public T GetNumber<T>() where T : struct, INumberBase<T>
    {
        try
        {
            return T.CreateChecked(GetDouble());
        }
        catch(OverflowException)
        {
            return ExceptionHelper.NotReturnThrow<T, Type>(ExceptionHelper.ThrowOverflow, typeof(T));
        }
        catch(NotSupportedException)
        {
            return ExceptionHelper.NotReturnThrow<T, string>(ExceptionHelper.ThrowNotSupported, nameof(T.CreateChecked));
        }
    }

    public T GetValue<T>()
    {
        var invoker = GetValueInvokerCache.Get<T>() ?? ExceptionHelper.NotReturnThrow<Func<CsTomlValue, T>>(ExceptionHelper.ThrowInvalidCasting);
        return invoker(this);
    }

    public virtual CsTomlValue? Find(ReadOnlySpan<byte> keys, bool isDottedKeys = false)
        => default;

    public CsTomlValue? Find(ReadOnlySpan<char> keys, bool isDottedKeys = false)
    {
        var maxBufferSize = (keys.Length + 1) * 3;
        if (maxBufferSize < 1024)
        {
            Span<byte> utf8Span = stackalloc byte[maxBufferSize];
            var status = Utf8.FromUtf16(keys, utf8Span, out int _, out int bytesWritten, replaceInvalidSequences: false);
            if (status != System.Buffers.OperationStatus.Done)
            {
                if (status == OperationStatus.InvalidData)
                    ExceptionHelper.ThrowInvalidByteIncluded();
                ExceptionHelper.ThrowBufferTooSmallFailed();
            }

            return Find(utf8Span[..bytesWritten], isDottedKeys);
        }
        else
        {
            var writer = new ArrayPoolBufferWriter<byte>(128);
            using var _ = writer;

            var utf8Writer = new Utf8Writer<ArrayPoolBufferWriter<byte>>(ref writer);
            try
            {
                ValueFormatter.Serialize(ref utf8Writer, keys);
            }
            catch (CsTomlException)
            {
                return default;
            }

            return Find(writer.WrittenSpan, isDottedKeys);
        }
    }

    public virtual CsTomlValue? Find(ReadOnlySpan<ByteArray> dottedKeys)
        => default;

    public bool TryGetArray(out ReadOnlyCollection<CsTomlValue> value)
    {
        if (CanGetValue(CsTomlValueFeature.Array))
        {
            value = GetArray();
            return true;
        }
        value = default!;
        return false;
    }

    public bool TryGetArrayValue(int index, out CsTomlValue value)
    {
        try
        {
            value = GetArrayValue(index);
            return true;
        }
        catch (CsTomlException)
        {
            value = default!;
            return false;
        }
    }

    public bool TryGetString(out string value)
    {
        if (CanGetValue(CsTomlValueFeature.String))
        {
            try
            {
                value = GetString();
                return true;
            }
            catch(CsTomlException)
            {
                value = default!;
                return false;
            }
        }
        value = default!;
        return false;
    }

    public bool TryGetInt64(out long value)
    {
        if (CanGetValue(CsTomlValueFeature.Int64))
        {
            try
            {
                value = GetInt64();
                return true;
            }
            catch (CsTomlException)
            {
                value = default!;
                return false;
            }
        }
        value = default!;
        return false;
    }

    public bool TryGetDouble(out double value)
    {
        if (CanGetValue(CsTomlValueFeature.Double))
        {
            try
            {
                value = GetDouble();
                return true;
            }
            catch (CsTomlException)
            {
                value = default!;
                return false;
            }
        }
        value = default!;
        return false;
    }

    public bool TryGetBool(out bool value)
    {
        if (CanGetValue(CsTomlValueFeature.Bool))
        {
            try
            {
                value = GetBool();
                return true;
            }
            catch (CsTomlException)
            {
                value = default!;
                return false;
            }
    }
        value = default!;
        return false;
    }

    public bool TryGetDateTime(out DateTime value)
    {
        if (CanGetValue(CsTomlValueFeature.DateTime))
        {
            try
            {
                value = GetDateTime();
                return true;
            }
            catch (CsTomlException)
            {
                value = default!;
                return false;
            }
        }
        value = default!;
        return false;
    }

    public bool TryGetDateTimeOffset(out DateTimeOffset value)
    {
        if (CanGetValue(CsTomlValueFeature.DateTimeOffset))
        {
            try
            {
                value = GetDateTimeOffset();
                return true;
            }
            catch (CsTomlException)
            {
                value = default!;
                return false;
            }
        }
        value = default!;
        return false;
    }

    public bool TryGetDateOnly(out DateOnly value)
    {
        if (CanGetValue(CsTomlValueFeature.DateOnly))
        {
            try
            {
                value = GetDateOnly();
                return true;
            }
            catch (CsTomlException)
            {
                value = default!;
                return false;
            }
        }
        value = default!;
        return false;
    }

    public bool TryGetTimeOnly(out TimeOnly value)
    {
        if (CanGetValue(CsTomlValueFeature.TimeOnly))
        {
            try
            {
                value = GetTimeOnly();
                return true;
            }
            catch (CsTomlException)
            {
                value = default!;
                return false;
            }
        }
        value = default!;
        return false;
    }

    public bool TryGetObject(out object value)
    {
        if (CanGetValue(CsTomlValueFeature.Object))
        {
            try
            {
                value = GetObject();
                return true;
            }
            catch (CsTomlException)
            {
                value = default!;
                return false;
            }
        }
        value = default!;
        return false;
    }

    public bool TryGetNumber<T>(out T value) where T : struct, INumberBase<T>
    {
        if (CanGetValue(CsTomlValueFeature.Number))
        {
            try
            {
                value = GetNumber<T>();
                return true;
            }
            catch (CsTomlException)
            {
                value = default!;
                return false;
            }
        }
        value = default!;
        return false;
    }

    public bool TryGetValue<T>(out T value)
    {
        if (CanGetValue(CsTomlValueFeature.Object))
        {
            try
            {
                value = GetValue<T>();
                return true;
            }
            catch (CsTomlException)
            {
                value = default!;
                return false;
            }
        }
        value = default!;
        return false;
    }

    public bool TryFind(ReadOnlySpan<byte> key, out CsTomlValue? value, bool isDottedKeys = false)
    {
        if (CanGetValue(CsTomlValueFeature.Table) || CanGetValue(CsTomlValueFeature.InlineTable))
        {
            value = Find(key, isDottedKeys);
            return value != default;
        }
        value = default;
        return false;
    }

    public bool TryFind(ReadOnlySpan<char> key, out CsTomlValue? value, bool isDottedKeys = false)
    {
        if (CanGetValue(CsTomlValueFeature.Table) || CanGetValue(CsTomlValueFeature.InlineTable))
        {
            value = Find(key, isDottedKeys);
            return value != default;
        }
        value = default;
        return false;
    }

    public bool TryFind(ReadOnlySpan<ByteArray> dottedKeys, out CsTomlValue? value)
    {
        if (CanGetValue(CsTomlValueFeature.Table) || CanGetValue(CsTomlValueFeature.InlineTable))
        {
            value = Find(dottedKeys);
            return value != default;
        }
        value = default;
        return false;
    }

}

