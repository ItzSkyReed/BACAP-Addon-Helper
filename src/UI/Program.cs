using System.Text;
using BacapGenerator.Configuration;
using BacapGenerator.Datapacks.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UI.Configuration;
using UI.Menus;
using UI.Startup;

// Environment & Pre-flight Bootstrap
Console.OutputEncoding = Encoding.UTF8;

const string configFileName = "config.yaml";
const string embeddedConfigResourceName = "UI.default_config.yaml";

YamlConfigBootstrapper.EnsureConfigExists(configFileName, embeddedConfigResourceName);

// Host Building & Dependency Registration
var host = Host.CreateDefaultBuilder(args)
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

// Domain Data Warmup
var datapackLoader = host.Services.GetRequiredService<DatapackLoaderService>();
datapackLoader.LoadAll();

// Execution Loop
var mainMenu = host.Services.GetRequiredService<MainMenuAction>();
await mainMenu.ExecuteAsync();