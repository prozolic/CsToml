namespace CsToml.Generator;

/// <summary>
/// Defines the naming convention for TOML property names during serialization.
/// </summary>
internal enum TomlNamingConvention
{
    /// <summary>
    /// No transformation applied. Use the original property name.
    /// </summary>
    None,

    /// <summary>
    /// Convert to lowercase (e.g., "MyProperty" -> "myproperty").
    /// </summary>
    LowerCase,

    /// <summary>
    /// Convert to UPPERCASE (e.g., "MyProperty" -> "MYPROPERTY").
    /// </summary>
    UpperCase,

    /// <summary>
    /// Convert to PascalCase (e.g., "myProperty" -> "MyProperty").
    /// </summary>
    PascalCase,

    /// <summary>
    /// Convert to camelCase (e.g., "MyProperty" -> "myProperty").
    /// </summary>
    CamelCase,

    /// <summary>
    /// Convert to snake_case (e.g., "MyProperty" -> "my_property").
    /// </summary>
    SnakeCase,

    /// <summary>
    /// Convert to kebab-case (e.g., "MyProperty" -> "my-property").
    /// </summary>
    KebabCase,

    /// <summary>
    /// Convert to SCREAMING_SNAKE_CASE (e.g., "MyProperty" -> "MY_PROPERTY").
    /// </summary>
    ScreamingSnakeCase,

    /// <summary>
    /// Convert to SCREAMING-KEBAB-CASE (e.g., "MyProperty" -> "MY-PROPERTY").
    /// </summary>
    ScreamingKebabCase
}
