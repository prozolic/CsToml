using CsToml.Error;
using CsToml.Extension;
using CsToml.Utility;
using System.Buffers;

namespace CsToml.Values;

public partial class CsTomlValue
{
    public string GetString()
    {
        var bufferWriter = new ArrayPoolBufferWriter<byte>();
        var utf8Writer = new Utf8Writer(bufferWriter);

        return ExceptionHelper.NotReturnThrow<string>(ExceptionHelper.ThrowInvalidCasting);
    }

    public long GetInt64()
    {
        if (Type == CsTomlType.Integar) return (this as CsTomlInt64)!.Value;
        else if (Type == CsTomlType.Float) return (long)(this as CsTomlDouble)!.Value;
        else if (Type == CsTomlType.Boolean) return (this as CsTomlBool)!.Value ? 1 : 0;

        return ExceptionHelper.NotReturnThrow<long>(ExceptionHelper.ThrowInvalidCasting);
    }

    public double GetDouble()
    {
        if (Type == CsTomlType.Float) return (this as CsTomlDouble)!.Value;
        else if (Type == CsTomlType.Integar) return (this as CsTomlInt64)!.Value;
        else if (Type == CsTomlType.Boolean) return (this as CsTomlBool)!.Value ? 1d : 0d;

        return ExceptionHelper.NotReturnThrow<double>(ExceptionHelper.ThrowInvalidCasting);
    }

    public bool GetBool()
    {
        if (Type == CsTomlType.Boolean) return (this as CsTomlBool)!.Value;
        else if (Type == CsTomlType.Integar) return (this as CsTomlInt64)!.Value != 0;
        else if (Type == CsTomlType.Float) return (this as CsTomlDouble)!.Value != 0d;

        return ExceptionHelper.NotReturnThrow<bool>(ExceptionHelper.ThrowInvalidCasting);
    }

    public DateTime GetDateTime()
    {
        if (Type == CsTomlType.LocalDateTime) 
            return (this as CsTomlLocalDateTime)!.Value;
        else if (Type == CsTomlType.LocalDate) 
            return (this as CsTomlLocalDate)!.Value.ToLocalDateTime();
        else if (Type == CsTomlType.LocalTime) 
            return (this as CsTomlLocalTime)!.Value.ToLocalDateTime();
        else if (Type == CsTomlType.OffsetDateTime || Type == CsTomlType.OffsetDateTimeByNumber) 
            return (this as CsTomlOffsetDateTime)!.Value.DateTime;

        return ExceptionHelper.NotReturnThrow<DateTime>(ExceptionHelper.ThrowInvalidCasting);
    }

    public DateTimeOffset GetDateTimeOffset()
    {
        if (Type == CsTomlType.OffsetDateTime || Type == CsTomlType.OffsetDateTimeByNumber) 
            return (this as CsTomlOffsetDateTime)!.Value.DateTime;
        else if (Type == CsTomlType.OffsetDateTimeByNumber) 
            return (this as CsTomlOffsetDateTime)!.Value.DateTime;
        else if (Type == CsTomlType.LocalDateTime)
        {
            CsTomlLocalDateTime localDateTimeValue = (this as CsTomlLocalDateTime)!;
            return DateTime.SpecifyKind(localDateTimeValue.Value, localDateTimeValue.Value.Kind);
        }
        else if (Type == CsTomlType.LocalDate)
            return (this as CsTomlLocalDate)!.Value.ToLocalDateTime();

        return ExceptionHelper.NotReturnThrow<DateTimeOffset>(ExceptionHelper.ThrowInvalidCasting);
    }

    public DateOnly GetDateOnly()
    {
        if (Type == CsTomlType.LocalDate)
            return (this as CsTomlLocalDate)!.Value;
        else if (Type == CsTomlType.LocalDateTime)
            return DateOnly.FromDateTime((this as CsTomlLocalDateTime)!.Value);
        else if (Type == CsTomlType.OffsetDateTime || Type == CsTomlType.OffsetDateTimeByNumber)
            return DateOnly.FromDateTime((this as CsTomlOffsetDateTime)!.Value.DateTime);

        return ExceptionHelper.NotReturnThrow<DateOnly>(ExceptionHelper.ThrowInvalidCasting);
    }

    public TimeOnly GetTimeOnly()
    {
        if (Type == CsTomlType.LocalTime)
            return (this as CsTomlLocalTime)!.Value;
        else if (Type == CsTomlType.LocalDate)
            return TimeOnly.MinValue;
        else if (Type == CsTomlType.LocalDateTime)
            return TimeOnly.FromDateTime((this as CsTomlLocalDateTime)!.Value);
        else if (Type == CsTomlType.OffsetDateTime || Type == CsTomlType.OffsetDateTimeByNumber)
            return TimeOnly.FromDateTime((this as CsTomlOffsetDateTime)!.Value.DateTime);

        return ExceptionHelper.NotReturnThrow<TimeOnly>(ExceptionHelper.ThrowInvalidCasting);
    }

}

