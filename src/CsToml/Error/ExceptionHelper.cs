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
    internal static void ThrowException(string argumentExceptionMessage)
    {
        throw new CsTomlException(argumentExceptionMessage);
    }

    [DoesNotReturn]
    internal static void ThrowException(string argumentExceptionMessage, Exception? innerException)
    {
        throw new CsTomlException(argumentExceptionMessage, innerException);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowNotSupported(string function)
    {
        ThrowException($"{function} is not supported.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowEndOfFileReached()
    {
        ThrowException($@"Reached end of file during processing.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowEndOfFileReachedAfterKey()
    {
        ThrowException($@"Reached end of file after key.");
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
        ThrowException($@"The value set is too large or too small.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowOverflowCount()
    {
        ThrowException($@"Value has overflowed.");
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
    internal static void ThrowNoValue()
    {
        ThrowException($@"There is no value.");
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
    internal static void ThrowInlineTableIsNotClosedWithClosingCurlyBrackets()
    {
        ThrowException($@"InlineTable is not closed with closing curly brackets.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowTableHeaderIsNotClosedWithClosingBrackets()
    {
        ThrowException($@"Table Header is not closed with closing brackets.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowArrayOfTablesHeaderIsNotClosedWithClosingBrackets()
    {
        ThrowException($@"Array Of Tables Header is not closed with closing brackets.");
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
    internal static void ThrowTheArrayOfTablesIsDefinedAsTable(string tableName)
    {
        ThrowException($@"The array of tables '{tableName}' is already defined as a table.");
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
    internal static void ThrowTheEndIsNotClosedInThreeSingleQuotationMarks()
    {
        ThrowException("The end is not enclosed in three single quotation marks");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowKeyIsDefined(TomlDottedKey keyName)
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
    internal static void ThrowTableHeaderIsDefinedAsArrayOfTables(string keyName)
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
    internal static void ThrowDotsAreUsedMoreThanOnce()
    {
        ThrowException($@"Dots are used more than once.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowTheDotIsDefinedFirst()
    {
        ThrowException($@"The dot is defined first.");
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
    internal static void ThrowDotIsUsedAtTheEnd()
    {
        ThrowException($@"Dot is used at the end.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowKeysAreNotJoinedByDots()
    {
        ThrowException($@"Keys are not joined by dots.");
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
    internal static void ThrowDotIsUsedWhereNotSurroundedByNumbers()
    {
        ThrowException($@"Dot is used where they are not surrounded by numbers.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowExponentPartUsedWhereNotSurroundedByNumbers()
    {
        ThrowException($@"Exponent part are used where they are not surrounded by numbers.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowDotIsUsedFirst()
    {
        ThrowException($@"Dot is used first.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectTomlBooleanFormat()
    {
        ThrowException($@"Failed due to incorrect Boolean formatting.");
    }


    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIncorrectTomlStringFormat()
    {
        ThrowException($@"Failed due to incorrect String formatting.");
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
    internal static void ThrowOverflow(Type t)
    {
        ThrowException($"value is not representable by {t}.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowArgumentOutOfRangeWhenOutsideTheBoundsOfTheArray()
    {
        ThrowException($"Index was outside the bounds of the array.");
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
    internal static void ThrowInvalidCodePoints()
    {
        ThrowException($@"This byte array contains invalid Unicode code points.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowBareKeyIsEmpty()
    {
        ThrowException($@"Bare keys is empty.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowBareKeyContainsInvalid(byte ch)
    {
        ThrowException($@"Bare keys contains an invalid string '{ch}'.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowKeyContainsInvalid(byte ch)
    {
        ThrowException($@"Key contains an invalid string '{ch}'.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowTableHeaderContainsInvalid(byte ch)
    {
        ThrowException($@"Table Header contains an invalid string '{ch}'.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowArrayOfTablesHeaderContainsInvalid(byte ch)
    {
        ThrowException($@"Array Of Tables Header contains an invalid string '{ch}'.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowNotRegisteredInResolver<T>()
    {
        ThrowNotRegisteredInResolver(typeof(T));
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowNotRegisteredInResolver(Type type)
    {
        ThrowException($@"{type.FullName} is not registered in Resolver.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowSerializationFailed(Type type)
    {
        ThrowException($@"Cannot serialize to {type}.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowSerializationFailedAsKey(Type type)
    {
        ThrowException($@"Cannot serialize to {type} as a key.");
    }

    internal static void ThrowDeserializationFailed(Type type)
    {
        ThrowException($@"Cannot deserialize to {type}.");
    }

    internal static void ThrowInvalidTupleCount()
    {
        ThrowException($@"Invalid Tuple count.");
    }
}

