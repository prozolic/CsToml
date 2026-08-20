using CsToml.Extensions.Configuration;
using CsToml.Playground;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var httpClient = new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
};

try
{
    // Load the playground settings using CsToml.Extensions.Configuration itself.
    var toml = await httpClient.GetByteArrayAsync("appSettings.toml");
    var ms = new MemoryStream(toml);
    builder.Configuration.AddTomlStream(ms);
}
catch (Exception e)
{
    Console.WriteLine($"Failed to load appSettings.toml: {e}");
}

builder.Services.AddScoped(sp => httpClient);
builder.Services.AddFluentUIComponents();

await builder.Build().RunAsync();
