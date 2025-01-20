using Microsoft.Extensions.Configuration;

namespace CsToml.Extensions.Configuration;

public static class TomlConfigurationExtensions
{
    public static IConfigurationBuilder AddTomlFile(this IConfigurationBuilder builder, string path)
        => AddTomlFile(builder, provider: null, path: path, optional: false, reloadOnChange: false);

    public static IConfigurationBuilder AddTomlFile(this IConfigurationBuilder builder, string path, bool optional)
        => AddTomlFile(builder, provider: null, path: path, optional: optional, reloadOnChange: false);

    public static IConfigurationBuilder AddTomlFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        => AddTomlFile(builder, provider: null, path: path, optional: optional, reloadOnChange: reloadOnChange);

    public static IConfigurationBuilder AddTomlFile(this IConfigurationBuilder builder, Microsoft.Extensions.FileProviders.IFileProvider? provider, string path, bool optional, bool reloadOnChange)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddTomlFile((c) =>
        {
            c.FileProvider = provider;
            c.Path = path;
            c.Optional = optional;
            c.ReloadOnChange = reloadOnChange;
            c.ResolveFileProvider();
        });
    }

    public static IConfigurationBuilder AddTomlFile(this IConfigurationBuilder builder, Action<TomlFileConfigurationSource> configureSource)
    {
        return builder.Add(configureSource);
    }
}
