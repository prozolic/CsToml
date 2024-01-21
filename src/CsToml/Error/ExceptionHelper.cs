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
    internal static void ThrowInvalidAdvance()
    {
        throw new CsTomlException($@"Cannot advance past the end of the buffer.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectSyntax()
    {
        throw new CsTomlException($@"The Utf8 string was not syntactically valid.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowInvalidUnicodeScalarValue()
    {
        throw new CsTomlException($@"It contains Unicode code points outside the range U+0000 to U+10FFFF.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowDeserializationFailed(string message)
    {
        throw new CsTomlException($@"Deserialization failed: ");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowEscapeCharactersIncluded(byte unknownByte)
    {
        throw new CsTomlException($@"Escape characters {unknownByte} were included.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectCompactEscapeCharacters(byte unknownByte)
    {
        throw new CsTomlException($@"An invalid escape characters {unknownByte} were included.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowOverflowCount()
    {
        throw new CsTomlException($@"The set value has overflowed.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowNumericConversionFailed(byte unknownByte)
    {
        throw new CsTomlException($@"{unknownByte} is a character that cannot be converted to a number.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowInvalidCasting()
    {
        throw new CsTomlException($@"Failed to cast to numeric value.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectTomlFormat()
    {
        throw new CsTomlException($@"Failed due to incorrect formatting.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowConsecutiveQuotationMarksOf3()
    {
        throw new CsTomlException($@"Three or more quotation marks are written consecutively.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowConsecutiveSingleQuotationMarksOf3()
    {
        throw new CsTomlException($@"Three or more single quotes written consecutively.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowKeyIsRedefined(CsTomlString keyName)
    {
        throw new CsTomlException($@"Key '{keyName.Utf16String}' is already defined.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowTableHeaderIsRedefined(string keyName)
    {
        throw new CsTomlException($@"Table Header '{keyName}' is already defined.");
    }

    // Sub-table defined before parent of table or array of tables
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowOrderOfSubtableDefinitions(string tableName)
    {
        throw new CsTomlException($@"Sub-table '{tableName}' defined before parent of table or array of tables.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowUnderscoreUsedFirst()
    {
        throw new CsTomlException($@"Underscores are used first.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowUnderscoreUsedConsecutively()
    {
        throw new CsTomlException($@"Underscores are used consecutively.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowUnderscoreUsedAtTheEnd()
    {
        throw new CsTomlException($@"Underscores are used at the end.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowUnderscoreUsedWhereNotSurroundedByNumbers()
    {
        throw new CsTomlException($@"Underscores are used where they are not surrounded by numbers.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowPeriodUsedWhereNotSurroundedByNumbers()
    {
        throw new CsTomlException($@"Perid are used where they are not surrounded by numbers.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowExponentPartUsedWhereNotSurroundedByNumbers()
    {
        throw new CsTomlException($@"Exponent part are used where they are not surrounded by numbers.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowPeriodUsedFirst()
    {
        throw new CsTomlException($@"Period are used first.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectTomlLocalDateTimeFormat()
    {
        throw new CsTomlException($@"Failed due to incorrect local DateTime formatting.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectTomlLocalDateFormat()
    {
        throw new CsTomlException($@"Failed due to incorrect local Date formatting.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectTomlLocalTimeFormat()
    {
        throw new CsTomlException($@"Failed due to incorrect local Time formatting.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectTomlOffsetDateTimeFormat()
    {
        throw new CsTomlException($@"Failed due to incorrect offset DateTime formatting.");
    }

}

