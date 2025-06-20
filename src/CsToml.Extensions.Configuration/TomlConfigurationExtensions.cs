using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace CsToml.Extensions.Configuration;

public static class TomlConfigurationExtensions
{
    public static IConfigurationBuilder AddTomlFile(this IConfigurationBuilder builder, string path)
        => AddTomlFile(builder, provider: null, path: path, optional: false, reloadOnChange: false);

    public static IConfigurationBuilder AddTomlFile(this IConfigurationBuilder builder, string path, bool optional)
        => AddTomlFile(builder, provider: null, path: path, optional: optional, reloadOnChange: false);

    public static IConfigurationBuilder AddTomlFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        => AddTomlFile(builder, provider: null, path: path, optional: optional, reloadOnChange: reloadOnChange);

    public static IConfigurationBuilder AddTomlFile(this IConfigurationBuilder builder, IFileProvider? provider, string path, bool optional, bool reloadOnChange)
        => AddTomlFile(builder, provider, path, optional, reloadOnChange, serializerOptions: null);

    public static IConfigurationBuilder AddTomlFile(this IConfigurationBuilder builder, IFileProvider? provider, string path, bool optional, bool reloadOnChange, CsTomlSerializerOptions? serializerOptions)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddTomlFile(c =>
        {
            c.FileProvider = provider;
            c.Path = path;
            c.Optional = optional;
            c.ReloadOnChange = reloadOnChange;
            c.SerializerOptions = serializerOptions;
            c.ResolveFileProvider();
        });
    }

    public static IConfigurationBuilder AddTomlFile(this IConfigurationBuilder builder, Action<TomlFileConfigurationSource> configureSource)
    {
        return builder.Add(configureSource);
    }

    public static IConfigurationBuilder AddTomlStream(this IConfigurationBuilder builder, System.IO.Stream stream)
        => AddTomlStream(builder, stream, serializerOptions: null);

    public static IConfigurationBuilder AddTomlStream(this IConfigurationBuilder builder, System.IO.Stream stream, CsTomlSerializerOptions? serializerOptions)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.Add<TomlStreamConfigurationSource>(s =>
        {
            s.Stream = stream;
            s.SerializerOptions = serializerOptions;
        });
    }
}
