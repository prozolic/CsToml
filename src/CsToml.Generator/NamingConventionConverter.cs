using System.Text;

namespace CsToml.Generator;

internal static class NamingConventionConverter
{
    /// <summary>
    /// Converts a property name to the specified naming convention.
    /// </summary>
    public static string Convert(string propertyName, TomlNamingConvention convention)
    {
        if (string.IsNullOrEmpty(propertyName))
            return propertyName;

        return convention switch
        {
            TomlNamingConvention.None => propertyName,
            TomlNamingConvention.LowerCase => ConvertToLowerCase(propertyName),
            TomlNamingConvention.UpperCase => ConvertToUpperCase(propertyName),
            TomlNamingConvention.PascalCase => ConvertToPascalCase(propertyName),
            TomlNamingConvention.CamelCase => ConvertToCamelCase(propertyName),
            TomlNamingConvention.SnakeCase => ConvertToSnakeCase(propertyName),
            TomlNamingConvention.KebabCase => ConvertToKebabCase(propertyName),
            TomlNamingConvention.ScreamingSnakeCase => ConvertToScreamingSnakeCase(propertyName),
            TomlNamingConvention.ScreamingKebabCase => ConvertToScreamingKebabCase(propertyName),
            _ => propertyName
        };
    }

    private static string ConvertToLowerCase(string name)
    {
        return name.ToLowerInvariant();
    }

    private static string ConvertToUpperCase(string name)
    {
        return name.ToUpperInvariant();
    }

    private static string ConvertToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        // If already in snake_case or kebab-case, convert first
        if (name.Contains('_') || name.Contains('-'))
        {
            return ConvertDelimitedToPascalCase(name);
        }

        // Ensure first character is uppercase
        if (char.IsLower(name[0]))
        {
            return char.ToUpperInvariant(name[0]) + name.Substring(1);
        }

        return name;
    }

    private static string ConvertToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        // If already in snake_case or kebab-case, convert first
        if (name.Contains('_') || name.Contains('-'))
        {
            var pascalCase = ConvertDelimitedToPascalCase(name);
            return char.ToLowerInvariant(pascalCase[0]) + pascalCase.Substring(1);
        }

        // Ensure first character is lowercase
        if (char.IsUpper(name[0]))
        {
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }

        return name;
    }

    private static string ConvertToSnakeCase(string name)
    {
        return ConvertToDelimitedCase(name, '_', false);
    }

    private static string ConvertToKebabCase(string name)
    {
        return ConvertToDelimitedCase(name, '-', false);
    }

    private static string ConvertToScreamingSnakeCase(string name)
    {
        return ConvertToDelimitedCase(name, '_', true);
    }

    private static string ConvertToScreamingKebabCase(string name)
    {
        return ConvertToDelimitedCase(name, '-', true);
    }

    private static string ConvertToDelimitedCase(string name, char delimiter, bool uppercase)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var builder = new StringBuilder();
        bool previousWasUpper = false;
        bool previousWasDelimiter = false;

        for (int i = 0; i < name.Length; i++)
        {
            var current = name[i];

            // Skip existing delimiters
            if (current == '_' || current == '-')
            {
                if (builder.Length > 0 && !previousWasDelimiter)
                {
                    builder.Append(delimiter);
                    previousWasDelimiter = true;
                }
                previousWasUpper = false;
                continue;
            }

            // Insert delimiter before uppercase letters (except at the start)
            if (char.IsUpper(current))
            {
                // Add delimiter if:
                // - Not at the start
                // - Previous character wasn't uppercase (or was, but next is lowercase - for acronyms)
                // - Previous character wasn't already a delimiter
                if (builder.Length > 0 && !previousWasDelimiter)
                {
                    // Handle acronyms: "XMLParser" -> "xml_parser" not "x_m_l_parser"
                    if (!previousWasUpper || (i + 1 < name.Length && char.IsLower(name[i + 1])))
                    {
                        builder.Append(delimiter);
                    }
                }
                previousWasUpper = true;
            }
            else
            {
                previousWasUpper = false;
            }

            previousWasDelimiter = false;
            builder.Append(uppercase ? char.ToUpperInvariant(current) : char.ToLowerInvariant(current));
        }

        return builder.ToString();
    }

    private static string ConvertDelimitedToPascalCase(string name)
    {
        var builder = new StringBuilder();
        bool capitalizeNext = true;

        foreach (var ch in name)
        {
            if (ch == '_' || ch == '-')
            {
                capitalizeNext = true;
                continue;
            }

            if (capitalizeNext)
            {
                builder.Append(char.ToUpperInvariant(ch));
                capitalizeNext = false;
            }
            else
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
        }

        return builder.ToString();
    }
}
