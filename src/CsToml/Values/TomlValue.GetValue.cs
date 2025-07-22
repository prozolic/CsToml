using CsToml.Error;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Numerics;

namespace CsToml.Values;

public partial class TomlValue
{
    public virtual bool CanGetValue(TomlValueFeature feature)
        => false;

    public virtual ReadOnlyCollection<TomlValue> GetArray()
        => ExceptionHelper.NotReturnThrow<ReadOnlyCollection<TomlValue>>(ExceptionHelper.ThrowInvalidCasting);

    public virtual TomlValue GetArrayValue(int index)
        => ExceptionHelper.NotReturnThrow<TomlValue>(ExceptionHelper.ThrowInvalidCasting);

    public virtual ImmutableArray<TomlValue> GetImmutableArray()
        => ExceptionHelper.NotReturnThrow<ImmutableArray<TomlValue>>(ExceptionHelper.ThrowInvalidCasting);

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
        var tempDocumentNode = new TomlDocumentNode(this);
        return tempDocumentNode.GetValue<T>();
    }

    public bool TryGetArray(out ReadOnlyCollection<TomlValue> value)
    {
        if (CanGetValue(TomlValueFeature.Array))
        {
            value = GetArray();
            return true;
        }
        value = default!;
        return false;
    }

    public bool TryGetArrayValue(int index, out TomlValue value)
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

    public bool TryGetImmutableArray(out ImmutableArray<TomlValue> value)
    {
        if (CanGetValue(TomlValueFeature.Array))
        {
            value = GetImmutableArray();
            return true;
        }
        value = default!;
        return false;
    }

    public bool TryGetString(out string value)
    {
        if (CanGetValue(TomlValueFeature.String))
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
        if (CanGetValue(TomlValueFeature.Int64))
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
        if (CanGetValue(TomlValueFeature.Double))
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
        if (CanGetValue(TomlValueFeature.Boolean))
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
        if (CanGetValue(TomlValueFeature.DateTime))
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
        if (CanGetValue(TomlValueFeature.DateTimeOffset))
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
        if (CanGetValue(TomlValueFeature.DateOnly))
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
        if (CanGetValue(TomlValueFeature.TimeOnly))
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
        if (CanGetValue(TomlValueFeature.Object))
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
        if (CanGetValue(TomlValueFeature.Number))
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

}

