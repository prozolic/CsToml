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
    internal static void ThrowException(string argumentExceptionMessage, Exception innerException)
    {
        throw new CsTomlException(argumentExceptionMessage, innerException);
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
    internal static void ThrowFailedToParseToNumeric()
    {
        ThrowException($@"Failed to parse to numeric type because it is not syntactically valid.");
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
    internal static void ThrowOverflowDuringParsingOfNumericTypes()
    {
        ThrowOverflowCountCore("numeric");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowOverflowCountCore(string type)
    {
        ThrowException($@"The set value has overflowed during parsing of {type} types.");
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
    internal static void ThrowNotSeparatedByCommas()
    {
        ThrowException($@"Values are not separated by commas.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowTheArrayIsNotClosedWithClosingBrackets()
    {
        ThrowException($@"The array is not closed with closing brackets.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowNotTurnIntoTable(string tableName)
    {
        ThrowException($@"Cannot be a table because the value is already defined in '{tableName}'");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowTheKeyIsDefinedAsTable()
    {
        ThrowException($@"The key is already defined as a table.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowTheKeyIsDefinedAsArrayOfTables()
    {
        ThrowException($@"Keys are already defined as an array of tables.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowTableArrayIsDefinedAsTable(string tableName)
    {
        ThrowException($@"The '{tableName}' table array is already defined as a table.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrect16bitCodePoint()
    {
        ThrowException($@"An incomplete '\uXXXX' escape sequence is specified.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrect32bitCodePoint()
    {
        ThrowException($@"An incomplete '\UXXXXXXXX' escape sequence is specified.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectPositivAndNegativeSigns()
    {
        ThrowException($@" '+' or '-' are included in incorrect positions.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowThreeOrMoreQuotationMarks()
    {
        ThrowException($@"Three or more quotation marks are used in a string.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowInvalidEscapeSequence()
    {
        ThrowException($@"An invalid escape sequence was included.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowNoEqualAfterTheKey()
    {
        ThrowException($@"There was no ""="" after key.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectFormattingOfFloatingNumbers()
    {
        ThrowException($@"Failed due to incorrect formatting of floating numbers.");
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
    internal static void ThrowKeyIsDefined(CsTomlString keyName)
    {
        ThrowException($@"Key '{keyName.Utf16String}' is already defined.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowTableHeaderIsDefined(string keyName)
    {
        ThrowException($@"Table Header '{keyName}' is already defined.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowTableHeaderIsDefinedAsTableArray(string keyName)
    {
        ThrowException($@"Table Header '{keyName}' is already defined as Array of Tables.");
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
    internal static void ThrowDecimalPointIsPresentAfterTheExponentialPartE()
    {
        ThrowException($@"A decimal point is present after the exponential part e");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowUnderscoreIsUsedAtTheEnd()
    {
        ThrowException($@"Underscores are used at the end.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowPeriodIsUsedAtTheEnd()
    {
        ThrowException($@"A period is used at the end.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowExponentPartIsUsedAtTheEnd()
    {
        ThrowException($@"Exponent part is used at the end.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowSignIsUsedAtTheEnd()
    {
        ThrowException($@"Sign is used at the end.");
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
    internal static void ThrowIncorrectTomlIntegerFormat()
    {
        ThrowException($@"Failed due to incorrect Integer formatting.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectTomlIntegerBinaryFormat()
    {
        ThrowException($@"Failed due to incorrect formatting of binary integers.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectTomlIntegerOctalFormat()
    {
        ThrowException($@"Failed due to incorrect formatting of octal integers.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectTomlIntegerHexadecimalFormat()
    {
        ThrowException($@"Failed due to incorrect formatting of hexadecimal integers.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectTomlFloatFormat()
    {
        ThrowException($@"Failed due to incorrect Float formatting.");
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

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectTomlInlineTableFormat()
    {
        ThrowException($@"Failed due to incorrect inline Table formatting.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowOverflowWhenCasting(Type t, Exception innerException)
    {
        ThrowException($"OverflowException occurred when casting from type long to type {t}.", innerException);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowArgumentOutOfRangeExceptionWhenCreating<T>(ArgumentOutOfRangeException innerException)
    {
        ThrowException($"ArgumentOutOfRangeException occurred when creating {typeof(T)}.", innerException);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowBasicStringsIsNotClosedWithClosingQuotationMarks()
    {
        ThrowException($@"Basic strings is not closed with closing quotation marks.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowMultilineBasicStringsIsNotClosedWithClosingThreeQuotationMarks()
    {
        ThrowException($@"Multi-line Basic strings is not closed with closing three quotation marks.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowLiteralStringsIsNotClosedWithClosingQuoted()
    {
        ThrowException($@"Literal strings is not closed with closing single quotes.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowMultilineLiteralStringsIsNotClosedWithThreeClosingQuoted()
    {
        ThrowException($@"Multi-line Literal strings is not closed with closing three single quotes.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowInvalidCodePoints()
    {
        ThrowException($@"This byte array contains invalid Unicode code points.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowBareKeyIsEmpty()
    {
        ThrowException($@"A bare key is empty.");
    }
}

