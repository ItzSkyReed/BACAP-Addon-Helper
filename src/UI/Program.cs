using System.Text;
using BacapGenerator.Configuration;
using BacapGenerator.Configuration.Exceptions;
using BacapGenerator.Datapacks.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UI.Configuration;
using UI.Diagnostics;
using UI.Menus;
using UI.Startup;
using UI.Styling;

Console.OutputEncoding = Encoding.UTF8;

const string configFileName = "config.yaml";
const string embeddedConfigResourceName = "UI.default_config.yaml";

// File pre-check
YamlConfigBootstrapper.EnsureConfigExists(configFileName, embeddedConfigResourceName);

// Host Building & Bootstrap
IHost host;

try
{
    host = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((_, config) =>
        {
            config.Sources.Clear();
            config.AddYamlFile(configFileName, optional: false, reloadOnChange: true);
            config.ExpandTemplates();
        })
        .ConfigureServices((context, services) =>
        {
            services.AddApplicationConfiguration(context.Configuration)
                .AddMinecraftRegistries(context.Configuration)
                .AddDatapackServices()
                .AddUiActions();
        })
        .Build();
}
catch (ConfigurationTemplateException ex)
{
    ConfigurationErrorHandler.RenderTemplateError(ex);
    TuiTheme.WaitForKey();
    return;
}
catch (Exception ex)
{
    ConfigurationErrorHandler.RenderBootstrapError(ex);
    TuiTheme.WaitForKey();
    return;
}

// Warmup & Run
using (host)
{
    var datapackLoader = host.Services.GetRequiredService<DatapackLoaderService>();
    datapackLoader.LoadAll();

    var mainMenu = host.Services.GetRequiredService<MainMenuAction>();
    await mainMenu.ExecuteAsync();
}