using Microsoft.Extensions.Configuration;

namespace CsToml.Extensions.Configuration;

public class TomlFileConfigurationSource : FileConfigurationSource
{
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        EnsureDefaults(builder);
        return new TomlFileConfigurationProvider(this);
    }
}
