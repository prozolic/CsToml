using CsToml.Utility;
using CsToml.Values;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CsToml.Error;

internal static class ExceptionHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T NotReturnThrow<T>(Action errorAction)
    {
        errorAction();
        return default!; // no return
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T NotReturnThrow<T, T2>(Action<T2> errorAction, T2 args)
    {
        errorAction(args);
        return default!; // no return
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowException(string argumentExceptionMessage)
    {
        throw new CsTomlException(argumentExceptionMessage);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowEndOfFileReached()
    {
        ThrowException($@"Reached end of file during processing.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowInvalidAdvance()
    {
        ThrowException($@"Cannot advance past the end of the buffer.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowInvalidByteIncluded()
    {
        ThrowException($@"Invalid byte was included.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowBufferTooSmallFailed()
    {
        ThrowException($@"Destination buffer is Too Small to fail.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectSyntax()
    {
        ThrowException($@"The Utf8 string was not syntactically valid.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowInvalidUnicodeScalarValue()
    {
        ThrowException($@"It contains Unicode code points outside the range U+0000 to U+10FFFF.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowDeserializationFailed(string message)
    {
        ThrowException($@"Deserialization failed: ");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowEscapeCharactersIncluded(byte unknownByte)
    {
        ThrowException($@"Escape characters {unknownByte} were included.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectCompactEscapeCharacters(byte unknownByte)
    {
        ThrowException($@"An invalid escape characters {unknownByte} were included.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowOverflowCount()
    {
        ThrowException($@"The set value has overflowed.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowNumericConversionFailed(byte unknownByte)
    {
        ThrowException($@"{unknownByte} is a character that cannot be converted to a number.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowInvalidCasting()
    {
        ThrowException($@"Failed to cast to numeric value.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectTomlFormat()
    {
        ThrowException($@"Failed due to incorrect formatting.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowConsecutiveQuotationMarksOf3()
    {
        ThrowException($@"Three or more quotation marks are written consecutively.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowConsecutiveSingleQuotationMarksOf3()
    {
        ThrowException($@"Three or more single quotes written consecutively.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowKeyIsRedefined(CsTomlString keyName)
    {
        ThrowException($@"Key '{keyName.Utf16String}' is already defined.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowTableHeaderIsRedefined(string keyName)
    {
        ThrowException($@"Table Header '{keyName}' is already defined.");
    }

    // Sub-table defined before parent of table or array of tables
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowOrderOfSubtableDefinitions(string tableName)
    {
        ThrowException($@"Sub-table '{tableName}' defined before parent of table or array of tables.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowUnderscoreUsedFirst()
    {
        ThrowException($@"Underscores are used first.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowUnderscoreUsedConsecutively()
    {
        ThrowException($@"Underscores are used consecutively.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowPeriodUsedMoreThanOnce()
    {
        ThrowException($@"Periods are used more than once.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowTheExponentPartUsedMoreThanOnce()
    {
        ThrowException($@"The exponent part e is used more than once.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowUnderscoreUsedAtTheEnd()
    {
        ThrowException($@"Underscores are used at the end.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowUnderscoreUsedWhereNotSurroundedByNumbers()
    {
        ThrowException($@"Underscores are used where they are not surrounded by numbers.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowPeriodUsedWhereNotSurroundedByNumbers()
    {
        ThrowException($@"Perid are used where they are not surrounded by numbers.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowExponentPartUsedWhereNotSurroundedByNumbers()
    {
        ThrowException($@"Exponent part are used where they are not surrounded by numbers.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowPeriodUsedFirst()
    {
        ThrowException($@"Period are used first.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectTomlLocalDateTimeFormat()
    {
        ThrowException($@"Failed due to incorrect local DateTime formatting.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectTomlLocalDateFormat()
    {
        ThrowException($@"Failed due to incorrect local Date formatting.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectTomlLocalTimeFormat()
    {
        ThrowException($@"Failed due to incorrect local Time formatting.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectTomlOffsetDateTimeFormat()
    {
        ThrowException($@"Failed due to incorrect offset DateTime formatting.");
    }

}

