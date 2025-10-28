namespace CsToml;

/// <summary>
/// Defines the naming convention for TOML property names during serialization.
/// </summary>
public enum TomlNamingConvention
{
    /// <summary>
    /// No transformation applied. Use the original property name.
    /// </summary>
    None = 0,

    /// <summary>
    /// Convert to lowercase (e.g., "MyProperty" -> "myproperty").
    /// </summary>
    LowerCase = 1,

    /// <summary>
    /// Convert to UPPERCASE (e.g., "MyProperty" -> "MYPROPERTY").
    /// </summary>
    UpperCase = 2,

    /// <summary>
    /// Convert to PascalCase (e.g., "myProperty" -> "MyProperty").
    /// </summary>
    PascalCase = 3,

    /// <summary>
    /// Convert to camelCase (e.g., "MyProperty" -> "myProperty").
    /// </summary>
    CamelCase = 4,

    /// <summary>
    /// Convert to snake_case (e.g., "MyProperty" -> "my_property").
    /// </summary>
    SnakeCase = 5,

    /// <summary>
    /// Convert to kebab-case (e.g., "MyProperty" -> "my-property").
    /// </summary>
    KebabCase = 6,

    /// <summary>
    /// Convert to SCREAMING_SNAKE_CASE (e.g., "MyProperty" -> "MY_PROPERTY").
    /// </summary>
    ScreamingSnakeCase = 7,

    /// <summary>
    /// Convert to SCREAMING-KEBAB-CASE (e.g., "MyProperty" -> "MY-PROPERTY").
    /// </summary>
    ScreamingKebabCase = 8
}
