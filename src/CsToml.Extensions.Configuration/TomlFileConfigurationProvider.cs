using Microsoft.Extensions.Configuration;

namespace CsToml.Extensions.Configuration;

public class TomlFileConfigurationProvider(FileConfigurationSource source) : FileConfigurationProvider(source)
{
    public override void Load(Stream stream)
    {
        Data = new TomlStreamConfigurationParser().Parse(stream);
    }
}
