using BacapGenerator.Factories;
using BacapGenerator.Models.Datapacks.Settings;
using BacapGenerator.Services;
using Core.DataComponents;
using Core.Registries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UI.Configuration;

const string configFileName = "config.yaml";

const string embeddedConfigResourceName = "UI.default_config.yaml";

// Check config and fail-fast if it was missing
YamlConfigBootstrapper.EnsureConfigExists(configFileName, embeddedConfigResourceName);

// Build the host with DI and Configuration
var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.Sources.Clear();
        config.AddYamlFile(configFileName, optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        ComponentRegistry.RegisterAll();

        services.Configure<List<DatapackSettings>>(
            context.Configuration.GetSection("Datapacks"));

        var registryPath = context.Configuration.GetValue<string>("RegistryBasePath");

        services.AddSingleton<RegistryLoader>(_ => new RegistryLoader(registryPath!));
        services.AddSingleton<MinecraftData>();

        services.AddTransient<IDatapackFactory, DatapackFactory>();
        services.AddTransient<GeneratorAppService>();
    })
    .Build();

// Resolve the main service and run the application
var app = host.Services.GetRequiredService<GeneratorAppService>();
app.Run();