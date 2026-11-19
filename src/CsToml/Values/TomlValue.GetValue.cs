using CsToml.Error;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace CsToml.Values;

public partial class TomlValue
{
    public ImmutableArray<TomlValue> GetArray()
    {
        if (!TryGetArray(out var value))
        {
            ExceptionHelper.ThrowTypeMismatch(Type, "array");
        }
        return value;
    }

    public ReadOnlySpan<TomlValue> GetArraySpan()
    {
        if (!TryGetArraySpan(out var value))
        {
            ExceptionHelper.ThrowTypeMismatch(Type, "array");
        }
        return value;
    }

    public string GetString()
    {
        if (!TryGetString(out var value))
        {
            ExceptionHelper.ThrowTypeMismatch(Type, "string");
        }
        return value;
    }

    public long GetInt64()
    {
        if (!TryGetInt64(out var value))
        {
            ExceptionHelper.ThrowTypeMismatch(Type, "long");
        }
        return value;
    }

    public double GetDouble()
    {
        if (!TryGetDouble(out var value))
        {
            ExceptionHelper.ThrowTypeMismatch(Type, "double");
        }
        return value;
    }

    public bool GetBool()
    {
        if (!TryGetBool(out var value))
        {
            ExceptionHelper.ThrowTypeMismatch(Type, "bool");
        }
        return value;
    }

    public DateTime GetDateTime()
    {
        if (!TryGetDateTime(out var value))
        {
            ExceptionHelper.ThrowTypeMismatch(Type, "DateTime");
        }
        return value;
    }

    public DateTimeOffset GetDateTimeOffset()
    {
        if (!TryGetDateTimeOffset(out var value))
        {
            ExceptionHelper.ThrowTypeMismatch(Type, "DateTimeOffset");
        }
        return value;
    }

    public DateOnly GetDateOnly()
    {
        if (!TryGetDateOnly(out var value))
        {
            ExceptionHelper.ThrowTypeMismatch(Type, "DateOnly");
        }
        return value;
    }

    public TimeOnly GetTimeOnly()
    {
        if (!TryGetTimeOnly(out var value))
        {
            ExceptionHelper.ThrowTypeMismatch(Type, "TimeOnly");
        }
        return value;
    }

    public virtual object GetObject()
        => ExceptionHelper.NotReturnThrow<object>(ExceptionHelper.ThrowNoValue);

    public virtual bool TryGetArray(out ImmutableArray<TomlValue> value)
    {
        value = ImmutableArray<TomlValue>.Empty;
        return false;
    }

    public virtual bool TryGetArraySpan(out ReadOnlySpan<TomlValue> value)
    {
        value = default;
        return false;
    }

    public virtual bool TryGetString([NotNullWhen(true)] out string? value)
    {
        value = default;
        return false;
    }

    public virtual bool TryGetInt64(out long value)
    {
        value = default;
        return false;
    }

    public virtual bool TryGetDouble(out double value)
    {
        value = default;
        return false;
    }

    public virtual bool TryGetBool(out bool value)
    {
        value = default;
        return false;
    }

    public virtual bool TryGetDateTime(out DateTime value)
    {
        value = default;
        return false;
    }

    public virtual bool TryGetDateTimeOffset(out DateTimeOffset value)
    {
        value = default;
        return false;
    }

    public virtual bool TryGetDateOnly(out DateOnly value)
    {
        value = default;
        return false;
    }

    public virtual bool TryGetTimeOnly(out TimeOnly value)
    {
        value = default;
        return false;
    }

    public bool TryGetObject([NotNullWhen(true)] out object? value)
    {
        if (HasValue)
        {
            value = GetObject();
            return true;
        }
        value = default;
        return false;
    }
}
