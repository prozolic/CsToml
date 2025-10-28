using Shouldly;

namespace CsToml.Generator.Tests.Seirialization;

public class NamingConventionTest
{
    [Fact]
    public void SnakeCase_SerializesCorrectly()
    {
        var type = new TypeSnakeCase
        {
            MyProperty = "test",
            AnotherValue = 42,
            XMLParser = "xml"
        };

        using var bytes = CsTomlSerializer.Serialize(type);
        var toml = System.Text.Encoding.UTF8.GetString(bytes.ByteSpan);

        toml.ShouldContain("my_property = \"test\"");
        toml.ShouldContain("another_value = 42");
        toml.ShouldContain("xml_parser = \"xml\"");
    }

    [Fact]
    public void SnakeCase_DeserializesCorrectly()
    {
        var toml = """
            my_property = "test"
            another_value = 42
            xml_parser = "xml"
            """u8;

        var result = CsTomlSerializer.Deserialize<TypeSnakeCase>(toml);

        result.ShouldNotBeNull();
        result.MyProperty.ShouldBe("test");
        result.AnotherValue.ShouldBe(42);
        result.XMLParser.ShouldBe("xml");
    }

    [Fact]
    public void KebabCase_SerializesCorrectly()
    {
        var type = new TypeKebabCase
        {
            MyProperty = "test",
            AnotherValue = 42
        };

        using var bytes = CsTomlSerializer.Serialize(type);
        var toml = System.Text.Encoding.UTF8.GetString(bytes.ByteSpan);

        toml.ShouldContain("my-property = \"test\"");
        toml.ShouldContain("another-value = 42");
    }

    [Fact]
    public void KebabCase_DeserializesCorrectly()
    {
        var toml = """
            my-property = "test"
            another-value = 42
            """u8;

        var result = CsTomlSerializer.Deserialize<TypeKebabCase>(toml);

        result.ShouldNotBeNull();
        result.MyProperty.ShouldBe("test");
        result.AnotherValue.ShouldBe(42);
    }

    [Fact]
    public void CamelCase_SerializesCorrectly()
    {
        var type = new TypeCamelCase
        {
            MyProperty = "test",
            AnotherValue = 42
        };

        using var bytes = CsTomlSerializer.Serialize(type);
        var toml = System.Text.Encoding.UTF8.GetString(bytes.ByteSpan);

        toml.ShouldContain("myProperty = \"test\"");
        toml.ShouldContain("anotherValue = 42");
    }

    [Fact]
    public void CamelCase_DeserializesCorrectly()
    {
        var toml = """
            myProperty = "test"
            anotherValue = 42
            """u8;

        var result = CsTomlSerializer.Deserialize<TypeCamelCase>(toml);

        result.ShouldNotBeNull();
        result.MyProperty.ShouldBe("test");
        result.AnotherValue.ShouldBe(42);
    }

    [Fact]
    public void PascalCase_SerializesCorrectly()
    {
        var type = new TypePascalCase
        {
            myProperty = "test",
            anotherValue = 42
        };

        using var bytes = CsTomlSerializer.Serialize(type);
        var toml = System.Text.Encoding.UTF8.GetString(bytes.ByteSpan);

        toml.ShouldContain("MyProperty = \"test\"");
        toml.ShouldContain("AnotherValue = 42");
    }

    [Fact]
    public void PascalCase_DeserializesCorrectly()
    {
        var toml = """
            MyProperty = "test"
            AnotherValue = 42
            """u8;

        var result = CsTomlSerializer.Deserialize<TypePascalCase>(toml);

        result.ShouldNotBeNull();
        result.myProperty.ShouldBe("test");
        result.anotherValue.ShouldBe(42);
    }

    [Fact]
    public void LowerCase_SerializesCorrectly()
    {
        var type = new TypeLowerCase
        {
            MyProperty = "test",
            AnotherValue = 42
        };

        using var bytes = CsTomlSerializer.Serialize(type);
        var toml = System.Text.Encoding.UTF8.GetString(bytes.ByteSpan);

        toml.ShouldContain("myproperty = \"test\"");
        toml.ShouldContain("anothervalue = 42");
    }

    [Fact]
    public void LowerCase_DeserializesCorrectly()
    {
        var toml = """
            myproperty = "test"
            anothervalue = 42
            """u8;

        var result = CsTomlSerializer.Deserialize<TypeLowerCase>(toml);

        result.ShouldNotBeNull();
        result.MyProperty.ShouldBe("test");
        result.AnotherValue.ShouldBe(42);
    }

    [Fact]
    public void UpperCase_SerializesCorrectly()
    {
        var type = new TypeUpperCase
        {
            MyProperty = "test",
            AnotherValue = 42
        };

        using var bytes = CsTomlSerializer.Serialize(type);
        var toml = System.Text.Encoding.UTF8.GetString(bytes.ByteSpan);

        toml.ShouldContain("MYPROPERTY = \"test\"");
        toml.ShouldContain("ANOTHERVALUE = 42");
    }

    [Fact]
    public void UpperCase_DeserializesCorrectly()
    {
        var toml = """
            MYPROPERTY = "test"
            ANOTHERVALUE = 42
            """u8;

        var result = CsTomlSerializer.Deserialize<TypeUpperCase>(toml);

        result.ShouldNotBeNull();
        result.MyProperty.ShouldBe("test");
        result.AnotherValue.ShouldBe(42);
    }

    [Fact]
    public void ScreamingSnakeCase_SerializesCorrectly()
    {
        var type = new TypeScreamingSnakeCase
        {
            MyProperty = "test",
            AnotherValue = 42
        };

        using var bytes = CsTomlSerializer.Serialize(type);
        var toml = System.Text.Encoding.UTF8.GetString(bytes.ByteSpan);

        toml.ShouldContain("MY_PROPERTY = \"test\"");
        toml.ShouldContain("ANOTHER_VALUE = 42");
    }

    [Fact]
    public void ScreamingSnakeCase_DeserializesCorrectly()
    {
        var toml = """
            MY_PROPERTY = "test"
            ANOTHER_VALUE = 42
            """u8;

        var result = CsTomlSerializer.Deserialize<TypeScreamingSnakeCase>(toml);

        result.ShouldNotBeNull();
        result.MyProperty.ShouldBe("test");
        result.AnotherValue.ShouldBe(42);
    }

    [Fact]
    public void ScreamingKebabCase_SerializesCorrectly()
    {
        var type = new TypeScreamingKebabCase
        {
            MyProperty = "test",
            AnotherValue = 42
        };

        using var bytes = CsTomlSerializer.Serialize(type);
        var toml = System.Text.Encoding.UTF8.GetString(bytes.ByteSpan);

        toml.ShouldContain("MY-PROPERTY = \"test\"");
        toml.ShouldContain("ANOTHER-VALUE = 42");
    }

    [Fact]
    public void ScreamingKebabCase_DeserializesCorrectly()
    {
        var toml = """
            MY-PROPERTY = "test"
            ANOTHER-VALUE = 42
            """u8;

        var result = CsTomlSerializer.Deserialize<TypeScreamingKebabCase>(toml);

        result.ShouldNotBeNull();
        result.MyProperty.ShouldBe("test");
        result.AnotherValue.ShouldBe(42);
    }

    [Fact]
    public void AliasOverridesConvention_SerializesCorrectly()
    {
        var type = new TypeAliasOverridesConvention
        {
            MyProperty = "test",
            AnotherValue = 42
        };

        using var bytes = CsTomlSerializer.Serialize(type);
        var toml = System.Text.Encoding.UTF8.GetString(bytes.ByteSpan);

        // MyProperty should be converted to snake_case
        toml.ShouldContain("my_property = \"test\"");

        // AnotherValue should use the explicit alias, not the convention
        toml.ShouldContain("custom_name = 42");
        toml.ShouldNotContain("another_value");
    }

    [Fact]
    public void AliasOverridesConvention_DeserializesCorrectly()
    {
        var toml = """
            my_property = "test"
            custom_name = 42
            """u8;

        var result = CsTomlSerializer.Deserialize<TypeAliasOverridesConvention>(toml);

        result.ShouldNotBeNull();
        result.MyProperty.ShouldBe("test");
        result.AnotherValue.ShouldBe(42);
    }
}
