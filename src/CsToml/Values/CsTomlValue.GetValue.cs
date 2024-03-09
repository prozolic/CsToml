using CsToml.Error;
using System.Collections.ObjectModel;
using System.Numerics;

namespace CsToml.Values;

public partial class CsTomlValue
{
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

    public T GetNumber<T>() where T : INumberBase<T>
    {
        var value = GetDouble();
        try
        {
            return T.CreateChecked(value);
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

    public bool TryGetArray(out IReadOnlyList<CsTomlValue> value)
    {
        try
        {
            value = GetArray();
            return true;
        }
        catch (CsTomlException)
        {
            value = default!;
            return false;
        }
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
        try
        {
            value = GetString();
            return true;
        }
        catch (CsTomlException)
        {
            value = default!;
            return false;
        }
    }

    public bool TryGetInt64(out long value)
    {
        try
        {
            value = GetInt64();
            return true;
        }
        catch (CsTomlException)
        {
            value = default;
            return false;
        }
    }

    public bool TryGetDouble(out double value)
    {
        try
        {
            value = GetDouble();
            return true;
        }
        catch (CsTomlException)
        {
            value = default;
            return false;
        }
    }

    public bool TryGetBool(out bool value)
    {
        try
        {
            value = GetBool();
            return true;
        }
        catch (CsTomlException)
        {
            value = default;
            return false;
        }
    }

    public bool TryGetDateTime(out DateTime value)
    {
        try
        {
            value = GetDateTime();
            return true;
        }
        catch (CsTomlException)
        {
            value = default;
            return false;
        }
    }

    public bool TryGetDateTimeOffset(out DateTimeOffset value)
    {
        try
        {
            value = GetDateTimeOffset();
            return true;
        }
        catch (CsTomlException)
        {
            value = default;
            return false;
        }
    }

    public bool TryGetDateOnly(out DateOnly value)
    {
        try
        {
            value = GetDateOnly();
            return true;
        }
        catch (CsTomlException)
        {
            value = default;
            return false;
        }
    }

    public bool TryGetTimeOnly(out TimeOnly value)
    {
        try
        {
            value = GetTimeOnly();
            return true;
        }
        catch (CsTomlException)
        {
            value = default;
            return false;
        }
    }

    public bool TryGetNumber<T>(out T value) where T : INumberBase<T>
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

}

